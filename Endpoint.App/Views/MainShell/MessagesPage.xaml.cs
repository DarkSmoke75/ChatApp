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
  
}