using BudgetTracker.Console.Constants;

ConfigureConsole();

var cursorPosition = Console.GetCursorPosition();
var selectedOption = 0;
var isSelected = false;

while (!isSelected)
{
    Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);

    var selectedOptionMarker = $"✅  {ForeColorConfig.GreenForeColor}";
    for (var i = 0; i < MenuItems.ActiveMenuItems.Count; i++)
    {
        Console.WriteLine($"{(selectedOption == i ? selectedOptionMarker : "    ")}{MenuItems.ActiveMenuItems.ElementAt(i)}\u001b[0m");
    }

    var key = Console.ReadKey(false);

    switch (key.Key)
    {
        case ConsoleKey.DownArrow:
            selectedOption = selectedOption == MenuItems.ActiveMenuItems.Count - 1
                ? 0
                : selectedOption + 1;
            break;

        case ConsoleKey.UpArrow:
            selectedOption = selectedOption == 0
                ? MenuItems.ActiveMenuItems.Count - 1
                : selectedOption - 1;
            break;

        case ConsoleKey.Enter:
            isSelected = true;
            break;
    }
}

Console.WriteLine($"{ForeColorConfig.GreenForeColor}You selected {MenuItems.ActiveMenuItems.ElementAt(selectedOption)}{ForeColorConfig.ForeColorReset}");

static void ConfigureConsole()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("Welcome to Budget Tracker, take control of your finances!");
    Console.ResetColor();
    Console.WriteLine($"\nUse ⬆ and ⬇ to navigate and key {ForeColorConfig.GreenForeColor}Enter/Return{ForeColorConfig.ForeColorReset} to select.");
    Console.CursorVisible = false;
}