using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace RaceDayMvc.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            // Check if we are running inside a Docker container
            if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
            {
                _apiBaseUrl = "http://host.docker.internal:5272/api";
            }
            else
            {
                // We are running locally in Visual Studio
                _apiBaseUrl = "http://localhost:5272/api";
            }
        }

        // Generic GET method
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/{endpoint}");
            AddSessionCookie(request);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return default;
        }

        // Generic POST method
        public async Task<HttpResponseMessage> PostAsync<T>(string endpoint, T data)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/{endpoint}");
            request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            AddSessionCookie(request);

            var response = await _httpClient.SendAsync(request);
            CaptureSessionCookie(response);
            return response;
        }

        // Generic PUT method
        public async Task<HttpResponseMessage> PutAsync<T>(string endpoint, T data)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"{_apiBaseUrl}/{endpoint}");
            request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            AddSessionCookie(request);

            return await _httpClient.SendAsync(request);
        }

        // Generic DELETE method
        public async Task<HttpResponseMessage> DeleteAsync(string endpoint)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/{endpoint}");
            AddSessionCookie(request);

            return await _httpClient.SendAsync(request);
        }

        // Forward the MVC session cookie to the API
        private void AddSessionCookie(HttpRequestMessage request)
        {
            var apiCookie = _httpContextAccessor.HttpContext?.Session.GetString("ApiCookie");
            if (!string.IsNullOrEmpty(apiCookie))
            {
                request.Headers.Add("Cookie", apiCookie);
            }
        }

        // Save the API's session cookie into MVC session
        private void CaptureSessionCookie(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var sessionCookie = cookies.FirstOrDefault(c => c.Contains(".RaceDayApi.Session"));
                if (sessionCookie != null)
                {
                    _httpContextAccessor.HttpContext?.Session.SetString("ApiCookie", sessionCookie);
                }
            }
        }
    }
}