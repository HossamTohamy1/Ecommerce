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
            var parsed = parseServerDate(dateString);
            if (isNaN(parsed.getTime())) return '';
            var seconds = Math.floor((new Date() - parsed) / 1000);
            if (seconds < 60) return 'now';
            var minutes = Math.floor(seconds / 60);
            if (minutes < 60) return minutes + 'm';
            var hours = Math.floor(minutes / 60);
            if (hours < 24) return hours + 'h';
            return Math.floor(hours / 24) + 'd';
        }

        function isRtlText(text) {
            if (!text) return false;
            var rtlCharRegex = /[\u0591-\u07FF\uFB1D-\uFDFD\uFE70-\uFEFC]/;
            return rtlCharRegex.test(text);
        }

        function escapeHtml(text) {
            var div = document.createElement('div');
            div.textContent = text || '';
            return div.innerHTML;
        }

        function renderList(items) {
            if (!items || items.length === 0) {
                list.innerHTML = '<div class="px-3 text-muted text-center py-4 fs-7">' + (list.dataset.emptyText || 'No notifications') + '</div>';
                return;
            }

            list.innerHTML = items.map(function (n) {
                var href = n.url ? n.url : '#';
                var unreadClass = n.isRead ? '' : 'is-unread';
                var dotColor = n.isRead ? 'bg-secondary' : 'bg-primary';
                var isTitleRtl = isRtlText(n.title);
                var isMsgRtl = isRtlText(n.message);
                var titleDir = isTitleRtl ? 'dir="rtl"' : 'dir="ltr"';
                var msgDir = isMsgRtl ? 'dir="rtl"' : 'dir="ltr"';

                return '<a href="' + href + '" class="notification-dropdown-item d-flex align-items-start gap-2 px-3 py-2 text-decoration-none border-bottom ' + unreadClass + '" data-notification-id="' + n.id + '" style="text-decoration: none !important;">' +
                    '<div class="flex-shrink-0 pt-1">' +
                    '<span class="bullet bullet-dot ' + dotColor + ' h-6px w-6px"></span>' +
                    '</div>' +
                    '<div class="flex-grow-1 min-w-0" style="unicode-bidi: isolate; text-align: start;">' +
                    '<div class="fw-bold fs-7 text-dark mb-1 text-truncate" ' + titleDir + ' style="text-decoration: none !important; line-height: 1.35;">' + escapeHtml(n.title) + '</div>' +
                    '<div class="text-muted fs-8 mb-1" ' + msgDir + ' style="text-decoration: none !important; line-height: 1.35; word-break: break-word;">' + escapeHtml(n.message) + '</div>' +
                    '<div class="text-muted fs-9" style="text-decoration: none !important;">' + timeAgo(n.createdAt) + '</div>' +
                    '</div>' +
                    '</a>';
            }).join('');
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
                item.classList.remove('is-unread');
                var dot = item.querySelector('.bullet-dot');
                if (dot) {
                    dot.classList.remove('bg-primary');
                    dot.classList.add('bg-secondary');
                }
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
