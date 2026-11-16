using Endpoint.App.Models.Dtos;
using Endpoint.App.Services.Api;
using Endpoint.App.Services.Authentication;
using Microsoft.AspNetCore.SignalR.Client;

namespace Endpoint.App.Services.Conversation;

public sealed class ChatConnection : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly IDisposable _subscription;
    public event Func<MessageDto, Task>? MessageReceived;
    public event Func<Task>? Reconnected;
    public bool IsConnected => _connection.State == HubConnectionState.Connected;

    public ChatConnection(ApiClientService api, TokenService tokens)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(new Uri(api.BaseAddress, "/hubs/chat"), options =>
                options.AccessTokenProvider = async () => await tokens.GetTokenAsync())
            .WithAutomaticReconnect().Build();
        _subscription = _connection.On<MessageDto>("ReceiveMessage",
            message => MessageReceived?.Invoke(message) ?? Task.CompletedTask);
        _connection.Reconnected += _ => Reconnected?.Invoke() ?? Task.CompletedTask;
    }
    public Task StartAsync(CancellationToken token) => _connection.State == HubConnectionState.Disconnected
        ? _connection.StartAsync(token) : Task.CompletedTask;
    public Task StopAsync() => _connection.StopAsync();
    public async ValueTask DisposeAsync()
    {
        _subscription.Dispose();
        await _connection.DisposeAsync();
    }
}
