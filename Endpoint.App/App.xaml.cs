using Endpoint.App.Services.Authentication;
using Endpoint.App.Views;

namespace Endpoint.App
{
    public partial class App : Application
    {
        private readonly TokenService _tokenService;
        private readonly IServiceProvider _serviceProvider;
        public App(TokenService tokenService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _tokenService = tokenService;
            _serviceProvider = serviceProvider;
            MainPage = new ContentPage();
        }

        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
        protected override async void OnStart()
        {
            base.OnStart();
            var token = await _tokenService.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                MainPage = new NavigationPage(_serviceProvider.GetRequiredService<LoginPage>());
            }
            else
            {
                MainPage = _serviceProvider.GetRequiredService<AppShell>();

            }
        }
    }
}