using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Application.DTOs.Specifications
{
    public class SpecificationCategoryGroupDto
    {
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<ProductSpecificationDto> Specifications { get; set; } = new List<ProductSpecificationDto>();
    }
}
