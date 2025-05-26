using BasedTechStore.Domain.Entities.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Domain.Entities.Specifications
{
    public class SpecificationCategory
    {
        public Guid Id { get; set; }
        public Guid ProductCategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        // Navigation Properties
        public Category ProductCategory { get; set; } = null!;
        public ICollection<SpecificationType> SpecificationTypes { get; set; } = new List<SpecificationType>();
    }
}
