using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
