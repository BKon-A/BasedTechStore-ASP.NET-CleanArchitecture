using BasedTechStore.Web.ViewModels.Specifications;

namespace BasedTechStore.Web.ViewModels.PendingChanges
{
    public class SpecsPendingChangesVM
    {
        public List<SpecificationCategoryVM> CreatedCategories { get; set; } = new List<SpecificationCategoryVM>();
        public List<SpecificationCategoryVM> UpdatedCategories { get; set; } = new List<SpecificationCategoryVM>();
        public List<SpecificationCategoryVM> DeletedCategories { get; set; } = new List<SpecificationCategoryVM>();
        public List<SpecificationTypeVM> CreatedTypes { get; set; } = new List<SpecificationTypeVM>();
        public List<SpecificationTypeVM> UpdatedTypes { get; set; } = new List<SpecificationTypeVM>();
        public List<SpecificationTypeVM> DeletedTypes { get; set; } = new List<SpecificationTypeVM>();
    }
}
