using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Data.Models;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.ViewModels;

namespace SchoolSelect.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsApiController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<NotificationsApiController> _logger;

        public NotificationsApiController(
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            ILogger<NotificationsApiController> logger)
        {
            _notificationService = notificationService;
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Връща всички известия на текущия потребител
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationViewModel>>> GetNotifications()
        {
            try
            {
                var userIdString = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("User ID is null or empty");
                }

                var userId = Guid.Parse(userIdString);
                var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);

                return Ok(notifications.Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    NotificationType = n.NotificationType,
                    ReferenceId = n.ReferenceId
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for user {UserId}", _userManager.GetUserId(User));
                return StatusCode(500, "An error occurred while retrieving notifications");
            }
        }

        /// <summary>
        /// Връща непрочетените известия на текущия потребител
        /// </summary>
        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<NotificationViewModel>>> GetUnreadNotifications()
        {
            try
            {
                var userIdString = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("User ID is null or empty");
                }

                var userId = Guid.Parse(userIdString);
                var notifications = await _notificationService.GetUnreadNotificationsByUserIdAsync(userId);

                return Ok(notifications.Select(n => new NotificationViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    NotificationType = n.NotificationType,
                    ReferenceId = n.ReferenceId
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications for user {UserId}", _userManager.GetUserId(User));
                return StatusCode(500, "An error occurred while retrieving unread notifications");
            }
        }

        /// <summary>
        /// Връща брой на непрочетените известия на текущия потребител
        /// </summary>
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetUnreadNotificationsCount()
        {
            try
            {
                var userIdString = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("User ID is null or empty");
                }

                var userId = Guid.Parse(userIdString);
                var count = await _notificationService.GetUnreadNotificationCountAsync(userId);

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications count for user {UserId}", _userManager.GetUserId(User));
                return StatusCode(500, "An error occurred while retrieving unread notifications count");
            }
        }

        /// <summary>
        /// Маркира известие като прочетено
        /// </summary>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userIdString = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("User ID is null or empty");
                }

                var userId = Guid.Parse(userIdString);
                var success = await _notificationService.MarkAsReadAsync(id, userId);

                if (!success)
                {
                    return BadRequest("Unable to mark notification as read");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", id, _userManager.GetUserId(User));
                return StatusCode(500, "An error occurred while marking notification as read");
            }
        }

        /// <summary>
        /// Маркира всички известия като прочетени
        /// </summary>
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userIdString = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userIdString))
                {
                    return Unauthorized("User ID is null or empty");
                }

                var userId = Guid.Parse(userIdString);
                await _notificationService.MarkAllAsReadAsync(userId);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", _userManager.GetUserId(User));
                return StatusCode(500, "An error occurred while marking all notifications as read");
            }
        }
    }
}