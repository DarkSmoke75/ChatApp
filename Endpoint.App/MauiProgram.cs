using Endpoint.App.Services.Api;
using Endpoint.App.Services.Authentication;
using Endpoint.App.Services.Conversation;
using Endpoint.App.Services.Navigations;
using Endpoint.App.ViewModels.Authentication;
using Endpoint.App.ViewModels.Conversation;
using Endpoint.App.ViewModels.Settings;
using Endpoint.App.Views;
using Microsoft.Extensions.Logging;

namespace Endpoint.App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddScoped<LoginViewModel>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SettingViewModel>();
            builder.Services.AddTransient<SignUpViewModel>();
            builder.Services.AddTransient<NavigationService>();
            builder.Services.AddTransient<ConversationsService>();
            builder.Services.AddSingleton<TokenService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddTransient<ConversationViewModel>();
            builder.Services.AddTransient<MessagesService>();
            builder.Services.AddTransient<MessagesViewModel>();
            builder.Services.AddHttpClient<ApiClientService>(client =>
            {
#if ANDROID
                client.BaseAddress = new Uri("http://192.168.137.1:5201");
#else
                client.BaseAddress = new Uri("https://localhost:7243");
#endif
            });
            

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
