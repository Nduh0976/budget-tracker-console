using BudgetTracker.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDataStore(this IServiceCollection services)
        {
            // Register services
            services.AddSingleton<IDataStore, DataStore>();
        }
    }
}
