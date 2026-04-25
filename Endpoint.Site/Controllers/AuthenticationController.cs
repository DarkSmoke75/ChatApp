using ChatApp.Application.Services.ApiClient;
using ChatApp.Application.Services.Users.Commands.UserLogin;
using ChatApp.Common.Dto;
using Endpoint.Site.Models.Dtos.Authentications;
using Endpoint.Site.Models.ViewModels.AuthenticationViewModel;
using Microsoft.AspNetCore.Mvc;
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

            Response.Cookies.Append("AccessToken", result.Data, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return RedirectToAction("Index", "Home");
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

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("AccessToken");
            return RedirectToAction("Login", "Authentication");
        }
    }
}

