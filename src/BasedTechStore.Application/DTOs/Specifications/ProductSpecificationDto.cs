﻿using System;
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
        public string Value { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string TypeUnit { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}
