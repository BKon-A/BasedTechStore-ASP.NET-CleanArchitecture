using BasedTechStore.Common.ViewModels.Categories;

namespace BasedTechStore.Common.ViewModels.Products
{
    public class ProductFilterVM
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<CategoryItemVM>? Categories { get; set; } = new List<CategoryItemVM>();
        public List<Guid>? SelectedCategoryIds { get; set; } = new List<Guid>();
        public List<Guid>? SelectedSubCategoryIds { get; set; } = new List<Guid>(); 
        public List<string>? Brands { get; set; } = new List<string>();
        public List<string>? SelectedBrands { get; set; } = new List<string>();
        public List<SpecificationFilterGroupVM>? FilterableSpecifications { get; set; } = new List<SpecificationFilterGroupVM>();

        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 12;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; } = "asc";
    }

    public class SpecificationFilterGroupVM
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<SpecificationFilterItemVM> Specifications { get; set; } = new List<SpecificationFilterItemVM>();
    }

    public class SpecificationFilterItemVM
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string MinValue { get; set; }
        public string MaxValue { get; set; }
        public bool IsFilterable { get; set; } = true;
    }

    
}
