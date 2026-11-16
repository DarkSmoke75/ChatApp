using ChatApp.Application.Services.ApiClient;
using ChatApp.Application.Services.Users.Commands.UserLogin;
using ChatApp.Common.Dto;
using Endpoint.Site.Models.Dtos.Authentications;
using Endpoint.Site.Models.ViewModels.AuthenticationViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Endpoint.Site.Controllers
{
    public class AuthenticationController : Controller
    {

        private readonly IWebHostEnvironment _environment;
        private readonly IApiClientService _apiClientService;
        public AuthenticationController(IWebHostEnvironment environment, IApiClientService apiClientService)
        {

            _environment = environment;
            _apiClientService = apiClientService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginRequest)
        {

            if (!ModelState.IsValid)
            {
                return View(loginRequest);
            }
            var client = _apiClientService.CreateClientWithToken(HttpContext);


            var requestDto = new RequestUserLoginDto
            {
                Email = loginRequest.Email,
                Password = loginRequest.Password
            };

            var json = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/authentication/Login", content);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "ایمیل یا رمز عبور اشتباه است");
                return View(loginRequest);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<LoginResultDto>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || !result.IsSuccess || string.IsNullOrEmpty(result.Data))
            {
                ModelState.AddModelError("", "خطا در ورود");
                return View(loginRequest);
            }

            // Only read tokens returned directly by the trusted API login call above.
            // This is not validation of a token supplied by a browser.
            JwtSecurityToken token;
            try
            {
                token = new JwtSecurityTokenHandler().ReadJwtToken(result.Data);
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError("", "The server returned an invalid session. Please try again.");
                return View(loginRequest);
            }

            var userId = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(userId, out var parsedUserId) || parsedUserId <= 0 ||
                token.ValidTo <= DateTime.UtcNow)
            {
                ModelState.AddModelError("", "The server returned an invalid or expired session.");
                return View(loginRequest);
            }

            var expires = new DateTimeOffset(token.ValidTo, TimeSpan.Zero);
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId!),
                new Claim(ClaimTypes.Name,
                    token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? loginRequest.Email)
            }, CookieAuthenticationDefaults.AuthenticationScheme);
            var properties = new AuthenticationProperties
            {
                ExpiresUtc = expires,
                IsPersistent = false,
                AllowRefresh = false
            };
            properties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = result.Data }
            });
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity), properties);

            Response.Cookies.Append("AccessToken", result.Data, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = expires
            });

            return RedirectToAction("Index", "Conversation");
        }
        [HttpGet]
        public async Task<IActionResult> SignUp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel signUpRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(signUpRequest);
            }
            var client = _apiClientService.CreateClientWithToken(HttpContext);
            var requestDto = new RequestSignUpDto
            {
                Email = signUpRequest.Email,
                Password = signUpRequest.Password,
                RePassword = signUpRequest.RePassword,
                UserName = signUpRequest.Username,
                DisplayName = signUpRequest.DisplayName
            };
            var json = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/authentication/signup", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                ModelState.AddModelError("", errorMessage);
                return View(signUpRequest);
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ResultDto>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
            {
                ModelState.AddModelError("", "پاسخ نامعتبر از سرور");
                return View(signUpRequest);
            }

            if (!result.IsSuccess)
            {
                ModelState.AddModelError("", result.Message);
                return View(signUpRequest);
            }

            return RedirectToAction("Login", "Authentication");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("AccessToken");
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Authentication");
        }
    }
}

