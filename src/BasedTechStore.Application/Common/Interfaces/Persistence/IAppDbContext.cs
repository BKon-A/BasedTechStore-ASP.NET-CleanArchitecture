namespace BasedTechStore.Application.Common.Interfaces.Persistence
{
    public interface IAppDbContext
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default);
        Task MigrateAsync(CancellationToken cancellationToken = default);
    }
}
