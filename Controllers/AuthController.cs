using Microsoft.AspNetCore.Mvc;
using StudyGPT.Web.Models;
using StudyGPT.Web.Services;
using System.Net.Http.Json;

namespace StudyGPT.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public AuthController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            JwtService jwtService)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _jwtService = jwtService;
        }


        // =====================================================
        // LOGIN - GET
        // =====================================================

        [HttpGet]
        public IActionResult Login()
        {
            // If user is already logged in,
            // send them to the correct dashboard.

            var token =
                HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var role =
                        _jwtService.GetRole(token);

                    if (role == "Student")
                    {
                        return RedirectToAction(
                            "Dashboard",
                            "Student");
                    }

                    if (role == "Teacher")
                    {
                        return RedirectToAction(
                            "Dashboard",
                            "Teacher");
                    }

                    if (role == "Admin")
                    {
                        return RedirectToAction(
                            "Dashboard",
                            "Admin");
                    }
                }
                catch
                {
                    // Invalid/expired token.
                    // Clear session and show login.
                    HttpContext.Session.Clear();
                }
            }

            return View();
        }


        // =====================================================
        // LOGIN - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client =
                    _httpClientFactory.CreateClient();

                var baseUrl =
                    _configuration["ApiSettings:BaseUrl"];

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "API Base URL is not configured.");

                    return View(model);
                }

                client.BaseAddress =
                    new Uri(baseUrl);


                // =================================================
                // CALL API LOGIN
                // =================================================

                var response =
                    await client.PostAsJsonAsync(
                        "api/Auth/login",
                        model);


                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Invalid email or password.");

                    return View(model);
                }


                // =================================================
                // READ LOGIN RESPONSE
                // =================================================

                var loginResponse =
                    await response.Content
                        .ReadFromJsonAsync<LoginResponse>();


                if (loginResponse == null ||
                    string.IsNullOrWhiteSpace(
                        loginResponse.Token))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Login response was invalid.");

                    return View(model);
                }


                // =================================================
                // SAVE JWT TOKEN IN SESSION
                // =================================================

                HttpContext.Session.SetString(
                    "JWToken",
                    loginResponse.Token);


                // =================================================
                // GET USER ROLE FROM JWT
                // =================================================

                var role =
                    _jwtService.GetRole(
                        loginResponse.Token);

                role =
                    role?.Trim() ?? string.Empty;


                // =================================================
                // ROLE BASED REDIRECT
                // =================================================

                if (role.Equals(
                        "Student",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(
                        "Dashboard",
                        "Student");
                }


                if (role.Equals(
                        "Teacher",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(
                        "Dashboard",
                        "Teacher");
                }


                if (role.Equals(
                        "Admin",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction(
                        "Dashboard",
                        "Admin");
                }


                // =================================================
                // UNKNOWN ROLE
                // =================================================

                HttpContext.Session.Clear();

                ModelState.AddModelError(
                    string.Empty,
                    $"Unknown user role: {role}");

                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the API: " +
                    ex.Message);

                return View(model);
            }
        }


        // =====================================================
        // REGISTER - GET
        // =====================================================

        [HttpGet]
        public IActionResult Register()
        {
            // Public registration is ONLY for students.

            var token =
                HttpContext.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var role =
     _jwtService.GetRole(token);

                    role = role?.Trim() ?? string.Empty;

                    if (role.Equals(
                            "Student",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction(
                            "Dashboard",
                            "Student");
                    }

                    if (role.Equals(
                            "Teacher",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction(
                            "Dashboard",
                            "Teacher");
                    }

                    if (role.Equals(
                            "Admin",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction(
                            "Dashboard",
                            "Admin");
                    }
                }
                catch
                {
                    HttpContext.Session.Clear();
                }
            }

            return View();
        }


        // =====================================================
        // REGISTER - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            RegisterViewModel model)
        {
            // =========================================
            // MODEL VALIDATION
            // =========================================

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            try
            {
                // =========================================
                // CREATE HTTP CLIENT
                // =========================================

                var client =
                    _httpClientFactory.CreateClient();


                // =========================================
                // GET API BASE URL
                // =========================================

                var baseUrl =
                    _configuration["ApiSettings:BaseUrl"];


                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "API Base URL is not configured.");

                    return View(model);
                }


                client.BaseAddress =
                    new Uri(baseUrl);


                // =========================================
                // CREATE REGISTER REQUEST
                // =========================================
                //
                // IMPORTANT:
                // Role is NOT taken from the user.
                //
                // Every public registration creates
                // a Student account.
                // =========================================

                var registerRequest = new
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    Role = "Student"
                };


                // =========================================
                // CALL API REGISTER
                // =========================================

                var response =
                    await client.PostAsJsonAsync(
                        "api/Auth/register",
                        registerRequest);


                // =========================================
                // REGISTRATION SUCCESS
                // =========================================

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] =
                        "Registration successful! " +
                        "Please login to continue.";

                    return RedirectToAction(
                        "Login",
                        "Auth");
                }


                // =========================================
                // REGISTRATION FAILED
                // =========================================

                var errorMessage =
                    await response.Content.ReadAsStringAsync();


                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        errorMessage);
                }
                else
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Registration failed. " +
                        "Please try again.");
                }


                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to connect to the API: " +
                    ex.Message);

                return View(model);
            }
        }


        // =====================================================
        // LOGOUT
        // =====================================================

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction(
                "Login",
                "Auth");
        }
    }
}