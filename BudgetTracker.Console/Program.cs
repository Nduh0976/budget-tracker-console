using BudgetTracker.Console;
using BudgetTracker.Console.Constants;
using BudgetTracker.Models;
using BudgetTracker.Services;

ConfigureConsole();

var appRunning = true;
var userService = new UserService();
var selectedMenuOption = DisplayMenuAndGetSelection(MenuItems.UserMenuItems);

ConfigureUser(userService, selectedMenuOption);

var selectedActiveMenuOption = DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
while (appRunning)
{

    switch (selectedActiveMenuOption)
    {
        case MenuItems.ExitApplication:
            appRunning = false;
            Console.WriteLine("Good Bye!");
            break;

        case MenuItems.SwitchUser:
            ViewUserSelection(userService);
            selectedActiveMenuOption = DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
            break;
        
        case MenuItems.EditUser:
            UpdateUser(userService);
            selectedActiveMenuOption = DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
            break;
        default:
            appRunning = false;
            break;
    }
}

string DisplayMenuAndGetSelection(IList<string> userMenuItems)
{
    if (userMenuItems == null || userMenuItems.Count == 0)
    {
        Console.WriteLine("No menu items available.");
        return string.Empty; // Indicates no selection
    }

    var (Left, Top) = Console.GetCursorPosition();
    var selectedOption = 0;
    var isSelected = false;

    while (!isSelected)
    {
        Console.SetCursorPosition(Left, Top);
        Console.WriteLine(MenuHelper.GetMenu(selectedOption, userMenuItems));
        
        switch (Console.ReadKey(false).Key)
        {
            case ConsoleKey.DownArrow:
                selectedOption = (selectedOption + 1) % userMenuItems.Count;
                break;

            case ConsoleKey.UpArrow:
                selectedOption = (selectedOption - 1 + userMenuItems.Count) % userMenuItems.Count;
                break;

            case ConsoleKey.Enter:
                isSelected = true;
                break;
        }
    }

    var selectedUserMenuOption = userMenuItems[selectedOption];
    Console.WriteLine($"{ForeColorConfig.GreenForeColor}You selected {selectedUserMenuOption}{ForeColorConfig.ForeColorReset}");

    return selectedUserMenuOption;
}

string GetUserInput(string message)
{
    Console.CursorVisible = !Console.CursorVisible; Console.WriteLine(message);
    var input = Console.ReadLine();
    Console.CursorVisible = !Console.CursorVisible;

    return input ?? string.Empty;
}

void ConfigureUser(UserService userService, string selectedMenuOption)
{
    if (selectedMenuOption == MenuItems.AddUser)
    {
        CreateUser(userService);
    }
    else if (selectedMenuOption == MenuItems.SelectUser)
    {
        ViewUserSelection(userService);
    }
}

void CreateUser(UserService userService)
{
    var username = GetUserInput("Enter username:");
    var name = GetUserInput("Enter name:");
    var response = userService.CreateUser(username, name);

    if (response.Success)
    {
        userService.ActiveUser = response.Data ?? new User() { Name = string.Empty, Username = string.Empty };
    }

    Console.WriteLine(response.Message);

    if (userService.ActiveUser != null && userService.ActiveUser.Exists())
    {
        ConfigureConsole(userService.ActiveUser);
    }
}

void ViewUserSelection(UserService userService)
{
    // Print table header
    Console.WriteLine("ID    | Username        | Name");
    Console.WriteLine(new string('-', 40));

    var usersTable = userService.GetUsersAsTable();

    var selectedUserId = int.Parse(DisplayMenuAndGetSelection(usersTable)[0].ToString());
    userService.SetActiveUserById(selectedUserId);

    if (userService.ActiveUser.Exists())
    {
        ConfigureConsole(userService.ActiveUser);
    }
}

void UpdateUser(UserService userService)
{
    var name = GetUserInput("Enter new name:");

    var response = userService.UpdateUser(userService.ActiveUser.Id, name ?? string.Empty);
    if (response.Success)
    {
        userService.ActiveUser = response.Data ?? new User() { Name = string.Empty, Username = string.Empty };
        ConfigureConsole(userService.ActiveUser);
    }
    Console.WriteLine(response.Message);
}

void ConfigureConsole(User? activeUser = null)
{
    Console.Clear();
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine(MenuHelper.GetWelcomeMessage(activeUser));
    Console.ResetColor();
    Console.WriteLine($"\nUse ⬆ and ⬇ to navigate and key {ForeColorConfig.GreenForeColor}Enter/Return{ForeColorConfig.ForeColorReset} to select.");
    Console.CursorVisible = false;
}