using Microsoft.AspNetCore.Mvc;
using StudyGPT.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace StudyGPT.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public UserController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // =========================================================
        // GET LOGGED-IN USER PROFILE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "Login",
                    "Auth");
            }

            try
            {
                var client =
                    CreateApiClient(token);

                var response =
                    await client.GetAsync(
                        "api/User/profile");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error =
                        await response.Content
                            .ReadAsStringAsync();

                    return View();
                }

                var profile =
                    await response.Content
                        .ReadFromJsonAsync<UserProfileViewModel>();

                if (profile == null)
                {
                    ViewBag.Error =
                        "Unable to load user profile.";

                    return View();
                }

                return View(profile);
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    ex.Message;

                return View();
            }
        }

        // =========================================================
        // GET ALL STUDENTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Students()
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction(
                    "Login",
                    "Auth");
            }

            try
            {
                var client =
                    CreateApiClient(token);

                var response =
                    await client.GetAsync(
                        "api/User/students");

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content
                            .ReadAsStringAsync();

                    ViewBag.Error =
                        $"Students API Error: " +
                        $"{(int)response.StatusCode} " +
                        $"{response.StatusCode}. " +
                        $"{error}";

                    return View(
                        new List<StudyGPT.Web.Models.StudentViewModel>());
                }

                var students =
                    await response.Content
                        .ReadFromJsonAsync<
                            List<StudyGPT.Web.Models.StudentViewModel>>();

                return View(
                    students ??
                    new List<StudyGPT.Web.Models.StudentViewModel>());
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    $"Students Error: {ex.Message}";

                return View(
                    new List<StudyGPT.Web.Models.StudentViewModel>());
            }
        }

        // =========================================================
        // CREATE API CLIENT
        // =========================================================

        private HttpClient CreateApiClient(
            string token)
        {
            var client =
                _httpClientFactory.CreateClient();

            var baseUrl =
                _configuration[
                    "ApiSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "ApiSettings:BaseUrl is missing in appsettings.json");
            }

            client.BaseAddress =
                new Uri(baseUrl);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            return client;
        }
    }

    // =============================================================
    // USER PROFILE VIEW MODEL
    // =============================================================

    public class UserProfileViewModel
    {
        public int Id { get; set; }

        public string FullName { get; set; }
            = string.Empty;

        public string Email { get; set; }
            = string.Empty;

        public string Role { get; set; }
            = string.Empty;
    }
}