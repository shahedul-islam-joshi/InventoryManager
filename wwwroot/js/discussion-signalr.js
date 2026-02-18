/*
 * discussion-signalr.js
 *
 * Handles real-time discussion for a single inventory page.
 *
 * WHY SIGNALR GROUPS?
 * Each inventory has its own discussion channel. By joining a group named
 * after the inventoryId, this client only receives messages for THIS inventory,
 * not for every inventory in the system.
 *
 * REQUIREMENTS:
 * - The page must include the SignalR client script before this file.
 * - The discussion container must have data-inventory-id="<guid>" attribute.
 * - The message list must have id="discussion-messages".
 * - The textarea must have id="discussion-input".
 * - The send button must have id="discussion-send".
 */

(function () {
    // Read the inventory ID from the DOM — injected by the Razor partial
    const container = document.getElementById('discussion-container');
    if (!container) return; // Guard: only run on pages that have the discussion tab

    const inventoryId = container.dataset.inventoryId;
    const messageList = document.getElementById('discussion-messages');
    const input = document.getElementById('discussion-input');
    const sendBtn = document.getElementById('discussion-send');

    // -----------------------------------------------------------------------
    // Build the SignalR connection
    // URL must match the hub route registered in Program.cs (/discussionHub)
    // -----------------------------------------------------------------------
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/discussionHub')
        .withAutomaticReconnect()   // Reconnect automatically on network blip
        .build();

    // -----------------------------------------------------------------------
    // receiveMessage handler
    // Called by the server when a new message is broadcast to this group.
    // Appends the message to the DOM without a page refresh.
    //
    // dto shape: { id, userName, content, createdAt }
    // -----------------------------------------------------------------------
    connection.on('receiveMessage', function (dto) {
        appendMessage(dto.userName, dto.content, dto.createdAt);
    });

    // -----------------------------------------------------------------------
    // Start the connection, then join the inventory group
    // -----------------------------------------------------------------------
    connection.start()
        .then(function () {
            // WHY JOIN A GROUP?
            // Without joining, the client would receive no messages even if the
            // server broadcasts to the group. This call tells the Hub to add
            // this connection to the inventory-specific group.
            return connection.invoke('joinInventoryGroup', inventoryId);
        })
        .catch(function (err) {
            console.error('SignalR connection error:', err);
        });

    // -----------------------------------------------------------------------
    // Send button click handler
    // -----------------------------------------------------------------------
    if (sendBtn) {
        sendBtn.addEventListener('click', function () {
            sendMessage();
        });
    }

    // Allow pressing Enter (without Shift) to send
    if (input) {
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                sendMessage();
            }
        });
    }

    function sendMessage() {
        const message = input ? input.value.trim() : '';
        if (!message) return;

        // Invoke the Hub method — the server saves, then broadcasts back via receiveMessage
        connection.invoke('sendMessage', inventoryId, message)
            .then(function () {
                input.value = '';
            })
            .catch(function (err) {
                console.error('SendMessage error:', err);
            });
    }

    // -----------------------------------------------------------------------
    // appendMessage
    // Creates a message card and appends it to the message list.
    // -----------------------------------------------------------------------
    function appendMessage(userName, content, createdAt) {
        const time = new Date(createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        const item = document.createElement('div');
        item.className = 'mb-2 p-2 border rounded bg-light';
        item.innerHTML =
            '<strong class="text-primary">' + escapeHtml(userName) + '</strong>' +
            ' <small class="text-muted">' + time + '</small>' +
            '<div>' + escapeHtml(content) + '</div>';

        messageList.appendChild(item);

        // Auto-scroll to the latest message
        messageList.scrollTop = messageList.scrollHeight;
    }

    // Prevent XSS — never inject raw user content as innerHTML
    function escapeHtml(text) {
        const div = document.createElement('div');
        div.appendChild(document.createTextNode(text));
        return div.innerHTML;
    }
})();
