using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace BasedTechStore.Application.Common.Interfaces.Persistence.Seed
{
    public interface IDbInitializer
    {
        Task InitializeDbAsync(IServiceProvider provider, IHostEnvironment environment);
        Task TestSqlServerConnectionAsync(string connectionString);
    }
}
