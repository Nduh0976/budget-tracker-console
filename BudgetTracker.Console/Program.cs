using System.Globalization;
using BudgetTracker.Console;
using BudgetTracker.Console.Constants;
using BudgetTracker.Models;
using BudgetTracker.Services;

var appRunning = true;
var userService = new UserService();
var budgetService = new BudgetService();
var expenseService = new ExpenseService();
var categoryService = new CategoryService();

ConfigureConsole();

var selectedMenuOption = DisplayMenuAndGetSelection(MenuItems.UserMenuItems);

ConfigureUser(selectedMenuOption);

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
            ViewUserSelection();
            selectedActiveMenuOption = DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
            break;

        case MenuItems.EditUser:
            UpdateUser();
            selectedActiveMenuOption = DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
            break;

        case MenuItems.Budgets:
            var selectedBudgetMenuOption = DisplayMenuAndGetSelection(MenuItems.BudgetsMenuItems);

            if (selectedBudgetMenuOption == MenuItems.ViewBudgets)
            {
                ViewBudgetSelection();
                selectedActiveMenuOption = DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
            }
            else if (selectedBudgetMenuOption == MenuItems.CreateBudget)
            {
                CreateBudget();
            }

            break;

        case MenuItems.ManageCategories:
            var selectedCategoryMenuOption = DisplayMenuAndGetSelection(MenuItems.CategoriesMenuItems);

            if (selectedCategoryMenuOption == MenuItems.ViewCategories)
            {
                ViewCategorySelection();
                selectedActiveMenuOption = DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
            }
            else if (selectedCategoryMenuOption == MenuItems.CreateCategory)
            {
                CreateCategory();
            }

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

    ConfigureConsole();

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

void ConfigureUser(string selectedMenuOption)
{
    if (selectedMenuOption == MenuItems.AddUser)
    {
        CreateUser();
    }
    else if (selectedMenuOption == MenuItems.SelectUser)
    {
        ViewUserSelection();
    }
}

void CreateUser()
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
        ConfigureConsole();
    }
}

void ViewUserSelection()
{
    // Print table header
    Console.WriteLine("    ID    | Username        | Name");
    Console.WriteLine(new string('-', 40));

    var usersTable = userService.GetUsersAsTable();

    var selectedUserId = int.Parse(DisplayMenuAndGetSelection(usersTable)[0].ToString());
    userService.SetActiveUserById(selectedUserId);

    if (userService.ActiveUser.Exists())
    {
        ConfigureConsole();
    }
}

void CreateBudget()
{
    var name = GetUserInput("Enter name:");
    var startDate = DateTime.ParseExact(GetUserInput("Enter start date(dd-mm-yyyy):"), "dd-MM-yyyy", CultureInfo.InvariantCulture);
    var endDate = DateTime.ParseExact(GetUserInput("Enter end date(dd-mm-yyyy):"), "dd-MM-yyyy", CultureInfo.InvariantCulture);
    var amount = decimal.Parse(GetUserInput("Enter amount:"));

    var response = budgetService.CreateBudget(userService.ActiveUser.Id, name, startDate, endDate, amount);

    Console.WriteLine(response.Message);
}

void CreateCategory()
{
    var name = GetUserInput("Enter name:");

    var response = categoryService.CreateCategory(name);

    Console.WriteLine(response.Message);
}

void AddExpense()
{
    var description = GetUserInput("Enter description:");
    var date = DateTime.ParseExact(GetUserInput("Enter date(dd-mm-yyyy):"), "dd-MM-yyyy", CultureInfo.InvariantCulture);
    var categoryId = GetExpenseCategory();
    var amount = decimal.Parse(GetUserInput("Enter amount:"));

    var response = expenseService.AddExpense(budgetService.SelectedBudget.Id, categoryId, description, date, amount);

    Console.WriteLine(response.Message);
}

int GetExpenseCategory()
{
    Console.WriteLine("Select Category:");
    var categoriesTable = categoryService.GetCategoriesAsTable();
    Console.WriteLine($"    {"ID",-5} | {"Name",-30}");

    return int.Parse(DisplayMenuAndGetSelection(categoriesTable)[0].ToString());
}

void ViewCategorySelection()
{
    // Print table header
    Console.WriteLine(new string('=', 85));
    Console.WriteLine($"    {"ID",-5} | {"Name",-30}");
    Console.WriteLine(new string('-', 85));

    var categoriesTable = categoryService.GetCategoriesAsTable();

    var selectedCategoryId = int.Parse(DisplayMenuAndGetSelection(categoriesTable)[0].ToString());

    categoryService.SetSelectedCategoryById(selectedCategoryId);

    if (categoryService.SelectedCategory != null)
    {
        var selectedBudgetMenu = DisplayMenuAndGetSelection(MenuItems.SelectedCategoryMenuItems);

        switch (selectedBudgetMenu)
        {
            case MenuItems.EditCategory:
                UpdateCategory();
                break;

            case MenuItems.DeleteCategory:
                DeleteCategory();
                break;
        }
    }
}

void ViewBudgetSelection()
{
    // Print table header
    Console.WriteLine(new string('=', 85));
    Console.WriteLine($"    {"ID",-5} | {"Name",-30} | {"Start Date",-12} | {"End Date",-12} | {"Amount",-10}");
    Console.WriteLine(new string('-', 85));

    var budgetsTable = budgetService.GetBudgetsAsTableByUserId(userService.ActiveUser.Id);

    var selectedBudgetId = int.Parse(DisplayMenuAndGetSelection(budgetsTable)[0].ToString());
    budgetService.SetSelectedBudgetById(selectedBudgetId);

    if (budgetService.SelectedBudget.Exists())
    {
        var selectedBudgetMenu = DisplayMenuAndGetSelection(MenuItems.SelectedBudgetMenuItems);

        switch (selectedBudgetMenu)
        {
            case MenuItems.AddExpense:
                AddExpense();
                break;

            case MenuItems.ViewExpenses:
                ViewExpenseSelection(selectedBudgetId);
                break;
        }
    }
}

void ViewExpenseSelection(int budgetId)
{
    // Print table header
    Console.WriteLine(new string('=', 90));
    Console.WriteLine($"    {"ID",-5} | {"Description",-30} | {"Category",-20} | {"Date",-12} | {"Amount",-10}");
    Console.WriteLine(new string('-', 90));

    var expensesTable = expenseService.GetExpensesAsTableByBudgetId(budgetId);

    var selectedExpenseId = int.Parse(DisplayMenuAndGetSelection(expensesTable)[0].ToString());

    if (selectedActiveMenuOption != null)
    {
        var selectedExpenseMenu = DisplayMenuAndGetSelection(MenuItems.SelectedExpenseMenuItems);

        switch (selectedExpenseMenu)
        {
            case MenuItems.EditExpense:
                UpdateExpense(selectedExpenseId);
                break;
            case MenuItems.DeleteExpense:
                DeleteExpense(selectedExpenseId);
                break;

        }
    }
}

void DeleteCategory()
{
    Console.Write("Are you sure you want to delete this category? (Y/N): ");
    var confirmation = Console.ReadLine()?.Trim().ToUpper();

    if (confirmation == "Y")
    {
        var result = categoryService.DeleteCategory();

        if (result)
        {
            Console.WriteLine("Category removed successfully.");
        }
        else
        {
            Console.WriteLine("There was a problem deleting the category, there may be dependencies.");
        }
    }
    else
    {
        Console.WriteLine("Deletion canceled.");
    }
}

void DeleteExpense(int expenseId)
{
    Console.Write("Are you sure you want to delete this expense? (Y/N): ");
    var confirmation = Console.ReadLine()?.Trim().ToUpper();

    if (confirmation == "Y")
    {
        var result = expenseService.DeleteExpense(expenseId);

        if (result)
        {
            Console.WriteLine("Expense removed successfully.");
        }
        else
        {
            Console.WriteLine("There was a problem deleting the expense.");
        }
    }
    else
    {
        Console.WriteLine("Deletion canceled.");
    }
}

void UpdateCategory()
{
    var name = GetUserInput("Enter new name:");

    var response = categoryService.UpdateCategory(name ?? string.Empty);

    Console.WriteLine(response.Message);
}

void UpdateUser()
{
    var name = GetUserInput("Enter new name:");

    var response = userService.UpdateUser(name ?? string.Empty);
    if (response.Success)
    {
        userService.ActiveUser = response.Data ?? new User() { Name = string.Empty, Username = string.Empty };
        ConfigureConsole();
    }

    Console.WriteLine(response.Message);
}

void UpdateExpense(int expenseId)
{
    Console.WriteLine($"Leave fields empty to skip update");
    var description = GetUserInput("Enter description:");
    var date = GetUserInput("Enter date(dd-mm-yyyy):");

    var categoryId = GetExpenseCategory();
    var amount = GetUserInput("Enter amount:");

    var response = expenseService.UpdateExpense(expenseId, categoryId, description, date, amount);

    Console.WriteLine(response.Message);
}

void ConfigureConsole()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine(MenuHelper.GetWelcomeMessage(userService?.ActiveUser));
    Console.ResetColor();
    Console.WriteLine($"\nUse ⬆ and ⬇ to navigate and key {ForeColorConfig.GreenForeColor}Enter/Return{ForeColorConfig.ForeColorReset} to select.");
    Console.CursorVisible = false;
}