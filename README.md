# ChatApp

A chat application built with ASP.NET Core, SQL Server, SignalR, and .NET MAUI. It includes a shared backend, an MVC web client, and a mobile/desktop client in progress.

**Status: development prototype.** Core chat flows exist in source, but registration on a fresh database, authentication consistency, message reliability, and mobile sending need work before release. See the [detailed review and roadmap](docs/PROJECT_ROADMAP.md).

## Current features

| Area | Implemented in source | Remaining work |
| --- | --- | --- |
| Accounts | Registration/login, password hashing, JWT issuance | Seeded-role fix, consistent sessions, recovery and refresh |
| Conversations | Private-chat creation/reuse, listing, group data model | Ordering, pagination, group permissions |
| Messages | SQL persistence, participant checks, sequence-based history | Concurrent sends, retry safety, consistent contracts |
| Live updates | SignalR hub, notifications and browser receiver | Conversation filtering, reconnect catch-up, secure send path |
| Web | Account forms, user directory, conversations, composer/history | Authentication state, error handling, smoother sending |
| MAUI | Authentication, secure token storage, conversation/history screens | Sending, live updates, new-chat flow, device configuration |

Media enum values and status entities do not imply working uploads, calls, or read receipts. End-to-end encryption is not implemented.

## Architecture

```text
MVC web / MAUI clients --> ASP.NET Core API --> Application services --> SQL Server
                                  |
                            SignalR events --> clients
```

| Project | Responsibility |
| --- | --- |
| `Endpoint.Api` | REST controllers, JWT validation, Swagger, hub hosting |
| `Endpoint.Site` | MVC web client calling the API |
| `Endpoint.App` | MAUI client with CommunityToolkit.Mvvm |
| `ChatApp.Application` | Use cases, validators, DTOs, interfaces, JWT generation |
| `ChatApp.Domain` | Users, conversations, participants, messages and statuses |
| `ChatApp.Persistance` | EF Core context and migrations; spelling matches the repository |
| `ChatApp.SignalR` | Chat hub |
| `ChatApp.Infrastructure` | SignalR notification implementation |
| `ChatApp.Common` | Shared results, password hashing and helpers |

## Local development

### 1. Prerequisites and build

Install a .NET SDK/runtime capable of building and running the current `net9.0` projects, plus SQL Server or SQL Server Express/LocalDB. MAUI additionally needs matching workloads and platform SDKs; Apple targets require the appropriate Mac/Xcode environment.

.NET 9 reached end of support on November 10, 2026. Upgrade to a supported release before deployment ([Microsoft lifecycle notice](https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/)). These instructions describe the current checkout, not a completed upgrade.

From the repository root, build API and web separately so MAUI workloads do not block backend work:

```powershell
dotnet --info
dotnet restore Endpoint.Api/Endpoint.Api.csproj
dotnet restore Endpoint.Site/Endpoint.Site.csproj
dotnet build Endpoint.Api/Endpoint.Api.csproj --no-restore
dotnet build Endpoint.Site/Endpoint.Site.csproj --no-restore
dotnet dev-certs https --trust
```

### 2. Configure the API

The checked-in API configuration contains a machine-specific database address and a JWT signing key. Treat that key as exposed, rotate it wherever used, and keep replacement secrets outside Git.

For local development, set these variables in the PowerShell window used for migrations and API startup:

```powershell
# Example for Windows LocalDB; replace with your installed SQL Server instance.
$env:ConnectionStrings__DefaultConnection = 'Server=(localdb)\MSSQLLocalDB;Database=ChatAppDev;Trusted_Connection=True;TrustServerCertificate=True'
$keyBytes = New-Object byte[] 64
$keyGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$keyGenerator.GetBytes($keyBytes)
$env:Jwt__Key = [Convert]::ToBase64String($keyBytes)
$keyGenerator.Dispose()
$env:Jwt__Issuer = 'ChatApp'
$env:Jwt__Audience = 'ChatAppUsers'
$env:Jwt__ExpireMinutes = '60'
```

These settings last only for this shell. Regenerating the key invalidates old tokens. Use a persistent secret store for a stable environment. `TrustServerCertificate=True` above is a local-development setting.

### 3. Create the development database

Install the EF tool if absent; use the version compatible with the project's EF Core version:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.15
dotnet ef database update --project ChatApp.Persistance/ChatApp.Persistance.csproj --startup-project Endpoint.Api/Endpoint.Api.csproj
```

**Known first-run blocker:** the migration seeds `Admin` and `Member`, but `RegisterUserService` searches for `User`. Align registration with the intended member role before testing sign-up. There is no seeded demo account.

### 4. Start the API and web client

In the configured API terminal:

```powershell
dotnet run --project Endpoint.Api/Endpoint.Api.csproj --launch-profile https
```

In a second terminal:

```powershell
dotnet run --project Endpoint.Site/Endpoint.Site.csproj --launch-profile https
```

- Web: [https://localhost:7105](https://localhost:7105)
- Swagger: [https://localhost:7243/swagger](https://localhost:7243/swagger)
- Hub: `https://localhost:7243/hubs/chat`; event: `ReceiveMessage`

The web API address is hard-coded in `Endpoint.Site/Program.cs`, the hub address in `Endpoint.Site/Views/Conversation/GetChat.cshtml`, and the allowed web origin in `Endpoint.Api/Program.cs`. Keep them consistent when changing ports. API launch profiles also bind non-loopback interfaces; restrict them to localhost for isolated development.

### 5. MAUI client

Open `Endpoint.App` with a matching MAUI workload and select a target. Android, iOS, Mac Catalyst and Windows are declared; these platforms have not been verified by this review.

`Endpoint.App/MauiProgram.cs` uses a fixed LAN HTTP address for Android and `https://localhost:7243` otherwise. Configure an address reachable from your device/emulator with valid HTTPS trust. A physical device's localhost means the device itself.

The mobile message composer currently binds to an unimplemented send command. Follow roadmap phase 5 before expecting two-way mobile chat.

## API overview

| Method | Route | Purpose |
| --- | --- | --- |
| POST | `/api/Authentication/SignUp` | Register and return JWT |
| POST | `/api/Authentication/Login` | Authenticate and return JWT |
| GET | `/api/Users/GetUsers` | List other users |
| GET | `/api/Conversations/Get?take=20` | List conversations; cursor currently ignored |
| POST | `/api/Conversations/Create` | Create/reuse conversation |
| GET | `/api/Messages/Get/{id}?take=20&beforeSequence=100` | History; omit cursor for newest messages |
| POST | `/api/Messages/Send` | Persist message and notify recipients |

Use `Authorization: Bearer <token>` for protected REST calls. Messages/users lack controller-level authorization even though their services require an identity; this is a defect, not anonymous API support. Error bodies and HTTP status codes need standardization.

## Verification and contributing

Source review completed November 16, 2026. `dotnet --info` failed because the CLI was unavailable. Builds, migrations, UI behavior and device runs were not verified. No automated test project or GitHub Actions workflow was found.

After the initial fixes, use two browser profiles to register users, create a chat, exchange messages, reload, paginate history and reconnect. Use a third account to check access denial. The [roadmap](docs/PROJECT_ROADMAP.md) includes release gates and regression checks.

Keep changes focused and include reproduction steps and regression coverage for behavior fixes. Never commit credentials, tokens, database exports or personal messages.

## License

No license file is included. The owner should choose a license before presenting this repository as open source.
