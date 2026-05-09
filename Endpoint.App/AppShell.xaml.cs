using Endpoint.App.Services.Authentication;
using Endpoint.App.Views;
using Endpoint.App.Views.MainShell;

namespace Endpoint.App
{
    public partial class AppShell : Shell
    {
      
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("SignUp", typeof(RegisterPage));
            Routing.RegisterRoute("Login", typeof(LoginPage));
            Routing.RegisterRoute("Messages", typeof(MessagesPage));
          
        }
        
        
    }
}
