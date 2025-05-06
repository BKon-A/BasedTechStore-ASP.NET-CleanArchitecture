namespace BasedTechStore.Application.DTOs.Specifications
{
    public class SpecificationCategoryDto
    {
        public Guid Id { get; set; }
        public Guid ProductCategoryId { get; set; }
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public string ProductCategoryName { get; set; }
    }
}
