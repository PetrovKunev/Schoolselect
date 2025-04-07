// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Wait for the DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function () {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize dropdowns
    var dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
    dropdownElementList.map(function (dropdownToggleEl) {
        return new bootstrap.Dropdown(dropdownToggleEl);
    });

    // Add animation to the hero section
    const heroSection = document.querySelector('.hero-section');
    if (heroSection) {
        setTimeout(() => {
            heroSection.classList.add('fade-in');
        }, 200);
    }

    // Add animation to feature cards
    const featureCards = document.querySelectorAll('.features-section .card');
    if (featureCards.length > 0) {
        window.addEventListener('scroll', function () {
            featureCards.forEach((card, index) => {
                const cardPosition = card.getBoundingClientRect().top;
                const screenPosition = window.innerHeight / 1.2;

                if (cardPosition < screenPosition) {
                    setTimeout(() => {
                        card.classList.add('show');
                    }, index * 100);
                }
            });
        });
    }

    // Периодично обновяване на броя на непрочетените известия
    function refreshNotificationCount() {
        if (document.querySelector('.notification-badge')) {
            fetch('/api/Notifications/count')
                .then(response => response.json())
                .then(count => {
                    const badge = document.querySelector('.notification-badge');
                    if (count > 0) {
                        badge.textContent = count;
                        badge.style.display = 'inline-block';
                    } else {
                        badge.style.display = 'none';
                    }
                })
                .catch(error => console.error('Error fetching notification count:', error));
        }
    }

    // Обновява броя на известията на всеки 60 секунди
    if (document.querySelector('.notification-badge')) {
        setInterval(refreshNotificationCount, 60000);
    }

    
    $(function () {
        $(".view-formula-btn").on("click", function () {
            const profileId = $(this).data("profile-id");

            $("#formulaModal .modal-content").html(`
            <div class="d-flex justify-content-center p-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>
        `);

            const formulaModal = new bootstrap.Modal(document.getElementById('formulaModal'));
            formulaModal.show();

            $.get(`/Schools/ViewFormula?profileId=${profileId}`)
                .done(function (data) {
                    $("#formulaModal .modal-content").html(data);
                })
                .fail(function () {
                    $("#formulaModal .modal-content").html(`
                    <div class="modal-header">
                        <h5 class="modal-title">Грешка</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <div class="alert alert-danger">Възникна грешка при зареждане на формулата.</div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Затвори</button>
                    </div>
                `);
                });
        });
    });

});