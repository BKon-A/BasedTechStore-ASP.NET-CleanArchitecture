namespace BasedTechStore.Common.Models.Api
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public int StatusCode { get; set; }
        public IEnumerable<string>? Errors { get; set; } = new List<string>();
    }
}