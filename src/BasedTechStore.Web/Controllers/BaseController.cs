using BasedTechStore.Application.DTOs.Identity.Request;
using BasedTechStore.Application.DTOs.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BasedTechStore.Web.Controllers
{
    public class BaseController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            
            if (ViewData.Model == null || ViewData.Model is not AuthModalViewModel)
            {
                
                ViewData.Model = new AuthModalViewModel
                {
                    SignInRequest = new SignInRequest(),
                    SignUpRequest = new SignUpRequest()
                };
            }

            base.OnActionExecuting(context);
        }
    }
}
