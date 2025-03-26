/**
 * JavaScript функционалност за сравнения на училища
 */
$(function () {
    // Инициализиране на AJAX форми за добавяне към сравнение
    initAddToComparisonForms();

    // Инициализиране на бутоните за премахване от сравнение
    initRemoveFromComparisonButtons();
});

/**
 * Инициализира AJAX форми за добавяне към сравнение
 */
function initAddToComparisonForms() {
    $('.dropdown-item-form').on('submit', function (e) {
        e.preventDefault();

        const form = $(this);
        const url = form.attr('action');
        const data = form.serialize();

        // Добавяме AJAX хедър за установяване на AJAX заявката
        const headers = {
            'X-Requested-With': 'XMLHttpRequest'
        };

        $.ajax({
            type: 'POST',
            url: url,
            data: data,
            headers: headers,
            success: function (response) {
                if (response.success) {
                    // Показваме съобщение за успех
                    showNotification('Успешно добавено към сравнение', 'success');
                } else {
                    // Показваме съобщение за грешка
                    showNotification('Грешка при добавяне към сравнение', 'danger');
                }
            },
            error: function () {
                showNotification('Възникна грешка при комуникацията със сървъра', 'danger');
            }
        });
    });
}

/**
 * Инициализира бутоните за премахване от сравнение с AJAX функционалност
 */
function initRemoveFromComparisonButtons() {
    $('.comparison-table form').on('submit', function (e) {
        e.preventDefault();

        const form = $(this);
        const url = form.attr('action');
        const data = form.serialize();
        const row = form.closest('th').parent();

        // Добавяме AJAX хедър
        const headers = {
            'X-Requested-With': 'XMLHttpRequest'
        };

        $.ajax({
            type: 'POST',
            url: url,
            data: data,
            headers: headers,
            success: function (response) {
                if (response.success) {
                    // Определяме индекса на колоната, която трябва да премахнем
                    const colIndex = row.children().index(form.closest('th'));

                    // Премахваме съответната колона от всички редове
                    $('.comparison-table tr').each(function () {
                        $(this).find(`td:eq(${colIndex - 1}), th:eq(${colIndex})`).remove();
                    });

                    showNotification('Успешно премахнато от сравнение', 'success');

                    // Проверяваме дали все още има елементи в сравнението
                    if ($('.comparison-table th').length <= 1) {
                        // Ако няма повече елементи, презареждаме страницата
                        location.reload();
                    }
                } else {
                    showNotification('Грешка при премахване от сравнение', 'danger');
                }
            },
            error: function () {
                showNotification('Възникна грешка при комуникацията със сървъра', 'danger');
            }
        });
    });
}

/**
 * Показва временно съобщение за известие
 * 
 * @param {string} message Съобщение за показване
 * @param {string} type Тип на съобщението (success, danger, warning, info)
 */
function showNotification(message, type) {
    // Създаваме елемент за известието
    const notification = $(`
        <div class="toast align-items-center text-white bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Затвори"></button>
            </div>
        </div>
    `);

    // Проверяваме дали съществува контейнер за известия, ако не - създаваме го
    let toastContainer = $('.toast-container');
    if (toastContainer.length === 0) {
        toastContainer = $('<div class="toast-container position-fixed bottom-0 end-0 p-3"></div>');
        $('body').append(toastContainer);
    }

    // Добавяме известието към контейнера
    toastContainer.append(notification);

    // Инициализираме и показваме известието
    const toast = new bootstrap.Toast(notification, {
        autohide: true,
        delay: 3000
    });
    toast.show();

    // Премахваме известието от DOM след като се скрие
    notification.on('hidden.bs.toast', function () {
        $(this).remove();
    });
}