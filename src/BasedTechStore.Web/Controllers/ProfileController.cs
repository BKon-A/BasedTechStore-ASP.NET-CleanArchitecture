using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.Controllers
{
    [Route("[controller]/[action]")]
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
