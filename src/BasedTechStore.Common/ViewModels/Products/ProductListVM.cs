using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Common.ViewModels.Products
{
    public class ProductListVM
    {
        public List<ProductItemVM> Products { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
