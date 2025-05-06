using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Application.DTOs.Specifications
{
    public class SpecificationTypeDto
    {
        public Guid Id { get; set; }
        public Guid SpecificationCategoryId { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public bool IsFilterable { get; set; }
        public int DisplayOrder { get; set; }
        public string SpecificationCategoryName { get; set; }
    }
}
