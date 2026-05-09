using Endpoint.App.ViewModels.Conversation;

namespace Endpoint.App.Views.MainShell;

public partial class ChatsPage : ContentPage
{
    private readonly ConversationViewModel _viewModel;
	public ChatsPage(ConversationViewModel viewModel)
	{
		InitializeComponent();
        _viewModel = viewModel;
		BindingContext = _viewModel;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.GetConversationsCommand.ExecuteAsync(null);
    }
}