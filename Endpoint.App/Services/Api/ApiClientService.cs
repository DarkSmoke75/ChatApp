using Endpoint.App.Services.Authentication;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Endpoint.App.Services.Api;

public class ApiClientService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;
    public Uri BaseAddress => _httpClient.BaseAddress!;
    public ApiClientService(HttpClient httpClient, TokenService tokenService)
    { _httpClient = httpClient; _tokenService = tokenService; }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using (request)
        {
            var token = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await _httpClient.SendAsync(request, cancellationToken);
        }
    }
    public Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default) =>
        SendAsync(new HttpRequestMessage(HttpMethod.Get, url), cancellationToken);
    public Task<HttpResponseMessage> PostAsync<T>(string url, T data, CancellationToken cancellationToken = default) =>
        SendAsync(new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(data) }, cancellationToken);
}
