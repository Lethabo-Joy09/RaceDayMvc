using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Services;

namespace RaceDayMvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApiService _api;

        public AuthController(ApiService api) { _api = api; }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var loginData = new { email = email.Trim(), password = password };
            var response = await _api.PostAsync("Auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonDocument.Parse(content).RootElement;

                HttpContext.Session.SetString("UserRole", data.GetProperty("role").GetString()!);
                HttpContext.Session.SetString("UserName", data.GetProperty("fullName").GetString()!);
                HttpContext.Session.SetInt32("UserId", data.GetProperty("userId").GetInt32());

                if (data.GetProperty("role").GetString() == "Organiser")
                    return RedirectToAction("Index", "Organiser");
                else
                    return RedirectToAction("Index", "Participant");
            }

            // Debugging: Show the REAL error from the API
            var errorContent = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Error: {response.StatusCode} - {errorContent}";
            return View();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string fullName, string role)
        {
            // Ensure the role is exactly "Organiser" or "Participant"
            string formattedRole = role == "Organiser" ? "Organiser" : "Participant";

            var registerData = new
            {
                email = email.Trim(),
                password = password,
                fullName = fullName.Trim(),
                role = formattedRole,
                // Include these fields as null so the API DTO is fully satisfied
                companyName = (string?)null,
                contactPhone = (string?)null,
                dateOfBirth = (string?)null,
                emergencyContact = (string?)null,
                emergencyPhone = (string?)null
            };

            var response = await _api.PostAsync("Auth/register", registerData);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Login");

            // Debugging: Show the REAL error from the API
            var errorContent = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Error: {response.StatusCode} - {errorContent}";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}