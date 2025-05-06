using BasedTechStore.Web.ViewModels.Modals;
using Microsoft.AspNetCore.Mvc;

namespace BasedTechStore.Web.ViewComponents
{
    public class InfoModalViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(InfoModalViewModel model = null)
        {
            if (model == null)
            {
                model = new InfoModalViewModel();
            }

            return View(model);
        }
    } 
}
