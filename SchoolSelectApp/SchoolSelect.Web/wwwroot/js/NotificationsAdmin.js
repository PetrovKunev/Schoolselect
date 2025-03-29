$(function () {
        // Load notification count
        function loadNotificationCount() {
            $.get('@Url.Action("GetUnreadCount", "Notification", new { area = "Admin" })', function (data) {
                const count = data.count;
                const badge = $('#notificationCount');

                if (count > 0) {
                    badge.text(count);
                    badge.show();
                } else {
                    badge.hide();
                }
            });
        }
    
    // Load notifications when clicked
    $('#notificationDropdown').on('click', function() {
        $('.notification-container').load('@Url.Action("GetNotifications", "Notification", new { area = "Admin" })');
    });

    // Mark notification as read when clicked
    $(document).on('click', '.notification-item', function(e) {
        e.preventDefault();
    const id = $(this).data('notification-id');

    $.post('@Url.Action("MarkAsRead", "Notification", new {area = "Admin"})', {id: id }, function() {
        $(e.currentTarget).removeClass('unread');
    loadNotificationCount();
        });
    });

    // Mark all as read
    $(document).on('click', '#markAllAsRead', function(e) {
        e.preventDefault();

    $.post('@Url.Action("MarkAllAsRead", "Notification", new {area = "Admin"})', function() {
        $('.notification-item').removeClass('unread');
    loadNotificationCount();
        });
    });

    // Initial load
    loadNotificationCount();

    // Refresh count periodically
    setInterval(loadNotificationCount, 60000); // Every minute
});