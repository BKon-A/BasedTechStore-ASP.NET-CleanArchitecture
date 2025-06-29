using BasedTechStore.Application.DTOs.Identity.Request;
using BasedTechStore.Application.DTOs.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BasedTechStore.Web.Controllers
{
    public class HomeController : Controller
    {
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
