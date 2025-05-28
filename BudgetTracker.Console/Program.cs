using BudgetTracker.Console;
using BudgetTracker.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.InteropServices;

// Clear terminal
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    await Process.Start("cmd", "/c cls").WaitForExitAsync();
}
else
{
    await Process.Start("clear").WaitForExitAsync();
}

var serviceProvider = AppInitializer.InitializeServices();

var menuService = serviceProvider.GetRequiredService<MenuService>();
menuService.Run();