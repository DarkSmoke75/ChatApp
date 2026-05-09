using Endpoint.App.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Endpoint.App.Services.Navigations
{
    public class NavigationService
    {

        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private Window GetWindow()
        {
            return Application.Current.Windows[0];
        }

        public void GoToLogin()
        {
            GetWindow().Page = new NavigationPage(
                _serviceProvider.GetRequiredService<LoginPage>()
            );
        }

        public void GoToMainApp()
        {
            GetWindow().Page =
                _serviceProvider.GetRequiredService<AppShell>();
        }

        public async Task GoToRegisterAsync()
        {
            var navPage = GetWindow().Page as NavigationPage;

            if (navPage == null)
                throw new Exception("Current page is not NavigationPage");

            await navPage.Navigation.PushAsync(
                _serviceProvider.GetRequiredService<RegisterPage>()
            );
        }
    }
}
