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
            return View();
        }

        // GET: /Home/Contact
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        // POST: /Home/Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // TODO: изпратете имейл или запишете съобщението в база данни

            TempData["SuccessMessage"] = "Вашето съобщение беше изпратено успешно. Ще се свържем с вас скоро.";
            return RedirectToAction("Contact");
        }

        // GET: /Home/Terms
        public IActionResult Terms()
        {
            return View();
        }
    }
}