using BasedTechStore.Application.Common.Interfaces.Persistence;
using BasedTechStore.Application.Common.Interfaces.Persistence.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BasedTechStore.Infrastructure.Persistence.Seed
{
    public class DbInitializer : IDbInitializer
    {
        private readonly IAppDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(IAppDbContext context, IServiceProvider serviceProvider, 
            ILogger<DbInitializer> logger, IConfiguration configuration)
        {
            _context = context;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InitializeDbAsync(IServiceProvider serviceProvider, IHostEnvironment environment)
        {
            const int maxRetries = 5;
            const int delaySeconds = 5;

            using var scope = serviceProvider.CreateScope();

            var connectionString = _configuration.GetConnectionString("SqlServerConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'SqlServerConnection' is not configured.");
            }
            _logger.LogInformation("Using connection string: {connectionString}", connectionString.Replace("Password=", "Password=****"));

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation("=> Database configuration attempt {attempt}/{maxRetries}", attempt, maxRetries);
                    await TestSqlServerConnectionAsync(connectionString);

                    _logger.LogInformation("=> Testing EF database connection...");
                    var canConnect = await _context.CanConnectAsync();
                    if (!canConnect)
                    {
                        throw new InvalidOperationException("EF Core reports database is not accessible");
                    }
                    _logger.LogInformation("=> => Database connection successful.");

                    _logger.LogInformation("Checking if database exists...");
                    var databaseExists = await _context.CanConnectAsync();
                    _logger.LogInformation("Database exists: {DatabaseExists}", databaseExists);

                    _logger.LogInformation("Checking pending migrations...");
                    var pendingMigrations = await _context.GetPendingMigrationsAsync();
                    var pendingMigrationsList = pendingMigrations.ToList();

                    if (pendingMigrationsList.Any())
                    {
                        _logger.LogInformation("Found {Count} pending migrations: {Migrations}",
                            pendingMigrationsList.Count, string.Join(", ", pendingMigrationsList));

                        _logger.LogInformation("Applying database migrations...");
                        await _context.MigrateAsync();
                        _logger.LogInformation("✓ Database migrations applied successfully");
                    }
                    else
                    {
                        _logger.LogInformation("✓ No pending migrations found");
                    }

                    _logger.LogInformation("Verifying applied migrations...");
                    var appliedMigrations = await _context.GetAppliedMigrationsAsync();
                    _logger.LogInformation("Applied migrations count: {Count}", appliedMigrations.Count());

                    // Ensure data protection keys directory exists
                    var dataProtectionKeysPath = "/app/data-protection-keys";
                    if (!Directory.Exists(dataProtectionKeysPath))
                    {
                        Directory.CreateDirectory(dataProtectionKeysPath);
                        _logger.LogInformation("✓ Created data protection keys directory: {Path}", dataProtectionKeysPath);
                    }

                    // Database seeding in development
                    if (environment.IsDevelopment())
                    {
                        _logger.LogInformation("Development environment - starting database seeding...");
                        var seeder = _serviceProvider.GetRequiredService<IManagerSeeder>();
                        await seeder.SeedManagerAsync();
                        _logger.LogInformation("✓ Database seeding completed successfully");
                    }

                    _logger.LogInformation("🎉 Database initialization completed successfully!");
                    return; // Success - exit the retry loop
                }
                catch (SqlException sqlEx)
                {
                    // Detailed SQL error handling and logging
                    _logger.LogError(sqlEx, "❌ SQL Server error (attempt {Attempt}/{MaxRetries}): {ErrorNumber} - {Message}",
                        attempt, maxRetries, sqlEx.Number, sqlEx.Message);

                    switch (sqlEx.Number)
                    {
                        case 2: // Network error
                        case 26: // Error Locating Server/Instance Specified
                            _logger.LogWarning("💡 Network connectivity issue. Check if SQL Server container is running and accessible.");
                            break;
                        case 18456: // Login failed
                            _logger.LogWarning("💡 Authentication issue. Check username/password in connection string.");
                            break;
                        case 1225: // Database not accessible
                            _logger.LogWarning("💡 Database access issue. Check if database exists and user has permissions.");
                            break;
                    }
                }
                catch (InvalidOperationException opEx) when (opEx.Message.Contains("database"))
                {
                    _logger.LogError(opEx, "❌ Database operation error (attempt {Attempt}/{MaxRetries}): {Message}",
                        attempt, maxRetries, opEx.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ General error during database initialization (attempt {Attempt}/{MaxRetries}): {Error}",
                        attempt, maxRetries, ex.Message);
                }

                if (attempt == maxRetries)
                {
                    _logger.LogCritical("💀 Database initialization failed after {MaxRetries} attempts. Application cannot start.", maxRetries);

                    if (environment.IsProduction())
                    {
                        Environment.Exit(1);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Development environment - application will continue but may not function correctly.");
                        return;
                    }
                }

                var delay = TimeSpan.FromSeconds(delaySeconds * Math.Min(attempt, 3));
                _logger.LogInformation("⏳ Waiting {DelaySeconds} seconds before retry {NextAttempt}...",
                    delay.TotalSeconds, attempt + 1);
                await Task.Delay(delay);
            }
        }

        public async Task TestSqlServerConnectionAsync(string connectionString)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(connectionString);
                var server = builder.DataSource;

                _logger.LogInformation("Attempting raw SQL connection to: {Server}", server);

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                using var command = new SqlCommand("SELECT 1", connection);
                var result = await command.ExecuteScalarAsync();

                _logger.LogInformation("✓ Raw SQL connection test successful");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Raw SQL connection test failed: {Message}", ex.Message);
                throw;
            }
        }
    }
}
