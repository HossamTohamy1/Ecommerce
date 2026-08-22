(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var messagesEl = document.getElementById('chatMessages');
        var form = document.getElementById('chatForm');
        var input = document.getElementById('chatInput');
        if (!messagesEl || !form || !input || typeof signalR === 'undefined') {
            return;
        }

        var conversationId = null;

        function escapeHtml(text) {
            var div = document.createElement('div');
            div.textContent = text || '';
            return div.innerHTML;
        }

        function parseServerDate(dateString) {
            if (!dateString) {
                return new Date();
            }
            var hasTimezone = /[Zz]$|[+-]\d{2}:\d{2}$/.test(dateString);
            return new Date(hasTimezone ? dateString : dateString + 'Z');
        }

        function renderEmpty() {
            messagesEl.innerHTML = '<div class="text-center text-muted my-auto">' + messagesEl.dataset.emptyText + '</div>';
        }

        function appendMessage(message) {
            if (message.id && messagesEl.querySelector('[data-msg-id="' + message.id + '"]')) {
                return;
            }

            var isMine = message.senderRole === 'Customer';

            if (message.id && isMine) {
                var pending = messagesEl.querySelector('[data-pending="true"]');
                if (pending && pending.getAttribute('data-content') === message.content) {
                    pending.removeAttribute('data-pending');
                    pending.setAttribute('data-msg-id', message.id);
                    return;
                }
            }

            var empty = messagesEl.querySelector('.my-auto');
            if (empty) {
                messagesEl.innerHTML = '';
            }

            var wrapper = document.createElement('div');
            wrapper.className = 'd-flex flex-column ' + (isMine ? 'align-items-end' : 'align-items-start');
            if (message.id) {
                wrapper.setAttribute('data-msg-id', message.id);
            }
            if (message.isPending) {
                wrapper.setAttribute('data-pending', 'true');
                wrapper.setAttribute('data-content', message.content);
            }

            var timeStr = parseServerDate(message.createdAt).toLocaleTimeString();

            wrapper.innerHTML =
                '<div class="rounded-3 px-3 py-2 ' + (isMine ? 'bg-primary text-white' : 'bg-light-secondary') + '" style="max-width: 75%;">' +
                escapeHtml(message.content) +
                '</div>' +
                '<div class="text-muted fs-9 mt-1">' + timeStr + '</div>';
            messagesEl.appendChild(wrapper);
            messagesEl.scrollTop = messagesEl.scrollHeight;
        }

        function loadMessages() {
            fetch('/api/chat/my', { credentials: 'same-origin' })
                .then(function (r) { return r.json(); })
                .then(function (conversation) {
                    if (conversation && conversation.id) {
                        conversationId = conversation.id;
                        return fetch('/api/chat/' + conversationId + '/messages?page=1&pageSize=50', { credentials: 'same-origin' });
                    }
                    return null;
                })
                .then(function (r) { return r ? r.json() : null; })
                .then(function (result) {
                    if (!result) return;
                    messagesEl.innerHTML = '';
                    if (!result.items || result.items.length === 0) {
                        renderEmpty();
                    } else {
                        result.items.forEach(appendMessage);
                    }
                    if (conversationId) {
                        fetch('/api/chat/' + conversationId + '/read', { method: 'PUT', credentials: 'same-origin' }).catch(function () {});
                    }
                })
                .catch(function () {});
        }

        var connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/chat')
            .withAutomaticReconnect()
            .build();

        connection.on('messageReceived', function (message) {
            if (!conversationId || message.conversationId === conversationId) {
                conversationId = message.conversationId;
                appendMessage(message);
                if (conversationId) {
                    fetch('/api/chat/' + conversationId + '/read', { method: 'PUT', credentials: 'same-origin' }).catch(function () {});
                }
            }
        });

        connection.start()
            .then(loadMessages)
            .catch(function () {});

        form.addEventListener('submit', function (e) {
            e.preventDefault();
            var content = input.value.trim();
            if (!content) {
                return;
            }

            input.value = '';

            appendMessage({
                content: content,
                senderRole: 'Customer',
                createdAt: new Date().toISOString(),
                isPending: true
            });

            connection.invoke('SendMessage', conversationId, content).catch(function (err) {
                console.error('Failed to send chat message:', err);
            });
        });
    });
})();
