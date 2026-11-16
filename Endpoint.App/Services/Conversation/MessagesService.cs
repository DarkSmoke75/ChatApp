using Endpoint.App.Models.Dtos;
using Endpoint.App.Services.Api;
using System.Net;
using System.Net.Http.Json;

namespace Endpoint.App.Services.Conversation;

public class MessagesService
{
    private readonly ApiClientService _api;
    public MessagesService(ApiClientService api) { _api = api; }

    public async Task<List<MessageDto>> GetMessages(long conversationId, long? afterSequence = null,
        long? beforeSequence = null, CancellationToken cancellationToken = default)
    {
        using var response = await _api.GetAsync(
            $"/api/Messages/Get/{conversationId}?take=50&afterSequence={afterSequence}&beforeSequence={beforeSequence}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultDto<List<MessageDto>>>(cancellationToken: cancellationToken);
        if (result?.IsSuccess != true || result.Data == null)
            throw new InvalidOperationException(result?.Message ?? "Unable to load messages.");
        return result.Data;
    }

    public async Task<MessageDto> SendAsync(SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _api.PostAsync("/api/Messages/Send", request, cancellationToken);
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var error = await response.Content.ReadFromJsonAsync<ResultDto>(cancellationToken: cancellationToken);
            throw new MessageRejectedException(error?.Message ?? "Message rejected.");
        }
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ResultDto<MessageDto>>(cancellationToken: cancellationToken);
        if (result?.IsSuccess != true || result.Data == null)
            throw new InvalidOperationException("Message was not confirmed. Retry with the same request ID.");
        return result.Data;
    }
}
public class MessageRejectedException(string message) : Exception(message) { }
