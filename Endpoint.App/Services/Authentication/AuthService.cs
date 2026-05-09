using Endpoint.App.Models.Dtos;
using Endpoint.App.Services.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Endpoint.App.Services.Authentication
{
    public class AuthService
    {
        private readonly ApiClientService _apiService;
        private readonly TokenService _tokenService;
        public AuthService(ApiClientService apiClientService, TokenService tokenService)
        {
            _apiService = apiClientService;
            _tokenService = tokenService;
        }
        public async Task<bool> Login(string email, string password)
        {
            var request = new LoginRequestDto
            {
                Email = email,
                Password = password
            };
            var response = await _apiService.PostAsync("/api/Authentication/Login", request);

            //response.EnsureSuccessStatusCode();
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            var token = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            if (token == null || string.IsNullOrWhiteSpace(token.Data))
                return false;
            await _tokenService.SaveTokenAsync(token.Data);
            return true;
        }
        public async Task<bool> SignUpAsync(SignUpRequestDto request)
        {
            var response = await _apiService.PostAsync("/api/Authentication/SignUp", request);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            return true;
        }
    }
}
