using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Application.DTOs.Specifications
{
    public class ProductSpecificationDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid SpecificationTypeId { get; set; }
        public string Value { get; set; }
        public string TypeName { get; set; }
        public string TypeUnit { get; set; }
        public string CategoryName { get; set; }
        public int DisplayOrder { get; set; }
    }
}
