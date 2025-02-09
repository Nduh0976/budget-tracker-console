using BudgetTracker.Console;
using BudgetTracker.Services;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = AppInitializer.InitializeServices();

var menuService = serviceProvider.GetRequiredService<MenuService>();
menuService.Run();