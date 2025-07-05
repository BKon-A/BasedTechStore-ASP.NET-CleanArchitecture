using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.Controllers
{
    public class WishlistController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
