using Microsoft.EntityFrameworkCore.Query.Internal;

namespace BasedTechStore.Web.ViewModels.Categories
{
    public class SubCategoryItemVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Guid CategoryId { get; set; }
    }
}
