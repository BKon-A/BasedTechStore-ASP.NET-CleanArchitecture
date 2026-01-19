using BasedTechStore.Application.Common.Interfaces.Services;
using BasedTechStore.Application.DTOs.Identity;
using BasedTechStore.Web.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignIn(SignInDto signInRequest)
        {
            if (Request.IsAjaxRequest())
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { 
                        success = false, 
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage).ToList() });
                }
                try
                {
                    var response = await _authService.SignInAsync(signInRequest);
                    if (!response.IsSuccess)
                    {
                        return Json(new { success = false, errors = response.Errors });
                    }

                    if (!double.TryParse(await _authService.GetJwtExpirationMinutes(), out var expirationMinutes))
                        expirationMinutes = 60;

                    Response.Cookies.Append("access-token", response.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                        Path = "/"
                    });

                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Profile") });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "An error occurred while signing in. Error: ", ex.Message } });
                }

            }

            return BadRequest(new { success = false, message = "Invalid request type." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(SignUpDto signUpRequest)
        {
            if (Request.IsAjaxRequest())
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { 
                        success = false, 
                        errors = ModelState.Values.SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage).ToList() });
                }

                try
                {
                    var response = await _authService.SignUpAsync(signUpRequest);
                    if (!response.IsSuccess)
                    {
                        return Json(new { success = false, errors = response.Errors });
                    }

                    if (!double.TryParse(await _authService.GetJwtExpirationMinutes(), out var expirationMinutes))
                        expirationMinutes = 60;

                    Response.Cookies.Append("access-token", response.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                        Path = "/"
                    });

                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Profile") });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, errors = new[] { "An error occurred while signing up. Error: ", ex.Message } });
                }
            }

            return BadRequest(new { success = false, message = "Invalid request type." });
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

        [HttpGet]
        public IActionResult CheckAuth()
        {
            return Json(new
            {
                isAuthenticated = User.Identity.IsAuthenticated,
                username = User.Identity.Name,
                claims = User.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList()
            });
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
