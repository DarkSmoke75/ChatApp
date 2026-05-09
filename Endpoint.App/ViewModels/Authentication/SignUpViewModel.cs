using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Endpoint.App.Models.Dtos;
using Endpoint.App.Services.Authentication;
using Endpoint.App.Services.Navigations;
using Endpoint.App.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Endpoint.App.ViewModels.Authentication
{
    public partial class SignUpViewModel: ObservableObject
    {
        private readonly AuthService _authService;
        private readonly NavigationService _navigationService;
        [ObservableProperty] private string email;
        [ObservableProperty] private string password;
        [ObservableProperty] private string rePassword;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private string username;
        [ObservableProperty] private string displayName;
        public SignUpViewModel(AuthService authService, NavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }
        [RelayCommand]private void BackToLogin()
        {
            _navigationService.GoToLogin();
        }
        [RelayCommand]private async Task SignUp()
        {
            errorMessage = "";
            var request = new SignUpRequestDto
            {
                Email = email,
                Password = password,
                RePassword = rePassword,
                Username = username,
                DisplayName = displayName
            };
            var signUpResult = await _authService.SignUpAsync(request);
            if (!signUpResult)
            {
                ErrorMessage = "Sign Up failed.";
                return;
            }
            _navigationService.GoToLogin();
        }
    }
}
