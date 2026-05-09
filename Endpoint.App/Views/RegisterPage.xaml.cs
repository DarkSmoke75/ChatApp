using Endpoint.App.ViewModels.Authentication;

namespace Endpoint.App.Views;

public partial class RegisterPage : ContentPage
{
	public RegisterPage(SignUpViewModel signUpViewModel)
	{
		InitializeComponent();
		BindingContext=signUpViewModel;
	}
}