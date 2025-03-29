$(function () {
    // Load unread notification count on page load
    loadUnreadNotificationCount();

    // Set up notification click handlers
    $(document).on('click', '.notification-item', function (e) {
        e.preventDefault();
        const notificationId = $(this).data('notification-id');
        markNotificationAsRead(notificationId);
    });

    // Mark all as read handler
    $(document).on('click', '#markAllAsRead', function (e) {
        e.preventDefault();
        markAllNotificationsAsRead();
    });

    // Refresh notifications periodically (every 60 seconds)
    setInterval(function () {
        loadUnreadNotificationCount();
    }, 60000);

    // Function to load unread notification count
    function loadUnreadNotificationCount() {
        $.ajax({
            url: '/Admin/Notifications/GetUnreadCount',
            type: 'GET',
            success: function (data) {
                // Update notification badge
                const badge = $('#notification-badge');
                if (data.count > 0) {
                    badge.text(data.count).show();
                } else {
                    badge.hide();
                }
            }
        });
    }

    // Function to mark a notification as read
    function markNotificationAsRead(id) {
        $.ajax({
            url: '/Admin/Notifications/MarkAsRead',
            type: 'POST',
            data: { id: id },
            success: function () {
                // Refresh notification list
                loadNotificationList();
                // Update unread count
                loadUnreadNotificationCount();
            }
        });
    }

    // Function to mark all notifications as read
    function markAllNotificationsAsRead() {
        $.ajax({
            url: '/Admin/Notifications/MarkAllAsRead',
            type: 'POST',
            success: function () {
                // Refresh notification list
                loadNotificationList();
                // Update unread count
                loadUnreadNotificationCount();
            }
        });
    }

    // Function to load notification list
    function loadNotificationList() {
        $.ajax({
            url: '/Admin/Notifications/GetNotifications',
            type: 'GET',
            success: function (response) {
                $('.notification-dropdown-menu').html(response);
            },
            error: function (xhr, status, error) {
                console.error('Грешка при зареждане на известия:', error);
            }
        });
    }
});