using BudgetTracker.Data.Extensions;
using BudgetTracker.Services.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Console
{
    public static class AppInitializer
    {
        public static IServiceProvider InitializeServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddDataStore();
            serviceCollection.AddServices();
            
            return serviceCollection.BuildServiceProvider();
        }
    }
}
