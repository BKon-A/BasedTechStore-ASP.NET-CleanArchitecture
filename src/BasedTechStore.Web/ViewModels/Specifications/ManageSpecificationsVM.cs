using BasedTechStore.Web.ViewModels.Categories;
using BasedTechStore.Web.ViewModels.PendingChanges;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasedTechStore.Web.ViewModels.Specifications
{
    public class ManageSpecificationsVM
    {
        public List<CategoryItemVM> Categories { get; set; } = new();

        public SpecsPendingChangesVM PendingChanges { get; set; } = new SpecsPendingChangesVM();
        public List<SelectListItem> CategoryOptions => Categories?.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList() ?? new List<SelectListItem>();
    }
}
