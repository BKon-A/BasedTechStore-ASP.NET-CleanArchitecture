using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Domain.Entities.Specifications
{
    public class SpecificationType
    {
        public Guid Id { get; set; }
        public Guid SpecificationCategoryId { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public bool isFilterable { get; set; }
        public int DisplayOrder { get; set; }

        // Navigation Properties
        public SpecificationCategory SpecificationCategory { get; set; }
        public ICollection<ProductSpecification> ProductSpecifications { get; set; }
    }
}