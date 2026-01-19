using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Common.Models.Api.Products
{
    public class ProductFilterRequest
    {
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public List<Guid>? SelectedCategoryIds { get; set; }
        public List<Guid>? SelectedSubCategoryIds { get; set; }
        public List<string>? SelectedBrands { get; set; }
    }
}
