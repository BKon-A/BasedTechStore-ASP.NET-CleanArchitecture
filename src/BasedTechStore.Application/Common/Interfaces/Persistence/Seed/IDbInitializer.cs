using Microsoft.Extensions.Hosting;

namespace BasedTechStore.Application.Common.Interfaces.Persistence.Seed
{
    public interface IDbInitializer
    {
        Task InitializeDbAsync(IServiceProvider provider, IHostEnvironment environment);
        Task TestSqlServerConnectionAsync(string connectionString);
    }
}
