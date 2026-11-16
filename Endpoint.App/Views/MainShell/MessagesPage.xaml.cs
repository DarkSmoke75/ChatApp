using Endpoint.App.ViewModels.Conversation;
using System.Threading.Tasks;

namespace Endpoint.App.Views.MainShell;

public partial class MessagesPage : ContentPage
{
    private readonly MessagesViewModel _viewModel;
	public MessagesPage(MessagesViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
        BindingContext= _viewModel;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (Window != null)
        {
            Window.Resumed += OnResumed;
            Window.Stopped += OnStopped;
        }
        await ActivateSafely();
    }

    protected override async void OnDisappearing()
    {
        if (Window != null)
        {
            Window.Resumed -= OnResumed;
            Window.Stopped -= OnStopped;
        }
        base.OnDisappearing();
        await DeactivateSafely();
    }

    private async void OnResumed(object? sender, EventArgs e) => await ActivateSafely();
    private async void OnStopped(object? sender, EventArgs e) => await DeactivateSafely();

    private async Task ActivateSafely()
    {
        try { await _viewModel.ActivateAsync(); }
        catch { _viewModel.StatusText = "Unable to connect. Reopen this chat to retry."; }
    }
    private async Task DeactivateSafely()
    {
        try { await _viewModel.DeactivateAsync(); }
        catch { _viewModel.StatusText = "Disconnected"; }
    }
}
