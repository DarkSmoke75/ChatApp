# Access-control regression checks

These integration checks require the API, database and valid test accounts. They
have not been executed as part of the dependency-free source changes.

1. Without a token, call GET `/api/Users/GetUsers`, GET
   `/api/Messages/Get/1?take=20`, and POST `/api/Messages/Send` with an otherwise
   valid body. Expect HTTP 401, with no database writes or notifications.
2. Repeat with expired/invalid tokens. Expect HTTP 401. Using test-only signing
   configuration, test missing, nonnumeric, zero and negative name-identifier
   claims in otherwise valid tokens: all must return 401.
3. Create a private conversation between A and B. With A's token, send through
   the API and fetch history. Confirm persistence and delivery to B still work.
4. With C's token, attempt to read/send into A/B's conversation. Expect a failure
   result, no message content, no new database row and no notification. Send
   failures now return HTTP 400 with the result envelope.
5. Connect an authenticated SignalR client and invoke `SendMessage` with A/B's
   conversation and an arbitrary recipient. Expect unknown-method failure and
   no event. Anonymous hub negotiation must fail authentication.
6. Open A/B's chat while C sends to a separate A/C conversation. A/B's message
   list, scroll and feedback must remain unchanged. B's messages must appear.

Run the dependency-free browser-handler checks with:

```powershell
& ./.tools/node/node-v26.13.0-win-x64/node.exe --test tests/live-message-filter.test.cjs
```

The Node checks execute the view's actual event callback; they do not verify
ASP.NET authorization or database behavior. Signing-key rotation and live-event
deduplication remain separate follow-ups.

## MVC sessions and antiforgery

These checks also require the running web client and API; they have not yet been
executed. Existing token-only browser sessions must log in again after this change.

1. Visit `/Conversation/Index` and `/Home/Index` anonymously. Expect a redirect to
   login. A browser containing only an `AccessToken` cookie must not count as a
   signed-in MVC user.
2. Log in through the web form. Expect a protected `ChatApp.Auth` cookie, the
   username and logout button in navigation, and access to conversations. Failed
   login must not create a new session.
3. Verify the auth ticket and access-token cookie expire with the token's `exp`;
   requests must not extend the session beyond that time. Remove or replace the
   access-token cookie: the protected web session must be rejected next request.
4. POST login, sign-up, create-chat, send-message and logout without a valid
   antiforgery field/cookie pair. Expect HTTP 400 with no action performed. Test
   a token from a different browser session as well.
5. Submit each corresponding web form normally. Expect success, including user
   cards creating chats and the header logout button signing out.
6. GET `/Conversation/CreateChat?userId=2`, `/Conversation/SendMessage`, and
   `/Authentication/Logout`. Expect 405/no mutation. Logout requires POST.
7. Log out and revisit a protected page. Expect a login redirect, both auth/token
   cookies removed and session data cleared. Previously captured API tokens are
   not revoked by browser logout; token revocation is a separate backend feature.
8. Force an API 401 while the MVC ticket is still valid (for example rotate the
   test API signing key). Each conversation action should clear the session and
   redirect to login rather than render a broken view or parse an error as JSON.
9. Confirm API/mobile bearer requests still work without MVC antiforgery tokens;
   the global antiforgery filter is installed in the web project only.

Session identity is read only from a successful server-to-server API login
response. Browser-provided JWTs are never decoded to establish an MVC session.
The protected auth ticket binds the accompanying access-token cookie to that
login. The existing browser SignalR token delivery remains unchanged.
