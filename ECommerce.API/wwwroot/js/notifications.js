(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var badge = document.getElementById('notificationsUnreadBadge');
        var list = document.getElementById('notificationsDropdownList');
        if (!badge || !list || typeof signalR === 'undefined') {
            return;
        }

        function setUnreadCount(count) {
            if (count > 0) {
                badge.textContent = count > 99 ? '99+' : String(count);
                badge.classList.remove('d-none');
            } else {
                badge.classList.add('d-none');
            }
        }

        function parseServerDate(dateString) {
            if (!dateString) {
                return new Date(NaN);
            }
            var hasTimezone = /[Zz]$|[+-]\d{2}:\d{2}$/.test(dateString);
            return new Date(hasTimezone ? dateString : dateString + 'Z');
        }

        function timeAgo(dateString) {
            var seconds = Math.floor((new Date() - parseServerDate(dateString)) / 1000);
            if (seconds < 60) return 'now';
            var minutes = Math.floor(seconds / 60);
            if (minutes < 60) return minutes + 'm';
            var hours = Math.floor(minutes / 60);
            if (hours < 24) return hours + 'h';
            return Math.floor(hours / 24) + 'd';
        }

        function renderList(items) {
            if (!items || items.length === 0) {
                list.innerHTML = '<div class="menu-item px-3 text-muted text-center py-3">' + list.dataset.emptyText + '</div>';
                return;
            }

            list.innerHTML = items.map(function (n) {
                var href = n.url ? n.url : '#';
                var unreadClass = n.isRead ? '' : 'bg-light-primary';
                return '<a href="' + href + '" class="menu-item px-3 py-2 d-block ' + unreadClass + '" data-notification-id="' + n.id + '">' +
                    '<div class="fw-semibold fs-7">' + escapeHtml(n.title) + '</div>' +
                    '<div class="text-muted fs-8">' + escapeHtml(n.message) + '</div>' +
                    '<div class="text-muted fs-9">' + timeAgo(n.createdAt) + '</div>' +
                    '</a>';
            }).join('');
        }

        function escapeHtml(text) {
            var div = document.createElement('div');
            div.textContent = text || '';
            return div.innerHTML;
        }

        function loadUnreadCount() {
            fetch('/api/notifications/unread-count', { credentials: 'same-origin' })
                .then(function (r) { return r.ok ? r.json() : null; })
                .then(function (data) { if (data) setUnreadCount(data.count); })
                .catch(function () {});
        }

        function loadRecent() {
            fetch('/api/notifications?page=1&pageSize=5', { credentials: 'same-origin' })
                .then(function (r) { return r.ok ? r.json() : null; })
                .then(function (data) { if (data) renderList(data.items); })
                .catch(function () {});
        }

        function markAllAsRead() {
            setUnreadCount(0);
            fetch('/api/notifications/read-all', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' }
            }).catch(function () {});
        }

        function markSingleAsRead(id) {
            if (!id) return;
            fetch('/api/notifications/' + id + '/read', {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json' },
                keepalive: true
            }).catch(function () {});
        }

        list.dataset.emptyText = list.textContent.trim();
        loadUnreadCount();

        var bellTrigger = document.getElementById('notificationsBellTrigger');
        if (bellTrigger) {
            bellTrigger.addEventListener('click', function () {
                loadRecent();
                markAllAsRead();
            });
        }

        list.addEventListener('click', function (e) {
            var item = e.target.closest('[data-notification-id]');
            if (item) {
                var id = item.getAttribute('data-notification-id');
                item.classList.remove('bg-light-primary');
                markSingleAsRead(id);
            }
        });

        var connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/notifications')
            .withAutomaticReconnect()
            .build();

        connection.on('notificationReceived', function () {
            loadUnreadCount();
            loadRecent();
        });

        connection.start().catch(function () {});
    });
})();
