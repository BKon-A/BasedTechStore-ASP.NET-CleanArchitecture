using BasedTechStore.Application.DTOs.Identity.Request;
using BasedTechStore.Application.DTOs.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BasedTechStore.Web.Controllers
{
    public class HomeController : BaseController
    {
        //public IActionResult Index(AuthModalViewModel model)
        //{
        //    return View(model ?? new AuthModalViewModel
        //    {
        //        SignInRequest = new SignInRequest(),
        //        SignUpRequest = new SignUpRequest()
        //    });
        //}

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            var model = new AuthModalViewModel
            {
                SignInRequest = new SignInRequest(),
                SignUpRequest = new SignUpRequest()
            };

            if (TempData["SignInErrors"] is string signInErrors)
            {
                ModelState.AddModelError("SignIn", signInErrors);
                ViewData["OpenModal"] = "authModal";
            }

            if (TempData["SignUpErrors"] is string signUpErrors)
            {
                ModelState.AddModelError("SignUp", signUpErrors);
                ViewData["OpenModal"] = "regModal";
            }

            return View(model);
        }
    }
}
