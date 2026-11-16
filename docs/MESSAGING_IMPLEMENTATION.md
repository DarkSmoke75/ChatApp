# Registration and reliable messaging implementation

## Changes

- Registration selects the seeded `Member` role. Usernames require 3–32 Unicode letters/numbers or underscores.
- History, send responses and `ReceiveMessage` use one server DTO: `messageId`, `conversationId`, `senderId`, `content`, `messageType`, `isEdited`, `sequenceNumber`, `clientRequestId`, `sentAt`. Transitional `userId` and `sendDate` aliases preserve existing readers.
- New messages persist UTC creation time. Unknown timestamps on historical rows are not invented or rewritten.
- Sends require a nonempty `clientRequestId` UUID. Retrying the same sender/request/content returns the same message. Reusing it for different content/conversation/type, or a deleted message, is rejected.
- SQL Server transaction-owned application locks serialize request IDs and sends per conversation across API instances. Unique database indexes independently enforce conversation/sequence and sender/request uniqueness. Locks time out after 10 seconds; retry uses the same ID.
- Sending/reading requires current membership (not soft-deleted, and `LeftDate` still its unset/default value). The API remains the only message-creation path.
- Live notifications include the sender's other sessions. Notification failure does not roll back or misreport a committed send. Clients recover from stored history.
- `afterSequence` returns ascending pages of new messages; `beforeSequence` returns older history. They cannot be combined. `take` must be 1–50.
- Browser and MAUI clients filter by conversation, merge by message ID and preserve sequence order. Only completed REST pages advance catch-up cursors, never out-of-order live events.
- Browser sending uses the antiforgery-protected MVC endpoint without reloading. Failed requests retain their payload/ID for retry. Pending text is read-only until confirmation or explicit validation rejection.
- MAUI now sends messages and displays pending/failed/sent states, loads older history, reconnects SignalR, and catches up on page activation/app resume. Visible chats also check history every 15 seconds to recover missed events, including initial connection failure.
- The mobile HTTP client sets authorization per request, avoiding mutable shared authorization headers during concurrent sending/recovery.

## Required rollout

1. Restore/build the existing .NET 9 API/web projects and selected MAUI target on a provisioned machine. This patch does not migrate frameworks or install dependencies.
2. Stop old API writers and back up the database. Deploy the API and both clients together: old send callers must be updated to include `clientRequestId`.
3. Review and apply migration `20261116090000_ReliableMessages` using the repository's EF Core 9 tool:

   ```powershell
   dotnet ef database update --project ChatApp.Persistance --startup-project Endpoint.Api
   ```

4. The migration adds a nullable request ID for historical compatibility and two unique indexes. It deliberately stops if existing messages have duplicate `(ConversationId, SequenceNumber)` values. Investigate/repair those in a backed-up staging copy; do not silently renumber live cursors. No migration has been executed by this task.
5. Validate on a disposable SQL Server database using the integration checks below before deploying to real users. Production clients need valid HTTPS/device connectivity; the existing Android address still requires environment configuration.

## Automated checks

The dependency-free harness targets the locally installed .NET 10 solely for tests; application projects stay on .NET 9. It compiles the actual domain/DTO/mobile timeline sources, checks their behavior, parses the repository C# syntax with the bundled compiler, and checks XAML XML structure. It is **not** a full application build or compiled-MAUI validation.

```powershell
$env:DOTNET_ROOT = "$PWD\.tools\dotnet"
$env:DOTNET_CLI_HOME = "$PWD\.tools\cli"
$env:NUGET_PACKAGES = "$PWD\.tools\packages"
& ./.tools/dotnet/dotnet.exe run --project tests/OfflineChecks -- .
& ./.tools/node/node-v26.13.0-win-x64/node.exe --test tests/live-message-filter.test.cjs tests/message-state.test.cjs
```

To run the real API tests, start a migrated API against a **disposable test database**. The tests create three users, one conversation and 17 stored messages. They intentionally leave the data available for inspection. Trust the development certificate through your environment; do not disable certificate validation.

```powershell
$env:CHATAPP_TEST_URL = 'https://localhost:7243'
& ./.tools/node/node-v26.13.0-win-x64/node.exe --test tests/api-messaging.integration.test.cjs
```

Without `CHATAPP_TEST_URL`, this test is skipped. It covers fresh registration, invalid usernames, unauthorized reads/sends, concurrent sequences, repeated-request deduplication, conflicting retries, paged catch-up, timestamp/contract consistency and invalid pagination.

Also verify: A has chats with B and C; C's events never enter A/B's screen. Disconnect A, send more than 50 messages, then reconnect and check each appears once. Delay a send response until its live event arrives, and verify one rendered message. Repeat from MAUI, suspend/resume it, switch accounts, and confirm no old-account connection remains. Test initial SignalR failure as well as a dropped established connection.

## Verification and remaining limits

Offline contract/timeline, browser-state and syntax checks passed during implementation. Full backend compilation, SQL migration/application-lock integration, live browser sessions and Android device tests remain unverified because this checkout lacks the required .NET 9 targeting/runtime and package dependencies.

Live delivery is best effort with history recovery, not a durable notification queue. Background push notifications, read receipts and persistent offline outbox storage are not implemented. Pending retry state survives transient failures/page suspension while the view remains alive, but not a browser reload, closing the chat or terminating the app. Browser logout still does not revoke an already issued API token. The previously committed signing key must be rotated before deployment.
