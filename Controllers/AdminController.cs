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
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        public AdminController(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUri);
        }

        [Authorize]
        //[Authorize]
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
            // make a login request and get token out of it
            var response = await _httpClient.PostAsJsonAsync($"accounts/login", login);
            if (!response.IsSuccessStatusCode)
            {
                return View(login);
            }
            Console.WriteLine("statuscode " + response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);
            var token = JsonSerializer.Deserialize<TokenResponseVM>(json, _options);

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

            // sign in and create a session cookie
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
            {
                IsPersistent = true,// user should stay logged in over different sessions????
                ExpiresUtc = jwtToken.ValidTo
            });

            // create and save a jwt-token in the browser
            HttpContext.Response.Cookies.Append("jwtToken", token.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = jwtToken.ValidTo
            });

            return RedirectToAction("Index");
        }
        [HttpPost]
        // borde den ha Authorize?
        public async Task<IActionResult> Logout()
        {
            // remove session cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // remove token cookie
            HttpContext.Response.Cookies.Delete("jwtToken");

            return RedirectToAction("Login");
        }

    }
}
