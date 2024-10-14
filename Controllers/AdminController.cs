using Lab2_MVC_Resto_Frontend.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Lab2_MVC_Resto_Frontend.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUri = "https://localhost:7212/api/";
        private readonly ILogger<AdminController> _logger;
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        public AdminController(HttpClient httpClient, ILogger<AdminController> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUri);
            _logger = logger;
        }

        [Authorize]
        //admin
        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM login)
        {
            try
            {
                _logger.LogInformation($"Sending a login request at time {DateTime.Now}.");
                var response = await _httpClient.PostAsJsonAsync($"accounts/login", login);
                _logger.LogInformation($"Login request sent successfully for {login.Email} as {login} at time {DateTime.Now}.");
                var json = await response.Content.ReadAsStringAsync();
                var token = JsonSerializer.Deserialize<TokenResponseVM>(json, _options);
                if (string.IsNullOrEmpty(token.Token))
                {
                    _logger.LogInformation($"No token obtained at time {DateTime.Now}.");
                }
                else
                {
                    _logger.LogInformation($"Token obtained at time {DateTime.Now}.");
                };

                // JWT token is a string consisting of 3 parts: Header (algorithm & token type), Payload (claims) and Signature (to check nobody modified payload).
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token.Token);

                // get claims and put them to ClaimsIdentity (one role/inlog) and ClaimsPrincipal (one user/entity)
                var claims = jwtToken.Claims.ToList();
                foreach (var claim in claims)
                {

                    Console.WriteLine("claim: " + claim);
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity); // en principal (användare/entitet) kan ha flera Identities

                _logger.LogInformation($"Signing in and creating a session cookie at time {DateTime.Now}.");
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
                {
                    IsPersistent = true,// user should stay logged in over different sessions????
                    ExpiresUtc = jwtToken.ValidTo
                });

                _logger.LogInformation($"Creating and saving a jwt-token in the browser at time {DateTime.Now}.");
                HttpContext.Response.Cookies.Append("jwtToken", token.Token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = jwtToken.ValidTo
                });

                return RedirectToAction("Index");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"HTTP request error when logging in or signing in at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "Unable to reach the API. Please try again later.";
                return View(login);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error when logging in or signing in at time {DateTime.Now}: {ex.Message}.");
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return View(login);
            }
        }
        [HttpPost]
        [Authorize] // borde den ha Authorize?
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation($"Removing session cookie at time {DateTime.Now}.");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            _logger.LogInformation($"Removing token cookie at time {DateTime.Now}.");
            HttpContext.Response.Cookies.Delete("jwtToken");

            return RedirectToAction("Login");
        }

    }
}
