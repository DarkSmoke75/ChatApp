using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Endpoint.App.Services.Authentication;
using Endpoint.App.Services.Navigations;
using Endpoint.App.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Endpoint.App.ViewModels.Settings
{
    public partial class SettingViewModel:ObservableObject
    {
        
        private readonly NavigationService _navigationService;
        public SettingViewModel(NavigationService navigationService)
        {
            _navigationService = navigationService;
           
        }
        [RelayCommand]
        private async Task Logout()
        {
            SecureStorage.Default.Remove("jwt_token");

            _navigationService.GoToLogin();
        }
    }
}
