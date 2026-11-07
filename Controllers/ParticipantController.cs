using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Services;
using System.Text.Json;

namespace RaceDayMvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly ApiService _api;

        public ParticipantController(ApiService api) { _api = api; }

        // GET: /Participant
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            ViewBag.Name = HttpContext.Session.GetString("UserName");

            // Fetch all events so the participant can browse them
            var eventsJson = await _api.GetAsync<JsonElement>("Events");
            return View(eventsJson);
        }

        // GET: /Participant/EventDetails/5
        public async Task<IActionResult> EventDetails(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            var eventJson = await _api.GetAsync<JsonElement>($"Events/{id}");

            if (eventJson.ValueKind == JsonValueKind.Undefined || eventJson.ValueKind == JsonValueKind.Null)
                return NotFound();

            return View(eventJson);
        }

        // POST: /Participant/Enrol
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventCategoryId, int eventId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            var enrolData = new { eventCategoryId = eventCategoryId };

            // The API route for enrolling
            var response = await _api.PostAsync("Enrolments", enrolData);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "You have successfully enrolled!";
                return RedirectToAction("MyEnrolments");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Failed to enrol: {response.StatusCode} - {errorContent}";
            return RedirectToAction("EventDetails", new { id = eventId });
        }

        // GET: /Participant/MyEnrolments
        public async Task<IActionResult> MyEnrolments()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            // API route for getting the logged-in participant's enrolments
            var enrolmentsJson = await _api.GetAsync<JsonElement>("Enrolments/my");
            return View(enrolmentsJson);
        }

        // GET: /Participant/MyResults
        public async Task<IActionResult> MyResults()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return RedirectToAction("Login", "Auth");

            // API route for getting the logged-in participant's results
            var resultsJson = await _api.GetAsync<JsonElement>("Results/my");
            return View(resultsJson);
        }
    }
}