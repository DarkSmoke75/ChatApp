# Review and implementation roadmap

Implementation follow-up: registration, web sessions/antiforgery, messaging contracts,
transactional sends, reconnect recovery and MAUI sending have since been updated.
This document preserves the original review. See [current implementation and
verification status](MESSAGING_IMPLEMENTATION.md) before treating a finding below
as still unchanged.

Reviewed November 16, 2026. Scope: source code, configuration, migration, MVC client and MAUI client. This is a static review, not a verified runtime audit. The .NET CLI was unavailable, so builds, database migrations, tests and device/UI runs were not performed. This documentation change does not fix application behavior.

## Assessment

There is a substantial foundation: layered projects, persistence, validators, participant checks on persisted messages, browser history pagination and SignalR notifications. The app is a prototype with several connected flows, rather than release-ready messaging software.

Recommended first release: accounts, private text conversations, reliable history/live updates, usable web and one MAUI target, clear send failures and an operable deployment. Defer calls, channels and advanced media until this baseline works.

## Findings

Paths below are relative to the repository. Findings are supported by source inspection; runtime reproduction remains required.

| Priority | Evidence | Impact and action |
| --- | --- | --- |
| Blocker | `ChatApp.Application/Services/Users/Commands/RegisterUser/RegisterUserService.cs` looks for `User`; `ChatApp.Persistance/Contexts/DatabaseContext.cs` and the initial migration seed `Admin`/`Member`. | Fresh-database registration fails. Align the role definition and add a fresh-database test. |
| Critical before deployment | `Endpoint.Api/appsettings.json` contains a committed signing key. | Knowledge of the key can allow token forgery. Rotate it wherever used; require externally supplied secrets. Removing it from the latest file alone is insufficient. |
| High | `ChatApp.SignalR/Hubs/ChatHub.cs:SendMessage` accepts arbitrary recipient/conversation IDs without membership checks or persistence. | Authenticated callers can emit unpersisted messages to arbitrary users. Remove this path or delegate to the authorized persistence service. |
| High | `Endpoint.Api/Controllers/MessagesController.cs` and `UsersController.cs` lack `[Authorize]`; `UserContext.UserId` dereferences a claim directly. | Anonymous requests can throw instead of returning 401. Persisted-message participant checks do exist; retain them while enforcing endpoint authentication. |
| High | MVC configures cookie authentication, but login only writes `AccessToken`; no `SignInAsync` creates `ChatApp.Auth`. | The web authentication principal is not established. Implement session creation and authorization together; adding `[Authorize]` alone is insufficient. |
| High | MVC mutations have no enforced antiforgery validation; create/send actions lack verb restrictions and logout is GET. | Use POST for mutations and enforce antiforgery validation. Generated form tokens alone do not validate requests. |
| High | `SendMessageService` uses `MAX(SequenceNumber) + 1` with no unique conversation/sequence constraint. | Concurrent sends can duplicate sequence values and break history pagination. Use atomic allocation and database uniqueness. |
| High | `Endpoint.Site/Views/Conversation/GetChat.cshtml` renders every `ReceiveMessage` without checking conversation ID. | Another conversation's message can appear in the active thread. Filter events and deduplicate by message ID. |
| High | MAUI `MessagesPage.xaml` binds `MessageText`/`SendMessageCommand`, absent from `MessagesViewModel`; `MessagesService` only reads. | Mobile sending is unfinished despite the visible composer. Implement service, command and state. |
| Medium | History DTO has sequence/user fields but no message ID/time; send DTO has ID/time/sender but no sequence. | Reconciliation lacks a stable shared contract. Standardize REST/event payloads. |
| Medium | `BaseEntity.CreationDate` has no initializer; message creation/context do not populate it; send response uses separate `DateTime.Now`. | History timestamps can remain default and disagree with live events. Persist UTC once and return it. |
| Medium | `GetConversationsService` ignores cursor and sorts by conversation-local sequence. | Old busy threads can outrank recent ones; pagination repeats. Sort by activity plus a stable tie-breaker and implement matching cursors. |
| Medium | `CreateConversationService` performs private-chat reuse lookup for group requests too; participant roles come from the client. | Group creation can return a private chat. Restrict lookup and assign roles server-side. |
| Medium | Browser renders the HttpOnly token into JavaScript; hub auth explicitly handles cookies while client uses `accessTokenFactory` too. | HttpOnly isolation is lost by rendering the token. Choose a consistent session/hub design and test deployed origins/transports. |
| Medium | Database commit precedes notifications without durable retry; recipients exclude sender. | Failed notification can make a saved send look failed; sender's other sessions miss updates. Add idempotency, durable delivery and catch-up. |
| Medium | Hub uses a static, non-thread-safe dictionary storing one connection per user. | Presence does not safely support concurrent callbacks, multiple devices or instances. Remove unused tracking or replace it deliberately. |
| Medium | Hard-coded URLs/CORS and Android LAN HTTP address. | Device/deployment connectivity is not portable. Externalize configuration and use HTTPS. |

Other gaps: no automated tests/CI found; no refresh/revocation, recovery, upload, push-notification, read-receipt or end-to-end-encryption implementation found. User listing is unpaginated. Some MVC errors return views without their required models. The legacy SignalR server package (`1.2.9`) and repeated package dependencies need review during modernization.

## Phase 0 — Reproducible builds and configuration

1. Install the required SDK and build API/MVC separately, then one MAUI target. Record actual build failures before restructuring.
2. Upgrade to a supported framework, preferably a suitable LTS baseline; align EF Core, ASP.NET Core and MAUI dependencies. .NET 9 ended support November 10, 2026 ([Microsoft notice](https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/)). Verify MAUI platform support separately.
3. Pin the selected SDK with `global.json`; add a local EF tool manifest. Replace obsolete SignalR server packaging with the appropriate framework reference and remove unused dependencies.
4. Externalize addresses/origins, validate configuration at startup, replace tracked secrets with placeholders and rotate the signing key.
5. Create a fresh development database from migrations and follow the README from a clean checkout. Do not use production data for these checks.
6. Add GitHub Actions for builds and subsequent regression tests; separate MAUI jobs by platform.

**Completion gate:** a new contributor can build and create the database without your machine name/network address, and CI reproduces the builds.

## Phase 1 — Accounts and access control

1. Align registration with the seeded member role using shared constants. Test fresh registration and duplicate requests.
2. Implement real MVC cookie sign-in/sign-out, authorization and token-expiry handling. Use a deliberate server-managed token/session design; remove token rendering if using a server-side proxy/session approach.
3. Protect users/messages endpoints and MVC conversation routes. Return 401 for missing/expired identity, and intentional 403 or non-disclosing 404 for unauthorized resources. Make identity access safe.
4. Restrict mutations to POST and enforce antiforgery validation. Populate error models instead of rendering null-dependent views.
5. Remove unchecked hub sending. Verify membership through the same use case for every send. Define access for disabled users, deleted memberships and participants who leave.
6. Validate usernames, normalize account identifiers, add race-safe uniqueness handling, and bound message/pagination inputs. Review hashing through a maintained identity component with migration for existing hashes.
7. Add authentication throttling, refresh-token rotation/revocation, recovery and verification before broad self-service registration.

**Tests/gate:** A and B register on a fresh database and chat. Anonymous requests return 401. C cannot read/send into A/B's conversation through REST or the hub. Missing antiforgery tokens fail. Logout and expiry behave consistently.

## Phase 2 — Correct, durable messages

1. Define a canonical message DTO containing ID, conversation ID, sender ID, content/type, sequence and persisted UTC timestamp. Use it for history, send responses and events.
2. Allocate sequences atomically within a transaction and enforce unique `(ConversationId, SequenceNumber)`. Add bounded contention handling rather than relying on `MAX + 1`.
3. Accept a client-generated request ID with a uniqueness constraint per sender/request. Retrying after a timeout returns the saved message instead of creating another.
4. Persist timestamps, order conversations by last activity and stable ID, and implement a cursor with the same ordering values. Define ordering for empty conversations.
5. Validate positive bounded `take` and cursors. Add participant, normalized username and message-status uniqueness where appropriate; clean existing duplicates before constraint migrations.
6. Restrict private-chat reuse to private requests, validate target users and enforce pair uniqueness under concurrent creation. Keep group UI deferred until permissions work.
7. Save an outbox event in the same transaction as the message; dispatch with retries. A temporary notification failure must not turn a committed send into an ambiguous failure.

**Tests/gate:** concurrent sends have unique ordered sequences, retries create one message, pagination has no missing/duplicate rows, timestamps survive reload, concurrent pair creation produces one private chat, and group requests do not reuse private chats.

## Phase 3 — Reliable live updates

1. Filter events by conversation; update other threads' previews/unread state separately.
2. Deduplicate by message ID, order by sequence and reconcile pending sends by client request ID. Notify the sender's other sessions too.
3. Add an authenticated `afterSequence` catch-up query; the current endpoint only supports older history. Fetch missed messages after reconnect, focus and app resume.
4. Handle the initial-history/subscription race by buffering events and merging by ID. Retry initial connection failure separately from automatic reconnect.
5. Choose a consistent browser hub authentication model. If using query bearer tokens, accept them only on the hub path and redact logs; if cookies, validate scope/origins and deployment topology. Use HTTPS.
6. Manage multiple connections and cleanup on logout. Add distributed presence/backplane only when multiple instances are required.

**Tests/gate:** C's message does not appear in A/B's open thread; offline messages appear exactly once after reconnect; two sessions for A converge; token expiry and initial connection failures offer recovery.

## Phase 4 — Usable web experience

1. Show the current user, correct login state and participant names instead of internal thread IDs.
2. Add own/other message alignment, readable local timestamps, responsive layout, keyboard sending and accessible focus/labels. Test Persian/RTL alongside English.
3. Send without full-page navigation. Show pending/sent/failed states, preserve drafts and retry with the same request ID.
4. Add consistent loading/empty/error states, expiry redirects, paginated user search and conversation continuation.
5. Persist read positions and unread counts. Distinguish saved, delivered and read; existing status rows marked `Sent` do not prove receipt.
6. Move inline chat behavior into a dedicated module. Test long text, emoji, fast sends, empty threads and safe text rendering.

**Completion gate:** two non-developers can register, find each other, chat, reload, understand unread state and recover from send failures without assistance.

## Phase 5 — Complete one MAUI platform

1. Choose Android or Windows first and configure reachable HTTPS with valid trust on that target.
2. Implement `MessageText`, `SendMessageCommand`, send DTO/service call, busy state, draft preservation and retry/error UI.
3. Add a SignalR client service using secure token storage and shared contracts. Implement reconnect catch-up, UI-thread collection updates and connection disposal.
4. Implement user discovery/new chat and pagination. Correct the `curosr` typo; clear or merge collections on reload to avoid duplicate entries.
5. Observe async history-loading failures from `ApplyQueryAttributes` rather than discarding its task. Check HTTP failures before parsing JSON and show useful errors.
6. If offline sending is in scope, add per-account local cache/outbox with stable retry IDs.
7. Add push notifications for background/closed-app delivery, device-token registration/revocation and privacy-aware previews. Foreground SignalR is insufficient.
8. Test logout/account switching, suspend/resume, network loss, notification deep links, keyboard behavior and secure-storage failures. Verify each additional platform separately.

**Completion gate:** web and mobile users exchange messages, restart apps and recover history reliably on a real supported target.

## Phase 6 — Private beta and operations

1. Deploy staging API/web/SQL using external secrets, valid TLS, actual domain/origin configuration and WebSocket support.
2. Add exception handling, structured logs with correlation IDs, readiness/health checks and metrics for failures, send latency, reconnects and outbox backlog. Keep tokens and message bodies out of routine logs.
3. Automate deployment and reviewed migrations. Verify backup restoration and document rollback; successful backup jobs alone do not prove recoverability.
4. Define deletion, retention, blocking/reporting, abuse handling and support contact. State clearly that server-stored messages are not end-to-end encrypted.
5. Run SQL Server integration tests, browser end-to-end tests, device tests and load tests against a declared beta target. Record results rather than claiming unspecified scale.
6. Invite a small group, monitor failed sends/crashes and resolve reliability issues before wider access. Choose a license and complete app metadata before public distribution.

**Completion gate:** the team can deploy, observe failures, restore data, revoke access and support real testers.

## Later features

- Attachments: authorized object storage, limits, validation/scanning, thumbnails and cleanup.
- Groups: owner/admin roles, invitations, removal/leave rules and membership history.
- Editing, deletion, replies and search: stable IDs, retention rules and synchronized events.
- Calls: separate WebRTC/signaling/TURN design and mobile lifecycle work.
- End-to-end encryption: dedicated protocol/key-management design covering devices, verification, recovery and backups; TLS alone is not equivalent.

## First issue queue

1. Establish baseline builds/CI and plan framework upgrade.
2. Rotate signing key and externalize configuration.
3. Fix registration role with a fresh-database test.
4. Close unchecked hub sending and add authorization regression tests.
5. Complete MVC session, POST/antiforgery and expiry handling.
6. Unify message DTO/timestamps and filter live events.
7. Add atomic sequences, uniqueness and idempotency.
8. Implement conversation pagination and reconnect catch-up.
9. Finish web send/error states, then MAUI sending/live updates.
10. Establish staging, restore verification and beta release gates.

Implement these as focused, reviewable changes. Each behavior fix should include reproduction steps and a regression check. The root planning text files remain historical notes; this roadmap consolidates them with the source findings above.
