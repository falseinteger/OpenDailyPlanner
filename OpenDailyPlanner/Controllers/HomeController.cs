using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using OpenDailyPlanner.ViewModels;

namespace OpenDailyPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Главная страница где мы запрашиваем информацию о пользователе
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            if (Services.Session.Current.IsExistUserName(HttpContext.Session))
                return RedirectToAction("Index", "DalilyPlanner");

            return View();
        }

        /// <summary>
        /// Страница с полочением имени пользователя и записываем его в сессию
        /// </summary>
        /// <param name="model">Модель запроса локального котроллера</param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Login(HomeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            Services.Session.Current.SetUserName(HttpContext.Session, model.UserName);
            return RedirectToAction("Index", "DalilyPlanner");
        }

        /// <summary>
        /// Удалем имя из сессии
        /// </summary>
        /// <returns></returns>
        public IActionResult Logout()
        {
            Services.Session.Current.RemoveUserName(HttpContext.Session);
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Политика конфиденциальности
        /// </summary>
        /// <returns></returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Страница с ошибкой
        /// </summary>
        /// <returns></returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
