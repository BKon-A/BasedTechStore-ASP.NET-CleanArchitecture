namespace BasedTechStore.Domain.Constants
{
    public static class Roles
    {
        public const string Customer = "customer";
        public const string Manager = "manager";
        public const string Admin = "admin";
        public const string Analyst = "analyst";
        public const string Support = "support";

        public static IReadOnlyList<string> GetAll()
        {
            return new[]
            {
                Customer,
                Manager,
                Admin,
                Analyst,
                Support
            };
        }
    }
}
