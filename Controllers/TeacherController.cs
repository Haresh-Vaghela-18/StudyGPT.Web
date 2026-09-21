using Microsoft.AspNetCore.Mvc;
using StudyGPT.Web.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace StudyGPT.Web.Controllers
{
    public class TeacherController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public TeacherController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }


        // =========================================================
        // TEACHER DASHBOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client = CreateApiClient(token);

                var response =
                    await client.GetAsync("api/Dashboard/teacher");

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse =
                        await response.Content.ReadAsStringAsync();

                    ViewBag.Error =
                        $"Dashboard API Error: {(int)response.StatusCode} {response.StatusCode}. {errorResponse}";

                    return View(new TeacherDashboardViewModel());
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                using var document =
                    JsonDocument.Parse(json);

                // API response should contain:
                // {
                //     "data": {
                //          ...
                //     }
                // }

                if (!document.RootElement.TryGetProperty(
                        "data",
                        out var data))
                {
                    ViewBag.Error =
                        "Invalid teacher dashboard API response. 'data' property was not found.";

                    return View(new TeacherDashboardViewModel());
                }

                var dashboard =
                    JsonSerializer.Deserialize<TeacherDashboardViewModel>(
                        data.GetRawText(),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (dashboard == null)
                {
                    ViewBag.Error =
                        "Unable to convert dashboard API data.";

                    return View(new TeacherDashboardViewModel());
                }

                return View(dashboard);
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    $"Dashboard Error: {ex.Message}";

                return View(
                    new TeacherDashboardViewModel());
            }
        }

        // =========================================================
        // STUDENTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Students()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client = CreateApiClient(token);

                var response =
                    await client.GetAsync("api/User/students");

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    ViewBag.Error =
                        $"Students API Error: {(int)response.StatusCode} {response.StatusCode}. {error}";

                    return View(
                        new List<StudyGPT.Web.Models.StudentViewModel>());
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                var students =
                    JsonSerializer.Deserialize<
                        List<StudyGPT.Web.Models.StudentViewModel>>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

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
        // MATERIALS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Materials()
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client =
                    CreateApiClient(token);

                var response =
                    await client.GetAsync(
                        "api/Material/my-materials");

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    TempData["Error"] =
                        $"Materials API Error: {(int)response.StatusCode} {response.StatusCode}. {error}";

                    return View(
                        new List<TeacherMaterialViewModel>());
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                var materials =
                    JsonSerializer.Deserialize<
                        List<TeacherMaterialViewModel>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                return View(
                    materials ??
                    new List<TeacherMaterialViewModel>());
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                return View(
                    new List<TeacherMaterialViewModel>());
            }
        }


        // =========================================================
        // UPLOAD MATERIAL - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> UploadMaterial()
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client =
                    CreateApiClient(token);

                var response =
                    await client.GetAsync(
                        "api/Subject");

                if (response.IsSuccessStatusCode)
                {
                    var json =
                        await response.Content.ReadAsStringAsync();

                    var subjects =
                        JsonSerializer.Deserialize<
                            List<SubjectViewModel>>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });

                    ViewBag.Subjects =
                        subjects ??
                        new List<SubjectViewModel>();
                }
                else
                {
                    ViewBag.Subjects =
                        new List<SubjectViewModel>();

                    TempData["Error"] =
                        "Unable to load subjects.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Subjects =
                    new List<SubjectViewModel>();

                TempData["Error"] =
                    ex.Message;
            }

            return View();
        }


        // =========================================================
        // UPLOAD MATERIAL - POST
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> UploadMaterial(
            string title,
            string description,
            int subjectId,
            IFormFile file)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (file == null || file.Length == 0)
            {
                TempData["Error"] =
                    "Please select a file.";

                return RedirectToAction(
                    nameof(UploadMaterial));
            }

            try
            {
                var client =
                    CreateApiClient(token);

                using var form =
                    new MultipartFormDataContent();

                form.Add(
                    new StringContent(title ?? ""),
                    "title");

                form.Add(
                    new StringContent(description ?? ""),
                    "description");

                form.Add(
                    new StringContent(subjectId.ToString()),
                    "subjectId");

                using var stream =
                    file.OpenReadStream();

                using var fileContent =
                    new StreamContent(stream);

                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        file.ContentType);

                form.Add(
                    fileContent,
                    "file",
                    file.FileName);

                var response =
                    await client.PostAsync(
                        "api/Material/upload",
                        form);

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    TempData["Error"] =
                        $"Upload failed: {error}";

                    return RedirectToAction(
                        nameof(UploadMaterial));
                }

                TempData["Success"] =
                    "Study material uploaded successfully.";

                return RedirectToAction(
                    nameof(Materials));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                return RedirectToAction(
                    nameof(UploadMaterial));
            }
        }


        // =========================================================
        // SUBJECTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Subjects()
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client =
                    CreateApiClient(token);

                var response =
                    await client.GetAsync(
                        "api/Subject");

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    TempData["Error"] =
                        $"Subject API Error: {(int)response.StatusCode} {response.StatusCode}. {error}";

                    return View(
                        new List<SubjectViewModel>());
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                var subjects =
                    JsonSerializer.Deserialize<
                        List<SubjectViewModel>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                return View(
                    subjects ??
                    new List<SubjectViewModel>());
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                return View(
                    new List<SubjectViewModel>());
            }
        }


        // =========================================================
        // ADD SUBJECT
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> AddSubject(
            string subjectName,
            string description)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client =
                    CreateApiClient(token);

                var data = new { SubjectName = subjectName, Description = description };

                var json =
                    JsonSerializer.Serialize(data);

                using var content =
                    new StringContent(
                        json,
                        System.Text.Encoding.UTF8,
                        "application/json");

                var response =
                    await client.PostAsync(
                        "api/Subject",
                        content);

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    TempData["Error"] =
                        $"Unable to create subject. {error}";

                    return RedirectToAction(
                        nameof(Subjects));
                }

                TempData["Success"] =
                    "Subject added successfully.";

                return RedirectToAction(
                    nameof(Subjects));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                return RedirectToAction(
                    nameof(Subjects));
            }
        }


        // =========================================================
        // DELETE SUBJECT
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> DeleteSubject(int id)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client =
                    CreateApiClient(token);

                var response =
                    await client.DeleteAsync(
                        $"api/Subject/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    TempData["Error"] =
                        $"Unable to delete subject. {error}";

                    return RedirectToAction(
                        nameof(Subjects));
                }

                TempData["Success"] =
                    "Subject deleted successfully.";

                return RedirectToAction(
                    nameof(Subjects));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                return RedirectToAction(
                    nameof(Subjects));
            }
        }


        // =========================================================
        // EDIT MATERIAL
        // =========================================================

        [HttpGet]
        public IActionResult EditMaterial(int id)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            TempData["Error"] =
                "Edit Material is not connected yet.";

            return RedirectToAction(
                nameof(Materials));
        }


        // =========================================================
        // DELETE MATERIAL
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client =
                    CreateApiClient(token);

                var response =
                    await client.DeleteAsync(
                        $"api/Material/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var error =
                        await response.Content.ReadAsStringAsync();

                    TempData["Error"] =
                        $"Unable to delete material. {error}";

                    return RedirectToAction(
                        nameof(Materials));
                }

                TempData["Success"] =
                    "Material deleted successfully.";

                return RedirectToAction(
                    nameof(Materials));
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    ex.Message;

                return RedirectToAction(
                    nameof(Materials));
            }
        }


        // =========================================================
        // TEACHER PROFILE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
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
                    var error =
                        await response.Content.ReadAsStringAsync();

                    ViewBag.Error =
                        $"Unable to load teacher profile. {error}";

                    return View(
                        new ProfileViewModel());
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                var profile =
                    JsonSerializer.Deserialize<
                        ProfileViewModel>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                return View(
                    profile ??
                    new ProfileViewModel());
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    ex.Message;

                return View(
                    new ProfileViewModel());
            }
        }


        // =========================================================
        // CREATE API CLIENT
        // =========================================================

        private HttpClient CreateApiClient(string token)
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
}