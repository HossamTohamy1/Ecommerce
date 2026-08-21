(function () {
    'use strict';

    var THEME_KEY = 'ecommerce-theme';

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(THEME_KEY, theme);
    }

    document.addEventListener('DOMContentLoaded', function () {
        var saved = localStorage.getItem(THEME_KEY);
        if (saved === 'dark') {
            applyTheme('dark');
        }

        var themeToggle = document.getElementById('themeToggle');
        var mobileThemeToggle = document.getElementById('mobileThemeToggle');
        function handleThemeToggle() {
            var current = document.documentElement.getAttribute('data-theme') || 'light';
            applyTheme(current === 'dark' ? 'light' : 'dark');
        }
        if (themeToggle) {
            themeToggle.addEventListener('click', handleThemeToggle);
        }
        if (mobileThemeToggle) {
            mobileThemeToggle.addEventListener('click', handleThemeToggle);
        }

        var sidebarToggle = document.getElementById('sidebarToggle');
        var mobileDrawer = document.getElementById('mobileDrawer');
        var mobileDrawerBackdrop = document.getElementById('mobileDrawerBackdrop');
        var mobileDrawerClose = document.getElementById('mobileDrawerClose');

        function openMobileDrawer() {
            if (mobileDrawer) mobileDrawer.classList.add('show');
            if (mobileDrawerBackdrop) mobileDrawerBackdrop.classList.add('show');
            document.body.style.overflow = 'hidden';
            if (mobileDrawerClose) mobileDrawerClose.focus();
        }

        function closeMobileDrawer() {
            if (mobileDrawer) mobileDrawer.classList.remove('show');
            if (mobileDrawerBackdrop) mobileDrawerBackdrop.classList.remove('show');
            document.body.style.overflow = '';
            if (sidebarToggle) {
                sidebarToggle.focus();
            }
        }

        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', openMobileDrawer);
        }
        if (mobileDrawerClose) {
            mobileDrawerClose.addEventListener('click', closeMobileDrawer);
        }
        if (mobileDrawerBackdrop) {
            mobileDrawerBackdrop.addEventListener('click', closeMobileDrawer);
        }

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && mobileDrawer && mobileDrawer.classList.contains('show')) {
                closeMobileDrawer();
            }
        });

        document.querySelectorAll('.mobile-nav-accordion-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var group = btn.closest('.mobile-nav-group');
                if (group) {
                    group.classList.toggle('open');
                }
            });
        });

        document.querySelectorAll('[data-set-culture]').forEach(function (el) {
            el.addEventListener('click', function (e) {
                e.preventDefault();
                var culture = el.getAttribute('data-set-culture');
                var value = 'c=' + culture + '|uic=' + culture;
                document.cookie = '.AspNetCore.Culture=' + encodeURIComponent(value) + ';path=/;max-age=' + (60 * 60 * 24 * 365);
                window.location.reload();
            });
        });

        var currentPath = window.location.pathname.toLowerCase();
        document.querySelectorAll('#kt_header_menu_items .menu-link').forEach(function (link) {
            var href = (link.getAttribute('href') || '').toLowerCase();
            if (href && href !== '/' && currentPath.indexOf(href) === 0) {
                link.classList.add('active');

                var parentTrigger = link.closest('.menu-item[data-kt-menu-trigger]');
                if (parentTrigger) {
                    var parentLink = parentTrigger.querySelector(':scope > .menu-link');
                    if (parentLink) {
                        parentLink.classList.add('active');
                    }
                }
            }
        });

        document.querySelectorAll('[data-confirm-delete]').forEach(function (form) {
            form.addEventListener('submit', function (e) {
                e.preventDefault();
                var title = form.getAttribute('data-confirm-title') || 'Are you sure?';
                var text = form.getAttribute('data-confirm-text') || '';
                var confirmText = form.getAttribute('data-confirm-button') || 'Yes';
                var cancelText = form.getAttribute('data-cancel-button') || 'Cancel';

                Swal.fire({
                    icon: 'warning',
                    title: title,
                    text: text,
                    showCancelButton: true,
                    confirmButtonText: confirmText,
                    cancelButtonText: cancelText,
                    confirmButtonColor: '#dc2626',
                    reverseButtons: true
                }).then(function (result) {
                    if (result.isConfirmed) {
                        form.submit();
                    }
                });
            });
        });
    });
})();
