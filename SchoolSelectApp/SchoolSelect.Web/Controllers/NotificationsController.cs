using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Data.Models;
using SchoolSelect.Repositories.Interfaces;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.Models;
using SchoolSelect.Web.ViewModels;
using System.Text.Json;

namespace SchoolSelect.Web.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepository,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _userManager = userManager;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Показва всички известия на потребителя
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Error", "Home");
            }

            var userId = Guid.Parse(userIdString);
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);

            var viewModels = notifications.Select(n => new NotificationViewModel
            {
                Id = n.Id,
                Title = n.Title,
                Content = n.Content,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                NotificationType = n.NotificationType,
                ReferenceId = n.ReferenceId
            });

            return View(viewModels);
        }

        /// <summary>
        /// Маркира известие като прочетено и пренасочва към съответния обект
        /// </summary>
        [HttpGet]
        [Route("Notifications/View/{id}")]
        public async Task<IActionResult> View(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Error", "Home");
            }

            var userId = Guid.Parse(userIdString);

            try
            {
                var notification = await _notificationService.GetByIdAsync(id);

                if (notification == null || notification.UserId != userId)
                {
                    return RedirectToAction(nameof(Index));
                }

                await _notificationService.MarkAsReadAsync(id, userId);

                if (notification.ReferenceId.HasValue)
                {
                    switch (notification.NotificationType)
                    {
                        case 1:
                            return RedirectToAction("Details", "Schools", new { id = notification.ReferenceId.Value });
                        case 2:
                            return RedirectToAction("Details", "SchoolProfiles", new { id = notification.ReferenceId.Value });
                        default:
                            break;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing notification {NotificationId}", id);
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Маркира всички известия като прочетени
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString))
            {
                return RedirectToAction("Error", "Home");
            }

            var userId = Guid.Parse(userIdString);
            await _notificationService.MarkAllAsReadAsync(userId);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Само за администратори - показва форма за изпращане на известия
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new CreateNotificationModel());
        }

        /// <summary>
        /// Само за администратори - обработва изпращане на известия
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateNotificationModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                int sentCount = 0;

                if (model.SendToAll)
                {
                    // Изпращане до всички потребители
                    sentCount = await _notificationService.SendNotificationToAllAsync(
                        model.Title, model.Content, model.NotificationType);
                }
                else if (model.UserIds != null && model.UserIds.Any())
                {
                    // Изпращане до конкретни потребители
                    sentCount = await _notificationService.SendNotificationToUsersAsync(
                        model.UserIds, model.Title, model.Content, model.NotificationType);
                }
                else if (!string.IsNullOrEmpty(model.PreferredDistrict) ||
                        (model.PreferredProfiles != null && model.PreferredProfiles.Any()))
                {
                    // Изпращане по предпочитания
                    sentCount = await _notificationService.SendNotificationByPreferenceAsync(
                        preference =>
                        {
                            // Филтриране по район
                            bool districtMatch = string.IsNullOrEmpty(model.PreferredDistrict) ||
                                                preference.UserDistrict == model.PreferredDistrict;

                            // Филтриране по профили
                            bool profilesMatch = model.PreferredProfiles == null || !model.PreferredProfiles.Any() ||
                                                model.PreferredProfiles.Any(p => preference.PreferredProfiles.Contains(p));

                            return districtMatch && profilesMatch;
                        },
                        model.Title, model.Content, model.NotificationType);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Моля, изберете получатели за известието");
                    return View(model);
                }

                TempData["SuccessMessage"] = $"Успешно изпратени {sentCount} известия.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notifications");
                ModelState.AddModelError(string.Empty, "Възникна грешка при изпращане на известията");
                return View(model);
            }
        }

        /// <summary>
        /// AJAX заявка за намиране на потребители
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FindUsers(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Json(new { results = new List<object>() });
            }

            try
            {
                // Търсим потребители по email или потребителско име
                var users = await _userRepository.FindAsync
                    (u =>
                    (u.Email != null && u.Email.Contains(term)) ||
                    (u.UserName != null && u.UserName.Contains(term)) ||
                    (u.FirstName != null && u.FirstName.Contains(term)) ||
                    (u.LastName != null && u.LastName.Contains(term)));

                var results = users.Select(u => new
                {
                    id = u.Id.ToString(),
                    text = $"{u.FirstName} {u.LastName} ({u.Email})"
                });

                return Json(new { results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding users with term {Term}", term);
                return Json(new { results = new List<object>() });
            }
        }
    }
}