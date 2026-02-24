namespace BasedTechStore.Application.DTOs.Specifications
{
    public sealed record ProductSpecificationDto
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
