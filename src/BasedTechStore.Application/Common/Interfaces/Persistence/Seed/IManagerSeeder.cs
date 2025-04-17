using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasedTechStore.Application.Common.Interfaces.Persistence.Seed
{
    public interface IManagerSeeder
    {
        Task SeedManagerAsync();
    }
}
