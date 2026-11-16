const { test } = require('node:test');
const assert = require('node:assert/strict');
const { randomUUID, randomBytes } = require('node:crypto');

// Opt-in: creates three accounts and messages. Point only at a disposable database.
const base = process.env.CHATAPP_TEST_URL;
test('registration, authorization, concurrent sends, retries and paged recovery',
    { skip: !base, timeout: 120000 }, async () => {
    async function call(route, token, body) {
        const response = await fetch(new URL(route, base), {
            method: body === undefined ? 'GET' : 'POST',
            headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}),
                ...(body === undefined ? {} : { 'Content-Type': 'application/json' }) },
            body: body === undefined ? undefined : JSON.stringify(body),
            signal: AbortSignal.timeout(20000)
        });
        const text = await response.text();
        let value;
        try { value = JSON.parse(text); } catch { value = text; }
        return { status: response.status, value };
    }
    async function ok(route, token, body) {
        const response = await call(route, token, body);
        assert.equal(response.status, 200, `${route} returned ${response.status}`);
        assert.equal(response.value.isSuccess, true, response.value.message);
        return response.value.data;
    }
    const suffix = randomBytes(6).toString('hex');
    const password = randomBytes(16).toString('hex') + 'A!2';
    const registration = name => ({ username: `check_${name}_${suffix}`, displayName: name,
        email: `${name}_${suffix}@example.test`, password, rePassword: password });
    const invalid = await call('/api/Authentication/SignUp', null,
        { ...registration('invalid'), username: 'bad username!' });
    assert.equal(invalid.status, 400);
    const a = await ok('/api/Authentication/SignUp', null, registration('a'));
    const b = await ok('/api/Authentication/SignUp', null, registration('b'));
    const c = await ok('/api/Authentication/SignUp', null, registration('c'));
    const users = await ok('/api/Users/GetUsers', a);
    const bId = users.find(user => user.username === registration('b').username).userId;
    const conversationId = await ok('/api/Conversations/Create', a,
        { isGroup: false, participants: [{ userId: bId, role: 1 }] });
    const send = (content = 'hello') => ({ conversationId, content, messageType: 0, clientRequestId: randomUUID() });
    assert.equal((await call(`/api/Messages/Get/${conversationId}?take=20`)).status, 401);
    assert.equal((await call('/api/Messages/Send', null, send())).status, 401);
    assert.equal((await call('/api/Messages/Send', c, send())).status, 400);
    assert.equal((await call(`/api/Messages/Get/${conversationId}?take=20`, c)).status, 400);
    assert.equal((await call('/api/Messages/Send', a, { ...send(), clientRequestId: undefined })).status, 400);
    assert.equal((await call('/api/Messages/Send', a, { ...send(), messageType: 1 })).status, 400);

    const results = await Promise.all(Array.from({ length: 16 }, (_, i) =>
        ok('/api/Messages/Send', i % 2 ? a : b, send(`concurrent-${i}`))));
    assert.equal(new Set(results.map(m => m.sequenceNumber)).size, 16);
    assert.equal(new Set(results.map(m => m.messageId)).size, 16);
    const request = send('retry me');
    const retries = await Promise.all(Array.from({ length: 8 }, () => ok('/api/Messages/Send', a, request)));
    assert.equal(new Set(retries.map(m => m.messageId)).size, 1);
    assert.equal((await call('/api/Messages/Send', a, { ...request, content: 'different' })).status, 400);

    let cursor = 0;
    const recovered = [];
    for (let pageCount = 0; pageCount < 20; pageCount++) {
        const page = await ok(`/api/Messages/Get/${conversationId}?take=3&afterSequence=${cursor}`, b);
        for (const message of page) {
            assert.ok(message.sequenceNumber > cursor);
            cursor = message.sequenceNumber;
            recovered.push(message);
        }
        if (page.length < 3) break;
    }
    assert.equal(recovered.length, 17);
    assert.equal(new Set(recovered.map(m => m.messageId)).size, 17);
    const expected = retries[0];
    const stored = recovered.find(m => m.messageId === expected.messageId);
    assert.deepEqual(stored, expected, 'history must match persisted send response');
    assert.ok(stored.sentAt.endsWith('Z'));
    assert.ok(Math.abs(Date.now() - Date.parse(stored.sentAt)) < 120000);
    assert.equal((await call(`/api/Messages/Get/${conversationId}?take=0`, a)).status, 400);
    assert.equal((await call(`/api/Messages/Get/${conversationId}?take=20&beforeSequence=3&afterSequence=0`, a)).status, 400);
    const older = await ok(`/api/Messages/Get/${conversationId}?take=3&beforeSequence=${cursor}`, a);
    assert.ok(older.every(m => m.sequenceNumber < cursor));
});
