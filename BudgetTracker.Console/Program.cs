using BudgetTracker.Console;
using BudgetTracker.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.InteropServices;

// Clear terminal
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    Process.Start("cmd", "/c cls").WaitForExit();
}
else
{
    Process.Start("clear").WaitForExit();
}

var serviceProvider = AppInitializer.InitializeServices();

var menuService = serviceProvider.GetRequiredService<MenuService>();
menuService.Run();