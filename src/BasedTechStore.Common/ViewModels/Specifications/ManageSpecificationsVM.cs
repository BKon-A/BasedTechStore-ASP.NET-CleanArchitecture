using BasedTechStore.Common.ViewModels.Categories;
using BasedTechStore.Common.ViewModels.PendingChanges;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasedTechStore.Common.ViewModels.Specifications
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
