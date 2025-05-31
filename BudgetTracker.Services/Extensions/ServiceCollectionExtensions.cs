using BudgetTracker.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BudgetTracker.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IBudgetService, BudgetService>();
            services.AddSingleton<IExpenseService, ExpenseService>();
            services.AddSingleton<ICategoryService, CategoryService>();
            services.AddSingleton<MenuService>();
            services.AddSingleton<MenuDisplayService>();
        }
    }
}
