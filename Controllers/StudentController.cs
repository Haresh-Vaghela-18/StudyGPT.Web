using Microsoft.AspNetCore.Mvc;
using StudyGPT.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace StudyGPT.Web.Controllers
{
    public class StudentController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public StudentController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }


        // =========================================================
        // STUDENT DASHBOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client = CreateApiClient(token);

                var response =
                    await client.GetAsync("api/Dashboard/student");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error =
                        $"Dashboard API Error: {(int)response.StatusCode} {response.StatusCode}";

                    return View(new DashboardViewModel());
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                using var document =
                    JsonDocument.Parse(json);

                if (!document.RootElement.TryGetProperty(
                        "data",
                        out var data))
                {
                    ViewBag.Error =
                        "Invalid dashboard API response.";

                    return View(new DashboardViewModel());
                }

                var dashboard =
                    JsonSerializer.Deserialize<DashboardViewModel>(
                        data.GetRawText(),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                return View(
                    dashboard ?? new DashboardViewModel()
                );
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                return View(new DashboardViewModel());
            }
        }


        // =========================================================
        // SUBJECT LIST
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
                var client = CreateApiClient(token);

                var response =
                    await client.GetAsync("api/Subject");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error =
                        $"API Error: {(int)response.StatusCode} {response.StatusCode}";

                    return View(
                        new List<SubjectViewModel>()
                    );
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                var subjects =
                    JsonSerializer.Deserialize<List<SubjectViewModel>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                return View(
                    subjects ?? new List<SubjectViewModel>()
                );
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                return View(
                    new List<SubjectViewModel>()
                );
            }
        }


        // =========================================================
        // SUBJECT MATERIALS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> SubjectMaterials(int id)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (id <= 0)
            {
                ViewBag.Error =
                    "Invalid subject selected.";

                return View(
                    new List<StudyMaterialViewModel>()
                );
            }

            try
            {
                var client = CreateApiClient(token);

                var response =
                    await client.GetAsync(
                        $"api/Material/subject/{id}"
                    );

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error =
                        $"API Error: {(int)response.StatusCode} {response.StatusCode}";

                    ViewBag.SubjectId = id;

                    return View(
                        new List<StudyMaterialViewModel>()
                    );
                }

                var json =
                    await response.Content.ReadAsStringAsync();

                var materials =
                    JsonSerializer.Deserialize<
                        List<StudyMaterialViewModel>>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                ViewBag.SubjectId = id;

                return View(
                    materials ??
                    new List<StudyMaterialViewModel>()
                );
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.SubjectId = id;

                return View(
                    new List<StudyMaterialViewModel>()
                );
            }
        }


        // =========================================================
        // VIEW / OPEN MATERIAL
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> DownloadMaterial(int id)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client = CreateApiClient(token);

                var response =
                    await client.GetAsync(
                        $"api/Material/download/{id}"
                    );

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] =
                        $"Unable to open material. API returned {(int)response.StatusCode}.";

                    return RedirectToAction("Subjects");
                }

                var fileBytes =
                    await response.Content.ReadAsByteArrayAsync();

                var contentType =
                    response.Content.Headers.ContentType?.MediaType
                    ?? "application/octet-stream";

                return File(
                    fileBytes,
                    contentType
                );
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Unable to open material: {ex.Message}";

                return RedirectToAction("Subjects");
            }
        }


        // =========================================================
        // AI ASSISTANT PAGE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> AIAssistant(int? subjectId)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var client = CreateApiClient(token);

                // -------------------------------------------------
                // DEFAULT VALUES
                // -------------------------------------------------

                ViewBag.SubjectId =
                    subjectId.GetValueOrDefault(0);

                ViewBag.SubjectName =
                    "AI Assistant";

                ViewBag.Subjects =
                    new List<SubjectViewModel>();


                // -------------------------------------------------
                // LOAD SUBJECTS
                // -------------------------------------------------

                var subjectResponse =
                    await client.GetAsync("api/Subject");

                if (subjectResponse.IsSuccessStatusCode)
                {
                    var subjectJson =
                        await subjectResponse.Content
                            .ReadAsStringAsync();

                    var subjects =
                        JsonSerializer.Deserialize<
                            List<SubjectViewModel>>(
                            subjectJson,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            })
                        ?? new List<SubjectViewModel>();

                    ViewBag.Subjects = subjects;


                    // -------------------------------------------------
                    // SET SELECTED SUBJECT
                    // -------------------------------------------------

                    if (subjectId.HasValue &&
                        subjectId.Value > 0)
                    {
                        var selectedSubject =
                            subjects.FirstOrDefault(
                                x => x.SubjectId == subjectId.Value
                            );

                        if (selectedSubject != null)
                        {
                            ViewBag.SubjectId =
                                selectedSubject.SubjectId;

                            ViewBag.SubjectName =
                                selectedSubject.SubjectName;
                        }
                        else
                        {
                            ViewBag.SubjectId = 0;

                            ViewBag.SubjectName =
                                "AI Assistant";

                            ViewBag.Error =
                                "The selected subject could not be found.";
                        }
                    }
                }
                else
                {
                    ViewBag.Error =
                        "Unable to load subjects from the API.";

                    ViewBag.Subjects =
                        new List<SubjectViewModel>();
                }


                // -------------------------------------------------
                // LOAD CHAT SESSIONS
                // -------------------------------------------------

                var chatResponse =
                    await client.GetAsync(
                        "api/Chat/sessions"
                    );

                if (chatResponse.IsSuccessStatusCode)
                {
                    var json =
                        await chatResponse.Content
                            .ReadAsStringAsync();

                    ViewBag.ChatSessions =
                        JsonSerializer.Deserialize<
                            List<JsonElement>>(
                            json,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            })
                        ?? new List<JsonElement>();
                }
                else
                {
                    ViewBag.ChatSessions =
                        new List<JsonElement>();
                }


                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    ex.Message;

                ViewBag.SubjectId =
                    subjectId.GetValueOrDefault(0);

                ViewBag.SubjectName =
                    "AI Assistant";

                ViewBag.Subjects =
                    new List<SubjectViewModel>();

                ViewBag.ChatSessions =
                    new List<JsonElement>();

                return View();
            }
        }


        // =========================================================
        // NEW AI CHAT SESSION
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> NewChat(
            [FromBody] CreateChatSessionViewModel model)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new
                {
                    message =
                        "Your session has expired. Please login again."
                });
            }

            // IMPORTANT:
            // SubjectId must be greater than 0.

            if (model == null)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid request."
                });
            }

            if (model.SubjectId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid subject. Please select a subject first."
                });
            }

            try
            {
                var client =
                    CreateApiClient(token);


                // Send subjectId to API.

                var request = new
                {
                    subjectId = model.SubjectId
                };


                Console.WriteLine(
                    $"Creating chat session for SubjectId: {model.SubjectId}"
                );


                var response =
                    await client.PostAsJsonAsync(
                        "api/Chat/session",
                        request
                    );


                var responseContent =
                    await response.Content
                        .ReadAsStringAsync();


                Console.WriteLine(
                    $"Chat Session API Response: {responseContent}"
                );


                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        new
                        {
                            message = responseContent
                        }
                    );
                }


                return Content(
                    responseContent,
                    "application/json"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Unable to create new chat session.",

                        error =
                            ex.Message
                    }
                );
            }
        }


        // =========================================================
        // GET CHAT SESSIONS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetChatSessions()
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            try
            {
                var client =
                    CreateApiClient(token);

                var response =
                    await client.GetAsync(
                        "api/Chat/sessions"
                    );

                var content =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        new
                        {
                            message = content
                        }
                    );
                }

                return Content(
                    content,
                    "application/json"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Unable to load chat sessions.",

                        error =
                            ex.Message
                    }
                );
            }
        }


        // =========================================================
        // GET CHAT HISTORY
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetChatHistory(int id)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }

            if (id <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid chat session."
                });
            }

            try
            {
                var client =
                    CreateApiClient(token);

                var response =
                    await client.GetAsync(
                        $"api/Chat/session/{id}"
                    );

                var content =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        new
                        {
                            message = content
                        }
                    );
                }

                return Content(
                    content,
                    "application/json"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Unable to load chat history.",

                        error =
                            ex.Message
                    }
                );
            }
        }


        // =========================================================
        // ASK AI
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> AskAI(
            [FromBody] AIQuestionViewModel model)
        {
            var token =
                HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new
                {
                    message =
                        "Your session has expired. Please login again."
                });
            }

            if (model == null ||
                string.IsNullOrWhiteSpace(model.Question))
            {
                return BadRequest(new
                {
                    message =
                        "Please enter a question."
                });
            }

            try
            {
                // -------------------------------------------------
                // GET CHAT SESSION ID
                // -------------------------------------------------

                if (!Request.Headers.TryGetValue(
                        "X-Chat-Session-Id",
                        out var sessionHeader))
                {
                    return BadRequest(new
                    {
                        message =
                            "Chat session is required. Please select a subject first."
                    });
                }


                if (!int.TryParse(
                        sessionHeader.ToString(),
                        out int chatSessionId) ||
                    chatSessionId <= 0)
                {
                    return BadRequest(new
                    {
                        message =
                            "Invalid ChatSessionId."
                    });
                }


                var client =
                    CreateApiClient(token);


                var request = new
                {
                    chatSessionId =
                        chatSessionId,

                    question =
                        model.Question
                };


                var response =
                    await client.PostAsJsonAsync(
                        "api/Chat/ask",
                        request
                    );


                var responseContent =
                    await response.Content.ReadAsStringAsync();


                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        (int)response.StatusCode,
                        new
                        {
                            message =
                                responseContent
                        }
                    );
                }


                return Content(
                    responseContent,
                    "application/json"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        message =
                            "Error connecting to AI API.",

                        error =
                            ex.Message
                    }
                );
            }
        }


        // =========================================================
        // STUDENT PROFILE
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
                        "api/User/profile"
                    );

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error =
                        await response.Content
                            .ReadAsStringAsync();

                    return View(
                        new ProfileViewModel()
                    );
                }

                var profile =
                    await response.Content
                        .ReadFromJsonAsync<ProfileViewModel>();

                if (profile == null)
                {
                    ViewBag.Error =
                        "Unable to load profile.";

                    return View(
                        new ProfileViewModel()
                    );
                }

                return View(profile);
            }
            catch (Exception ex)
            {
                ViewBag.Error =
                    ex.Message;

                return View(
                    new ProfileViewModel()
                );
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
                    "ApiSettings:BaseUrl"
                ];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException(
                    "ApiSettings:BaseUrl is missing in appsettings.json"
                );
            }

            client.BaseAddress =
                new Uri(baseUrl);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token
                );

            return client;
        }
    }
}