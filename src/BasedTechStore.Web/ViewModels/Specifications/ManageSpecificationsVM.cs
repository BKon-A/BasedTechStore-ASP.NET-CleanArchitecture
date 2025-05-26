using BasedTechStore.Web.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasedTechStore.Web.ViewModels.Specifications
{
    public class ManageSpecificationsVM
    {
        public List<CategoryItemVM> Categories { get; set; } = new();

        public PendingChangesVM PendingChanges { get; set; } = new PendingChangesVM();
        public List<SelectListItem> CategoryOptions => Categories?.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList() ?? new List<SelectListItem>();
    }
}
