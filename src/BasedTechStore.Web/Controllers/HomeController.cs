using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.Controllers
{
    public class HomeController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}
