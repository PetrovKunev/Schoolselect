using Microsoft.AspNetCore.Mvc;
using SchoolSelect.Services.Interfaces;
using SchoolSelect.Web.Models;
using SchoolSelect.Web.ViewModels;
using System.Diagnostics;

namespace SchoolSelect.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _homeService;

        public HomeController(ILogger<HomeController> logger, IHomeService homeService)
        {
            _logger = logger;
            _homeService = homeService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _homeService.GetHomePageDataAsync();
            model.IsAuthenticated = User.Identity?.IsAuthenticated ?? false;
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: /Home/About
        public IActionResult About()
        {
            var model = new AboutViewModel
            {
                MissionStatement = "Да помогнем на ученици и родители да направят информиран избор на училище, базиран на данни, оценки и реални критерии.",
                VisionStatement = "Да създадем платформа, която прави избора на училище по-лесен, прозрачен и подходящ за всеки ученик и родител.",
                ContactEmail = "admin@schoolselect.net"
            };

            return View(model);
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            return View(new ContactFormViewModel());
        }

        // POST: /Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Here you would implement email sending logic
            // For example, using a service like SendGrid, SMTP client, etc.

            // For this example, we'll just show a success message
            TempData["SuccessMessage"] = "Вашето съобщение беше изпратено успешно. Ще се свържем с вас възможно най-скоро.";

            return RedirectToAction(nameof(Contact));
        }

        // GET: /Home/Terms
        public IActionResult Terms()
        {
            return View();
        }
    }
}