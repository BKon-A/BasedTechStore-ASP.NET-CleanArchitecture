using BasedTechStore.Common.ViewModels.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Common.ViewModels.Products
{
    public class ProductFilterOptionsVM
    {
        public List<CategoryItemVM> Categories { get; set; } = new();
        public List<string?> Brands { get; set; } = new();
        public List<SpecificationFilterGroupVM> FilterableSpecifications { get; set; } = new();
    }
}
