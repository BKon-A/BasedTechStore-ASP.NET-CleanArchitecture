namespace BasedTechStore.Application.DTOs.Categories
{
    public sealed record SubCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public Guid CategoryId { get; set; }
    }
}
