using Microsoft.AspNetCore.Mvc;
using RaceDayMvc.Services;
using System.Text.Json;
using System.Globalization;

namespace RaceDayMvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly ApiService _api;

        public OrganiserController(ApiService api) { _api = api; }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            ViewBag.Name = HttpContext.Session.GetString("UserName");
            return View();
        }

        public async Task<IActionResult> Events()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var eventsJson = await _api.GetAsync<JsonElement>("Events");
            return View(eventsJson);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(string eventName, string description, DateTime eventDate, string location, string distance, string eventType)
        {
            decimal parsedDistance = ParseDistance(distance);

            var eventData = new
            {
                eventName = eventName,
                description = description ?? "",
                eventDate = eventDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                location = location,
                distance = parsedDistance,
                eventType = eventType,
                status = "Open",
                categories = new object[] { }
            };

            var response = await _api.PostAsync("Events", eventData);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Events");

            var errorContent = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Error: {response.StatusCode} - {errorContent}";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditEvent(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var eventJson = await _api.GetAsync<JsonElement>($"Events/{id}");

            if (eventJson.ValueKind == JsonValueKind.Undefined || eventJson.ValueKind == JsonValueKind.Null)
                return NotFound();

            return View(eventJson);
        }

        [HttpPost]
        public async Task<IActionResult> EditEvent(int id, string eventName, string description, DateTime eventDate, string location, string distance, string eventType, string status)
        {
            decimal parsedDistance = ParseDistance(distance);

            var eventData = new
            {
                eventName = eventName,
                description = description ?? "",
                eventDate = eventDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                location = location,
                distance = parsedDistance,
                eventType = eventType,
                status = status ?? "Open"
            };

            var response = await _api.PutAsync($"Events/{id}", eventData);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Events");

            var errorContent = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Error: {response.StatusCode} - {errorContent}";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var response = await _api.DeleteAsync($"Events/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Events");

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Failed to delete: {response.StatusCode} - {errorContent}";
            return RedirectToAction("Events");
        }

        [HttpGet]
        public async Task<IActionResult> ManageCategories()
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var categoriesJson = await _api.GetAsync<JsonElement>("Categories");
            return View(categoriesJson);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(string categoryName, string description, string distanceKM)
        {
            decimal parsedDistance = 5.0m;
            if (!string.IsNullOrWhiteSpace(distanceKM))
            {
                distanceKM = distanceKM.Replace(",", ".");
                decimal.TryParse(distanceKM, NumberStyles.Any, CultureInfo.InvariantCulture, out parsedDistance);
            }

            var categoryData = new
            {
                categoryName = categoryName,
                description = description ?? "",
                distanceKM = parsedDistance
            };

            var response = await _api.PostAsync("Categories", categoryData);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("ManageCategories");

            var errorContent = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Failed to add category: {response.StatusCode} - {errorContent}";
            return RedirectToAction("ManageCategories");
        }

        // --- ENROLMENTS & RESULTS MANAGEMENT ---

        [HttpGet]
        public async Task<IActionResult> EventEnrolments(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            ViewBag.EventId = id;

            var eventJson = await _api.GetAsync<JsonElement>($"Events/{id}");
            if (eventJson.ValueKind == JsonValueKind.Undefined) return NotFound();

            ViewBag.EventName = eventJson.GetProperty("eventName").GetString();

            var enrolmentsJson = await _api.GetAsync<JsonElement>("Enrolments");

            var filtered = new List<JsonElement>();
            if (enrolmentsJson.ValueKind == JsonValueKind.Array)
            {
                foreach (var e in enrolmentsJson.EnumerateArray())
                {
                    if (e.GetProperty("eventId").GetInt32() == id)
                    {
                        filtered.Add(e);
                    }
                }
            }

            var filteredJson = JsonSerializer.Serialize(filtered);
            var model = JsonDocument.Parse(filteredJson).RootElement;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CaptureResult(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Organiser")
                return RedirectToAction("Login", "Auth");

            var enrolmentJson = await _api.GetAsync<JsonElement>($"Enrolments/{id}");
            if (enrolmentJson.ValueKind == JsonValueKind.Undefined) return NotFound();

            return View(enrolmentJson);
        }

        [HttpPost]
        public async Task<IActionResult> CaptureResult(int id, string finishTime, int? position, string status)
        {
            var timeSpan = TimeSpan.TryParse(finishTime, out var ts) ? ts : TimeSpan.Zero;
            var formattedTime = timeSpan.ToString(@"hh\:mm\:ss");

            var resultData = new
            {
                finishTime = formattedTime,
                position = position,
                status = status ?? "Completed"
            };

            var response = await _api.PostAsync($"Results/enrolment/{id}", resultData);

            if (response.IsSuccessStatusCode)
            {
                var enrolmentJson = await _api.GetAsync<JsonElement>($"Enrolments/{id}");
                var eventId = enrolmentJson.GetProperty("eventId").GetInt32();

                TempData["Success"] = "Result captured successfully!";
                return RedirectToAction("EventEnrolments", new { id = eventId });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            ViewBag.Error = $"API Error: {response.StatusCode} - {errorContent}";

            var currentEnrolment = await _api.GetAsync<JsonElement>($"Enrolments/{id}");
            return View(currentEnrolment);
        }

        private decimal ParseDistance(string distance)
        {
            decimal parsed = 5.0m;
            if (!string.IsNullOrWhiteSpace(distance))
            {
                distance = distance.Replace(",", ".");
                decimal.TryParse(distance, NumberStyles.Any, CultureInfo.InvariantCulture, out parsed);
            }
            if (parsed < 0.1m) parsed = 5.0m;
            return parsed;
        }
    }
}