using Endpoint.App.Models.Dtos;
using Endpoint.App.Services.Authentication;
using Endpoint.App.ViewModels.Authentication;

namespace Endpoint.App.Views
{
	public partial class LoginPage : ContentPage
	{
		public LoginPage(LoginViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}
		
        

    }
}