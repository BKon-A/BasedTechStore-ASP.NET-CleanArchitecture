namespace BasedTechStore.Application.DTOs.Products
{
    public sealed record ProductFiltersDto
    {
        public List<string> Brands { get; set; } = new();
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public Dictionary<string, List<string>> SpecOptions { get; set; } = new();
    }
}
