using Endpoint.App.Services.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Endpoint.App.Services.Api
{
    public class ApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;
        public ApiClientService(HttpClient httpClient,TokenService tokenService)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
        }
        private async Task AddTokenAsync()
        {
            var token = await _tokenService.GetTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            await AddTokenAsync();
            var token = await _tokenService.GetTokenAsync();
            return await _httpClient.GetAsync(url);
        }
        public async Task<HttpResponseMessage> PostAsync<T>(string url,T data)
        {
            await AddTokenAsync();

            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync(url, content);
        }
    }
}
