using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Endpoint.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    // Receive-only hub. Message creation goes through the participant-checked API.
    // SignalR's user mapping delivers to every connection for the authenticated user;
    // a separate single-connection presence dictionary would break multi-device use.
}
