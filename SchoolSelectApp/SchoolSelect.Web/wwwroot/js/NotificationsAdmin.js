document.addEventListener('DOMContentLoaded', function () {
    // Проверка дали сме в админ панела
    if ($('body').find('.notification-dropdown-menu').length > 0) {
        // Зареждане на брой нотификации при зареждане на страницата
        loadNotificationCount();

        // Периодично опресняване на броя нотификации (на всеки 30 секунди)
        setInterval(loadNotificationCount, 30000);

        // Инициализиране на dropdown-а с нотификации
        $("#notification-dropdown-toggle").on("click", function () {
            loadNotifications();
        });

        // Обработка на бутона "Маркирай всички като прочетени"
        $(document).on("click", "#markAllAsRead", function (e) {
            e.preventDefault();
            markAllAsRead();
        });

        // Обработка на кликване върху отделна нотификация
        $(document).on("click", ".notification-item", function (e) {
            e.preventDefault();
            var notificationId = $(this).data("notification-id");
            markAsRead(notificationId);
        });
    }

    function loadNotificationCount() {
        $.ajax({
            url: "/Admin/Notifications/GetUnreadCount",
            type: "GET",
            success: function (data) {
                if (data && data.count > 0) {
                    $("#notification-badge").text(data.count).show();
                } else {
                    $("#notification-badge").hide();
                }
            },
            error: function (xhr, status, error) {
                console.error("Грешка при зареждане на броя нотификации: " + error);
            }
        });
    }

    function loadNotifications() {
        $.ajax({
            url: "/Admin/Notifications/GetNotifications",
            type: "GET",
            success: function (data) {
                $(".notification-dropdown-menu").html(data);
            },
            error: function (xhr, status, error) {
                console.error("Грешка при зареждане на нотификации: " + error);
                $(".notification-dropdown-menu").html('<div class="p-3 text-center">Грешка при зареждане на нотификации</div>');
            }
        });
    }

    function markAsRead(notificationId) {
        $.ajax({
            url: "/Admin/Notifications/MarkAsRead",
            type: "POST",
            data: { id: notificationId },
            headers: {
                RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            success: function () {
                loadNotifications();
                loadNotificationCount();
            },
            error: function (xhr, status, error) {
                console.error("Грешка при маркиране като прочетено: " + error);
            }
        });
    }

    function markAllAsRead() {
        $.ajax({
            url: "/Admin/Notifications/MarkAllAsRead",
            type: "POST",
            headers: {
                RequestVerificationToken: $('input:hidden[name="__RequestVerificationToken"]').val()
            },
            success: function () {
                loadNotifications();
                loadNotificationCount();
            },
            error: function (xhr, status, error) {
                console.error("Грешка при маркиране на всички като прочетени: " + error);
            }
        });
    }
});