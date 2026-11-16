const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

// Exercise the actual view handler without a browser or external packages.
const view = readFileSync(path.join(__dirname,
    '../Endpoint.Site/Views/Conversation/GetChat.cshtml'), 'utf8');
const handler = view.match(/connection\.on\("ReceiveMessage", function \(message\) \{([\s\S]*?)\n\s*\}\);/);
assert.ok(handler, 'ReceiveMessage handler must exist');

function deliver(message) {
    const rendered = [];
    const feedback = [];
    const messagesBox = { scrollTop: 12, scrollHeight: 100 };
    vm.runInNewContext(`(function(message) {${handler[1]} })(message)`, {
        message, conversationId: 42, messagesBox,
        createMessage: (...args) => rendered.push(args),
        setFeedback: (...args) => feedback.push(args)
    });
    return { rendered, feedback, messagesBox };
}

test('other conversations and malformed events leave the open thread untouched', () => {
    for (const message of [null, undefined, {}, { conversationId: 43 },
        { conversationId: null }, { conversationId: '42x' }]) {
        const result = deliver(message);
        assert.equal(result.rendered.length, 0);
        assert.equal(result.feedback.length, 0);
        assert.equal(result.messagesBox.scrollTop, 12);
    }
});

test('current conversation accepts numeric and string IDs', () => {
    for (const conversationId of [42, '42']) {
        const message = { conversationId, content: 'Hello' };
        const result = deliver(message);
        assert.equal(result.rendered.length, 1);
        assert.equal(result.rendered[0][0], message);
        assert.equal(result.rendered[0][1], false);
        assert.equal(result.feedback.length, 1);
        assert.equal(result.messagesBox.scrollTop, 100);
    }
});
