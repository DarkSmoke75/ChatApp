using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Endpoint.App.Services.Authentication;
using Endpoint.App.Services.Navigations;
using Endpoint.App.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Endpoint.App.ViewModels.Authentication
{
    public partial class LoginViewModel:ObservableObject
    {
        private readonly AuthService _authService;
        private readonly NavigationService _navigationService;
        [ObservableProperty] private string email;
        [ObservableProperty] private string password;
        [ObservableProperty] private string errorMessage;
        public LoginViewModel(AuthService authService, NavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }
        [RelayCommand]private async Task Login()
        {
            errorMessage ="";
            var loginResult = await _authService.Login(email, password);
            if (!loginResult)
            {
                ErrorMessage = "Login failed. Please check your credentials.";
                return;
            }
            _navigationService.GoToMainApp();
        }
        [RelayCommand]
        private async Task Register()
        {

            await _navigationService.GoToRegisterAsync();
        }
    }
}
