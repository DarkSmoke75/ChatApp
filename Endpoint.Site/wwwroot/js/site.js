(function () {
    const page = document.querySelector(".chat-page");
    if (!page) {
        return;
    }

    const tokenStorageKey = "token";
    const tokenInput = document.getElementById("tokenInput");
    const apiBaseUrlInput = document.getElementById("apiBaseUrl");
    const conversationIdInput = document.getElementById("conversationId");
    const messageTypeInput = document.getElementById("messageType");
    const messageInput = document.getElementById("messageInput");
    const connectBtn = document.getElementById("connectBtn");
    const saveTokenBtn = document.getElementById("saveTokenBtn");
    const clearTokenBtn = document.getElementById("clearTokenBtn");
    const loadMessagesBtn = document.getElementById("loadMessagesBtn");
    const sendBtn = document.getElementById("sendBtn");
    const clearMessagesBtn = document.getElementById("clearMessagesBtn");
    const connectionStatus = document.getElementById("connectionStatus");
    const tokenState = document.getElementById("tokenState");
    const hubState = document.getElementById("hubState");
    const feedbackText = document.getElementById("feedbackText");
    const messagesList = document.getElementById("messagesList");
    const emptyState = document.getElementById("emptyState");

    let connection = null;

    function getToken() {
        return (localStorage.getItem(tokenStorageKey) || "").trim();
    }

    function setFeedback(message, isError) {
        feedbackText.textContent = message;
        feedbackText.style.color = isError ? "#9d2f2f" : "#5f6c82";
    }

    function updateTokenState() {
        const token = getToken();
        tokenState.textContent = token ? "Available" : "Missing";
        tokenInput.value = token;
    }

    function setStatus(text, stateClass) {
        connectionStatus.textContent = text;
        connectionStatus.className = "status-pill";
        if (stateClass) {
            connectionStatus.classList.add(stateClass);
        }
        hubState.textContent = text;
    }

    function getApiBaseUrl() {
        return (apiBaseUrlInput.value || "").trim().replace(/\/+$/, "");
    }

    function removeEmptyState() {
        if (emptyState) {
            emptyState.remove();
        }
    }

    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#39;");
    }

    function formatTime(value) {
        if (!value) {
            return new Date().toLocaleTimeString();
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return String(value);
        }

        return date.toLocaleString();
    }

    function renderMessage(message, mode) {
        removeEmptyState();

        const item = document.createElement("article");
        item.className = "message-item " + mode;

        const sender = message.senderId ?? message.userId ?? "-";
        const conversation = message.conversationId ?? conversationIdInput.value;
        const content = message.content ?? message.message ?? JSON.stringify(message, null, 2);
        const sentAt = message.sendDate ?? message.sentAt ?? message.messageDate ?? null;
        const sequence = message.sequenceNumber ?? message.messageId ?? "-";
        const badge = mode === "history" ? "History" : "Live";

        item.innerHTML =
            "<div class=\"message-topline\">" +
                "<span class=\"message-badge\">" + badge + "</span>" +
                "<span class=\"message-time\">" + escapeHtml(formatTime(sentAt)) + "</span>" +
            "</div>" +
            "<p class=\"message-content\">" + escapeHtml(content) + "</p>" +
            "<div class=\"message-meta\">" +
                "<span>Conversation: " + escapeHtml(conversation) + "</span>" +
                "<span>Sender: " + escapeHtml(sender) + "</span>" +
                "<span>Ref: " + escapeHtml(sequence) + "</span>" +
            "</div>";

        messagesList.prepend(item);
    }

    async function ensureConnection() {
        const token = getToken();
        const baseUrl = getApiBaseUrl();

        if (!token) {
            setFeedback("Add a JWT token first.", true);
            updateTokenState();
            return;
        }

        if (!baseUrl) {
            setFeedback("Enter the API base URL first.", true);
            return;
        }

        if (connection && connection.state === signalR.HubConnectionState.Connected) {
            setStatus("Connected", "connected");
            return;
        }

        if (connection) {
            try {
                await connection.stop();
            } catch (error) {
                console.error(error);
            }
        }

        setStatus("Connecting", "connecting");
        setFeedback("Connecting to SignalR...", false);

        connection = new signalR.HubConnectionBuilder()
            .withUrl(baseUrl + "/hubs/chat", {
                accessTokenFactory: getToken
            })
            .withAutomaticReconnect()
            .build();

        connection.on("ReceiveMessage", function (message) {
            renderMessage(message, "live");
            setFeedback("A live message arrived.", false);
        });

        connection.onreconnecting(function () {
            setStatus("Reconnecting", "connecting");
            setFeedback("SignalR is reconnecting...", false);
        });

        connection.onreconnected(function () {
            setStatus("Connected", "connected");
            setFeedback("SignalR reconnected.", false);
        });

        connection.onclose(function () {
            setStatus("Disconnected", "");
        });

        try {
            await connection.start();
            setStatus("Connected", "connected");
            setFeedback("SignalR connected.", false);
        } catch (error) {
            console.error(error);
            setStatus("Error", "error");
            setFeedback("Connection failed. Check the token and API URL.", true);
        }
    }

    async function loadMessages() {
        const token = getToken();
        const baseUrl = getApiBaseUrl();
        const conversationId = conversationIdInput.value;

        if (!token) {
            setFeedback("Add a JWT token first.", true);
            return;
        }

        try {
            setFeedback("Loading message history...", false);
            const response = await fetch(
                baseUrl + "/api/messages/get/" + encodeURIComponent(conversationId) + "?take=20",
                {
                    headers: {
                        "Authorization": "Bearer " + token
                    }
                }
            );

            if (!response.ok) {
                throw new Error("History request failed with status " + response.status);
            }

            const payload = await response.json();
            const items = Array.isArray(payload.data) ? payload.data : [];
            messagesList.innerHTML = "";

            if (items.length === 0) {
                messagesList.appendChild(emptyState);
                setFeedback("No history was returned for this conversation.", false);
                return;
            }

            items.slice().reverse().forEach(function (item) {
                renderMessage(item, "history");
            });

            setFeedback("Loaded " + items.length + " messages.", false);
        } catch (error) {
            console.error(error);
            setFeedback("Could not load history. Verify the conversation id and token.", true);
        }
    }

    async function sendMessage() {
        const token = getToken();
        const baseUrl = getApiBaseUrl();
        const conversationId = Number(conversationIdInput.value);
        const messageType = Number(messageTypeInput.value);
        const content = messageInput.value.trim();

        if (!token) {
            setFeedback("Add a JWT token first.", true);
            return;
        }

        if (!content) {
            setFeedback("Write a message before sending.", true);
            return;
        }

        try {
            const response = await fetch(baseUrl + "/api/messages/send", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": "Bearer " + token
                },
                body: JSON.stringify({
                    conversationId: conversationId,
                    content: content,
                    messageType: messageType
                })
            });

            if (!response.ok) {
                throw new Error("Send request failed with status " + response.status);
            }

            messageInput.value = "";
            setFeedback("Message sent.", false);
        } catch (error) {
            console.error(error);
            setFeedback("Message send failed. Check the token and conversation id.", true);
        }
    }

    saveTokenBtn.addEventListener("click", function () {
        const token = tokenInput.value.trim();
        if (token) {
            localStorage.setItem(tokenStorageKey, token);
            setFeedback("Token saved to localStorage.", false);
        } else {
            localStorage.removeItem(tokenStorageKey);
            setFeedback("Token cleared from localStorage.", false);
        }

        updateTokenState();
    });

    clearTokenBtn.addEventListener("click", function () {
        tokenInput.value = "";
        localStorage.removeItem(tokenStorageKey);
        updateTokenState();
        setFeedback("Token cleared.", false);
    });

    connectBtn.addEventListener("click", ensureConnection);
    loadMessagesBtn.addEventListener("click", loadMessages);
    sendBtn.addEventListener("click", sendMessage);

    clearMessagesBtn.addEventListener("click", function () {
        messagesList.innerHTML = "";
        messagesList.appendChild(emptyState);
        setFeedback("Activity cleared.", false);
    });

    messageInput.addEventListener("keydown", function (event) {
        if (event.key === "Enter" && event.ctrlKey) {
            event.preventDefault();
            sendMessage();
        }
    });

    updateTokenState();
    setStatus("Offline", "");
    setFeedback("Ready.", false);
})();
