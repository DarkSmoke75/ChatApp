(function (root) {
    "use strict";
    function create(conversationId, initialMessages, initialCursor) {
        const seen = new Set(initialMessages.map(m => String(m.messageId)));
        let cursor = initialCursor;
        return {
            accept(message) {
                if (!message || String(message.conversationId) !== String(conversationId) ||
                    !message.messageId || !Number.isSafeInteger(message.sequenceNumber) || message.sequenceNumber <= 0)
                    return false;
                const id = String(message.messageId);
                if (seen.has(id)) return false;
                seen.add(id);
                return true;
            },
            // Only a completed, ascending REST page advances recovery. Live events
            // may arrive out of order and must never skip an unseen stored message.
            advancePage(messages) {
                for (const message of messages) {
                    if (String(message.conversationId) === String(conversationId))
                        cursor = Math.max(cursor, message.sequenceNumber);
                }
            },
            get cursor() { return cursor; }
        };
    }
    if (typeof module !== "undefined" && module.exports) module.exports = { create };
    else root.ChatMessageState = { create };
})(globalThis);
