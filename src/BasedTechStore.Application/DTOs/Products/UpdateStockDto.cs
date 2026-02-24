using System.ComponentModel.DataAnnotations;

namespace BasedTechStore.Application.DTOs.Products
{
    public sealed record UpdateStockDto
    {
        [Required]
        public int Quantity { get; set; }
    }
}
