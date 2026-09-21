using Microsoft.AspNetCore.Mvc;
using StudyGPT.Web.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace StudyGPT.Web.Controllers
{
    public class SubjectController : Controller
    {
        private readonly HttpClient _httpClient;

        public SubjectController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("Token");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(
                "https://localhost:7002/api/Subject"
            );

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to load subjects.";
                return View(new List<SubjectViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();

            var subjects = JsonSerializer.Deserialize<List<SubjectViewModel>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );

            return View(subjects ?? new List<SubjectViewModel>());
        }
    }
}