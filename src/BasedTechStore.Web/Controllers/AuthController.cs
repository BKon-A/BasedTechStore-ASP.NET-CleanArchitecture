using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Application.DTOs.Identity.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult SignIn()
        {
            var model = new AuthModalViewModel
            {
                SignUpRequest = new SignUpRequest(),
                SignInRequest = new SignInRequest()
            };

            ViewData["OpenModal"] = "authModal"; // ID твого модального вікна
            return View("~/Views/Home/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInRequest signInRequest)
        {
            if (!ModelState.IsValid)
            {
                TempData["SignInErrors"] = "Invalid login attempt.";
                TempData["OpenModal"] = "authModal";
                return RedirectToAction("Index", "Home");
            }

            var response = await _authService.SignInAsync(signInRequest);
            if (!response.IsSuccess)
            {
                TempData["SignInErrors"] = string.Join(", ", response.Errors);
                TempData["OpenModal"] = "authModal";
                return RedirectToAction("Index", "Home");
            }

            if (!double.TryParse(await _authService.GetJwtExpirationMinutes(), out var expirationMinutes))
                expirationMinutes = 60;

            Response.Cookies.Append("access-token", response.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes)
            });

            return RedirectToAction("Index", "Profile");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpRequest signUpRequest)
        {
            if (!ModelState.IsValid)
            {
                TempData["SignUpErrors"] = "Invalid fields";
                return RedirectToAction("Index", "Home");
            }

            var response = await _authService.SignUpAsync(signUpRequest);
            if (!response.IsSuccess)
            {
                TempData["SignUpErrors"] = string.Join(", ", response.Errors);
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index", "Profile");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignOutPost()
        {
            await _authService.SignOutAsync();
            Response.Cookies.Delete("access-token");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> SignOutGet()
        {
            await _authService.SignOutAsync();
            Response.Cookies.Delete("access-token");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult RefreshToken()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Token is required.");
                return View();
            }

            var response = await _authService.RefreshJwtTokenAsync(token);
            if (!response.IsSuccess)
            {
                ModelState.AddModelError("", string.Join(", ", response.Errors));
                return View();
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
