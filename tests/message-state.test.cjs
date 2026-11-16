const { test } = require('node:test');
const assert = require('node:assert/strict');
const { create } = require('../Endpoint.Site/wwwroot/js/message-state.js');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const message = (id, sequence = id, conversationId = 42) => ({ messageId: id, sequenceNumber: sequence, conversationId });

test('history, send response and live event converge to one message', () => {
    const state = create(42, [message(1)], 1);
    assert.equal(state.accept(message(1)), false);
    assert.equal(state.accept(message(2)), true);
    assert.equal(state.accept(message(2)), false);
    state.advancePage([message(2)]);
    assert.equal(state.cursor, 2);
});

test('out-of-order live events do not skip missing messages during recovery', () => {
    const state = create(42, [], 10);
    assert.equal(state.accept(message(13)), true);
    assert.equal(state.cursor, 10);
    assert.equal(state.accept(message(11)), true);
    assert.equal(state.accept(message(12)), true);
    assert.equal(state.accept(message(13)), false);
    state.advancePage([message(11), message(12), message(13)]);
    assert.equal(state.cursor, 13);
});

test('unrelated conversations and invalid messages cannot enter state', () => {
    const state = create(42, [], 0);
    for (const value of [null, {}, message(1, 1, 43), message(1, 0)])
        assert.equal(state.accept(value), false);
    state.advancePage([message(99, 99, 43)]);
    assert.equal(state.cursor, 0);
    assert.equal(state.accept(message(1)), true);
});

test('older history never rewinds the catch-up cursor', () => {
    const state = create(42, [], 100);
    state.advancePage([message(50)]);
    assert.equal(state.cursor, 100);
    assert.equal(state.accept(message(50)), true);
});

test('chat page JavaScript parses after Razor values are supplied', () => {
    const view = fs.readFileSync(path.join(__dirname, '../Endpoint.Site/Views/Conversation/GetChat.cshtml'), 'utf8');
    const script = view.match(/<script>([\s\S]*?)<\/script>/)[1]
        .replace(/@conversationId/g, '42').replace(/@firstSequence/g, '1').replace(/@lastSequence/g, '20')
        .replace(/@Html\.Raw\(System\.Text\.Json\.JsonSerializer\.Serialize\(serverToken\)\)/g, '"test-token"')
        .replace(/@Html\.Raw\(System\.Text\.Json\.JsonSerializer\.Serialize\(userDisplayNames\)\)/g, '{}');
    assert.doesNotThrow(() => new vm.Script(script));
});
