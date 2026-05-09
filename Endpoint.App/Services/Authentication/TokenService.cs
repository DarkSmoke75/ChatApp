using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Endpoint.App.Services.Authentication
{
    public class TokenService
    {
        private const string TokenKey = "jwt_token";
        public async Task<string> GetTokenAsync()
        {
            return await SecureStorage.Default.GetAsync(TokenKey);
        }
        public void RemoveToken()
        {
            SecureStorage.Default.Remove(TokenKey);
        }
        public async Task SaveTokenAsync(string token)
        {
            await SecureStorage.Default.SetAsync(TokenKey, token);
        }
    }
}
