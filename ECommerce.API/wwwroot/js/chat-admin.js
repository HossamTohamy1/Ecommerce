(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var listEl = document.getElementById('conversationsList');
        var messagesEl = document.getElementById('chatMessages');
        var form = document.getElementById('chatForm');
        var input = document.getElementById('chatInput');
        var formWrapper = document.getElementById('chatFormWrapper');
        if (!listEl || !messagesEl || !form || !input || typeof signalR === 'undefined') {
            return;
        }

        var activeConversationId = listEl.dataset.initialId || null;
        var conversations = [];

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

        function renderConversationsList() {
            if (!conversations.length) {
                listEl.innerHTML = '<div class="text-center text-muted p-4">' + listEl.dataset.emptyText + '</div>';
                return;
            }

            listEl.innerHTML = conversations.map(function (c) {
                var active = c.id === activeConversationId ? 'active' : '';
                var unread = c.hasUnreadForAdmin ? '<span class="badge badge-circle badge-danger ms-2">&nbsp;</span>' : '';
                return '<a href="#" class="list-group-item list-group-item-action chat-conversation-item px-4 py-3 ' + active + '" data-conversation-id="' + c.id + '">' +
                    '<div class="d-flex justify-content-between align-items-center mb-1">' +
                    '<span class="chat-conv-title fw-semibold">' + escapeHtml(c.customerName) + unread + '</span>' +
                    '</div>' +
                    '<div class="chat-conv-preview fs-8 text-truncate">' + escapeHtml(c.lastMessagePreview || '') + '</div>' +
                    '</a>';
            }).join('');

            listEl.querySelectorAll('[data-conversation-id]').forEach(function (el) {
                el.addEventListener('click', function (e) {
                    e.preventDefault();
                    selectConversation(el.getAttribute('data-conversation-id'));
                });
            });
        }

        function loadConversations() {
            return fetch('/api/chat/conversations', { credentials: 'same-origin' })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    conversations = data || [];
                    renderConversationsList();
                })
                .catch(function () {});
        }

        function renderEmptyMessages() {
            messagesEl.innerHTML = '<div class="text-center text-muted my-auto">' + messagesEl.dataset.emptyText + '</div>';
        }

        function appendMessage(message) {
            if (message.id && messagesEl.querySelector('[data-msg-id="' + message.id + '"]')) {
                return;
            }

            var isMine = message.senderRole === 'Admin';

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

        var connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/chat')
            .withAutomaticReconnect()
            .build();

        var conversationsCol = document.getElementById('chatConversationsCol');
        var messagesCol = document.getElementById('chatMessagesCol');
        var backToListBtn = document.getElementById('chatBackToList');

        if (backToListBtn && conversationsCol && messagesCol) {
            backToListBtn.addEventListener('click', function () {
                conversationsCol.classList.remove('d-none');
                messagesCol.classList.add('d-none');
                messagesCol.classList.remove('d-flex');
            });
        }

        function selectConversation(id) {
            if (activeConversationId && activeConversationId !== id) {
                connection.invoke('LeaveConversation', activeConversationId).catch(function () {});
            }

            activeConversationId = id;
            renderConversationsList();
            formWrapper.classList.remove('d-none');

            if (conversationsCol && messagesCol && window.innerWidth < 768) {
                conversationsCol.classList.add('d-none');
                messagesCol.classList.remove('d-none');
                messagesCol.classList.add('d-flex');
            }

            connection.invoke('JoinConversation', id).catch(function () {});

            fetch('/api/chat/' + id + '/messages?page=1&pageSize=50', { credentials: 'same-origin' })
                .then(function (r) { return r.json(); })
                .then(function (result) {
                    messagesEl.innerHTML = '';
                    if (!result.items || result.items.length === 0) {
                        renderEmptyMessages();
                    } else {
                        result.items.forEach(appendMessage);
                    }
                    return fetch('/api/chat/' + id + '/read', { method: 'PUT', credentials: 'same-origin' });
                })
                .then(loadConversations)
                .catch(function () {});
        }

        connection.on('messageReceived', function (message) {
            if (activeConversationId && message.conversationId === activeConversationId) {
                appendMessage(message);
                fetch('/api/chat/' + activeConversationId + '/read', { method: 'PUT', credentials: 'same-origin' }).catch(function () {});
            }

            loadConversations();
        });

        connection.start()
            .then(function () {
                return loadConversations();
            })
            .then(function () {
                if (activeConversationId) {
                    selectConversation(activeConversationId);
                }
            })
            .catch(function () {});

        renderEmptyMessages();

        form.addEventListener('submit', function (e) {
            e.preventDefault();
            var content = input.value.trim();
            if (!content || !activeConversationId) {
                return;
            }

            input.value = '';

            appendMessage({
                content: content,
                senderRole: 'Admin',
                createdAt: new Date().toISOString(),
                isPending: true
            });

            connection.invoke('SendMessage', activeConversationId, content).catch(function (err) {
                console.error('Failed to send admin chat message:', err);
            });
        });
    });
})();
