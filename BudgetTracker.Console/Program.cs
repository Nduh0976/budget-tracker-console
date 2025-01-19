using System.Text;
using BudgetTracker.Console.Constants;

ConfigureConsole();

var selectedMenuOption = DisplayMenuAndGetSelection();

Console.WriteLine($"{ForeColorConfig.GreenForeColor}You selected {MenuItems.ActiveMenuItems.ElementAt(selectedMenuOption)}{ForeColorConfig.ForeColorReset}");

static void ConfigureConsole()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("Welcome to Budget Tracker, take control of your finances!");
    Console.ResetColor();
    Console.WriteLine($"\nUse ⬆ and ⬇ to navigate and key {ForeColorConfig.GreenForeColor}Enter/Return{ForeColorConfig.ForeColorReset} to select.");
    Console.CursorVisible = false;
}

static int DisplayMenuAndGetSelection()
{
    if (MenuItems.ActiveMenuItems == null || MenuItems.ActiveMenuItems.Count == 0)
    {
        Console.WriteLine("No menu items available.");
        return -1; // Indicates no selection
    }

    var cursorPosition = Console.GetCursorPosition();
    var selectedOption = 0;
    var isSelected = false;

    while (!isSelected)
    {
        DrawMenu(cursorPosition, selectedOption);

        switch (Console.ReadKey(false).Key)
        {
            case ConsoleKey.DownArrow:
                selectedOption = (selectedOption + 1) % MenuItems.ActiveMenuItems.Count;
                break;

            case ConsoleKey.UpArrow:
                selectedOption = (selectedOption - 1 + MenuItems.ActiveMenuItems.Count) % MenuItems.ActiveMenuItems.Count;
                break;

            case ConsoleKey.Enter:
                isSelected = true;
                break;
        }
    }

    return selectedOption;
}

static void DrawMenu((int Left, int Top) cursorPosition, int selectedOption)
{
    Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);

    var menuBuilder = new StringBuilder();
    var selectedOptionMarker = $"✅  {ForeColorConfig.GreenForeColor}";

    for (var i = 0; i < MenuItems.ActiveMenuItems.Count; i++)
    {
        var marker = selectedOption == i ? selectedOptionMarker : "    ";
        menuBuilder.AppendLine($"{marker}{MenuItems.ActiveMenuItems.ElementAt(i)}{ForeColorConfig.ForeColorReset}");
    }

    Console.WriteLine(menuBuilder.ToString());
}