using Microsoft.AspNetCore.Mvc;

namespace RaceDayMvc.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Organiser") return RedirectToAction("Index", "Organiser");
            if (role == "Participant") return RedirectToAction("Index", "Participant");

            return View();
        }
    }
}