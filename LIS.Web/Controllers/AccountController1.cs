using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using مشروع_ادار_المختبرات.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using مشروع_ادار_المختبرات.DTOS;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class AccountController : Controller
    {
        private readonly HttpClient _httpClient;

        public AccountController( IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        public class LoginDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var loginData = new LoginDto
            {
                Email = model.Email,
                Password = model.Password
            };

            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(loginData),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("https://localhost:7116/api/Users/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = "خطأ في تسجيل الدخول: " + errorMsg;
                    return View(model);
                }

                var responseJson = await response.Content.ReadAsStringAsync();

                var user = JsonSerializer.Deserialize<LoginResponseDto>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (user == null)
                {
                    TempData["Error"] = "خطأ في بيانات المستخدم.";
                    return View(model);
                }

                // تخزين التوكن و UserID في السيشن
                HttpContext.Session.SetString("JWToken", user.Token);
                HttpContext.Session.SetInt32("UserID", user.UserId);

                // إنشاء Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = "حدث خطأ في الاتصال بالخادم: " + ex.Message;
                return View(model);
            }
        }

        public class LoginResponseDto
        {
            public string Token { get; set; }
            public int UserId { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
            public bool IsActive { get; set; }
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("JWToken");
            HttpContext.Session.Remove("UserID");
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Setting()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");

            if (userId == null)
                return RedirectToAction("Login");

            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"https://localhost:7116/api/Users/byiD?iD={userId}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "المستخدم غير موجود";
                    return NotFound();
                }

                var json = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<DTOEditUsers>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var dtoPatients = new DTOEditUsers
                {
                    UserID = users.UserID,
                    FullName = users.FullName,
                    Email = users.Email,
                    Password = users.Password,
                    IsActive = users.IsActive,
                };

                return View(dtoPatients);
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = "خطأ في الاتصال بالخادم: " + ex.Message;
                return View();
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return RedirectToAction("Login");

            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync($"https://localhost:7116/api/Users/byiD?iD={userId}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "المستخدم غير موجود";
                    return NotFound();
                }

                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<DTOEditUsers>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var dtoUser = new DTOEditUsers
                {
                    UserID = user.UserID,
                    FullName = user.FullName,
                    Password = user.Password,
                    Email = user.Email,
                    RoleId = user.RoleId,
                    IsActive = user.IsActive
                };

                return View(dtoUser);
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = "خطأ في الاتصال بالخادم: " + ex.Message;
                return View();
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DTOEditUsers user)
        {
            if (!ModelState.IsValid)
                return View(user);

            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(user),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PutAsync(
                    $"https://localhost:7116/api/Users/EditmyAccount?id={user.UserID}",
                    jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Successful"] = "تم تعديل بيانات المستخدم بنجاح";
                    return RedirectToAction(nameof(Setting));
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"حدث خطأ أثناء تعديل المستخدم: {errorResponse}";
                    return View(user);
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["Error"] = "خطأ في الاتصال بالخادم: " + ex.Message;
                return View(user);
            }
        }
    }

    public class UserDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
    }
}
