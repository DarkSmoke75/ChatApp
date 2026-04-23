using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Application.Services.ApiClient
{
    public interface IApiClientService
    {
        HttpClient CreateClientWithToken(HttpContext httpContext);
    }
    public class ApiClientService : IApiClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiClientService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public HttpClient CreateClientWithToken(HttpContext httpContext)
        {
            var client = _httpClientFactory.CreateClient("api");

            var token = httpContext.Request.Cookies["AccessToken"];

            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }
    }
}
