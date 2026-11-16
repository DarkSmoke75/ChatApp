using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Endpoint.App.Models.Dtos;
using Endpoint.App.Services.Conversation;
using System.Collections.ObjectModel;

namespace Endpoint.App.ViewModels.Conversation;

public partial class MessagesViewModel : ObservableObject, IQueryAttributable
{
    private readonly MessagesService _messages;
    private readonly ChatConnection _connection;
    private readonly SemaphoreSlim _lifecycle = new(1, 1);
    private readonly SemaphoreSlim _recovery = new(1, 1);
    private CancellationTokenSource? _active;
    private Task? _poll;
    private MessageTimeline _timeline = new(0);
    private SendMessageRequest? _pending;
    private bool _loaded;
    public ObservableCollection<MessageDto> Messages => _timeline.Messages;
    [ObservableProperty] private string messageText = "";
    [ObservableProperty] private string statusText = "Ready";
    [ObservableProperty] private bool canEditMessage = true;
    [ObservableProperty] private bool isSending;
    [ObservableProperty] private string sendButtonText = "Send";

    public MessagesViewModel(MessagesService messages, ChatConnection connection)
    {
        _messages = messages;
        _connection = connection;
        _connection.MessageReceived += message => MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (_active == null) return;
            _timeline.Merge(message);
            ConfirmPending(message);
        });
        _connection.Reconnected += () => RecoverAsync();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("ConversationId", out var value) ||
            !long.TryParse(value?.ToString(), out var id) || id <= 0)
        { StatusText = "Invalid conversation."; return; }
        if (_timeline.ConversationId == id) return;
        _timeline = new MessageTimeline(id);
        _loaded = false;
        _pending = null;
        MessageText = "";
        CanEditMessage = true;
        OnPropertyChanged(nameof(Messages));
    }

    public async Task ActivateAsync()
    {
        await _lifecycle.WaitAsync();
        try
        {
            if (_active != null || _timeline.ConversationId <= 0) return;
            _active = new CancellationTokenSource();
            await RecoverAsync();
            _poll = PollAsync(_active.Token);
        }
        finally { _lifecycle.Release(); }
    }

    public async Task DeactivateAsync()
    {
        await _lifecycle.WaitAsync();
        try
        {
            var active = _active;
            if (active == null) return;
            active.Cancel();
            if (_poll != null) await _poll;
            try { await _connection.StopAsync(); }
            catch { StatusText = "Disconnected"; }
            _active = null;
            active.Dispose();
        }
        finally { _lifecycle.Release(); }
    }

    private async Task PollAsync(CancellationToken token)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
            while (!token.IsCancellationRequested)
            {
                try { await _connection.StartAsync(token); }
                catch (OperationCanceledException) when (token.IsCancellationRequested) { break; }
                catch { await MainThread.InvokeOnMainThreadAsync(() => StatusText = "Live updates reconnecting; checking history."); }
                await RecoverAsync();
                if (!await timer.WaitForNextTickAsync(token)) break;
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }

    private async Task RecoverAsync()
    {
        var active = _active;
        if (active == null || active.IsCancellationRequested || !await _recovery.WaitAsync(0)) return;
        var timeline = _timeline;
        try
        {
            if (!_loaded)
            {
                var initial = await _messages.GetMessages(timeline.ConversationId, cancellationToken: active.Token);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    timeline.ApplyPage(initial);
                    foreach (var message in initial) ConfirmPending(message);
                    _loaded = true;
                });
            }
            for (;;)
            {
                var page = await _messages.GetMessages(timeline.ConversationId, timeline.RecoveryCursor,
                    cancellationToken: active.Token);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    timeline.ApplyPage(page);
                    foreach (var message in page) ConfirmPending(message);
                });
                if (page.Count < 50) break;
            }
        }
        catch (OperationCanceledException) when (active.IsCancellationRequested) { }
        catch { await MainThread.InvokeOnMainThreadAsync(() => StatusText = "Could not refresh messages. Reconnecting automatically."); }
        finally { _recovery.Release(); }
    }

    [RelayCommand]
    private async Task LoadOlder()
    {
        var first = Messages.Where(m => m.MessageId > 0).Select(m => m.SequenceNumber).DefaultIfEmpty(0).Min();
        if (first <= 0) return;
        try
        {
            var page = await _messages.GetMessages(_timeline.ConversationId, beforeSequence: first);
            foreach (var message in page) _timeline.Merge(message);
            StatusText = page.Count == 0 ? "No older messages" : "Older messages loaded";
        }
        catch { StatusText = "Could not load older messages. Try again."; }
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (IsSending || _timeline.ConversationId <= 0 || (_pending == null && string.IsNullOrWhiteSpace(MessageText))) return;
        _pending ??= new SendMessageRequest(_timeline.ConversationId, MessageText, MessageType.Text, Guid.NewGuid());
        var request = _pending;
        IsSending = true;
        CanEditMessage = false;
        StatusText = "Sending…";
        ShowPending(request, "Sending");
        try
        {
            var saved = await _messages.SendAsync(request);
            _timeline.Merge(saved);
            ConfirmPending(saved);
        }
        catch (MessageRejectedException exception)
        {
            ShowPending(request, "Rejected");
            _pending = null;
            CanEditMessage = true;
            StatusText = exception.Message;
            SendButtonText = "Send";
        }
        catch
        {
            // A live event/catch-up may already have confirmed a timed-out HTTP send.
            if (_pending?.ClientRequestId == request.ClientRequestId)
            {
                ShowPending(request, "Failed — retry");
                StatusText = "Send not confirmed. Retry keeps the same message ID.";
                SendButtonText = "Retry";
            }
        }
        finally { IsSending = false; }
    }

    private void ShowPending(SendMessageRequest request, string state) => _timeline.Merge(new MessageDto
    {
        ConversationId = request.ConversationId, Content = request.Content,
        ClientRequestId = request.ClientRequestId, SentAt = DateTime.UtcNow, DeliveryState = state
    });

    private void ConfirmPending(MessageDto message)
    {
        if (message.MessageId <= 0 || _pending == null || message.ClientRequestId != _pending.ClientRequestId ||
            message.ConversationId != _pending.ConversationId) return;
        _pending = null;
        MessageText = "";
        CanEditMessage = true;
        SendButtonText = "Send";
        StatusText = "Sent";
    }
}
