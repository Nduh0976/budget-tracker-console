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

        case MenuItems.DeleteUser:
            DeleteUser();

            selectedMenuOption = DisplayMenuAndGetSelection(MenuItems.UserMenuItems);
            ConfigureUser(selectedMenuOption);

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
                var expenses = expenseService.GetExpensesByBudgetId(budgetService.SelectedBudget.Id);

                // Apply sorting and filtering
                var sortedExpenses = GetSortedExpenses(expenses);
                var filteredExpenses = GetFilteredExpenses(sortedExpenses);

                ViewBudgetTotals();
                ViewExpenseSelection(filteredExpenses);
                break;

            case MenuItems.ViewBudgetSummary:
                ViewBudgetSummary();
                break;
        }
    }
}

void ViewBudgetTotals()
{
    var amountUsed = budgetService.GetSelectedBudgetTotalSpent();
    var remainingBalance = budgetService.GetSelectedBudgetRemainingBalance();

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(new string('=', 90));
    Console.WriteLine($" Budget Details - {budgetService.SelectedBudget.Name} ");
    Console.WriteLine(new string('=', 90));
    Console.ResetColor();

    Console.WriteLine($"Total Amount: {budgetService.SelectedBudget.Amount}");
    Console.WriteLine($"Amount Used:  {amountUsed}");
    Console.WriteLine($"Remaining Balance: {remainingBalance}\n");
}

void ViewExpenseSelection(IEnumerable<Expense> expenses)
{
    // Print table header
    Console.WriteLine(new string('=', 90));
    Console.WriteLine($"    {"ID",-5} | {"Description",-30} | {"Category",-20} | {"Date",-12} | {"Amount",-10}");
    Console.WriteLine(new string('-', 90));

    var expensesTable = GetExpensesAsTable(expenses);
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

IEnumerable<Expense> GetSortedExpenses(IEnumerable<Expense> expenses)
{
    var sortOption = GetSortOption();
    switch (sortOption)
    {
        case SortItems.Date:
            expenses = expenses.OrderBy(e => e.Date);
            break;
        case SortItems.Amount:
            expenses = expenses.OrderByDescending(e => e.Amount);
            break;
        case SortItems.Category:
            expenses = expenses.OrderBy(e => categoryService.GetCategoryById(e.CategoryId)?.Name ?? "Unknown");
            break;
        default:
            break;
    }

    return expenses;
}

string GetSortOption()
{
    Console.WriteLine("\nSelect sorting option:");
    Console.WriteLine("1. Sort by Date");
    Console.WriteLine("2. Sort by Amount");
    Console.WriteLine("3. Sort by Category");
    Console.WriteLine("4. No sorting");

    var input = GetUserInput("Enter your choice (1-4): ");

    return input switch
    {
        "1" => SortItems.Date,
        "2" => SortItems.Amount,
        "3" => SortItems.Category,
        _ => SortItems.NoSorting,
    };
}

IEnumerable<Expense> GetFilteredExpenses(IEnumerable<Expense> expenses)
{
    var filterOption = GetFilterOption();

    switch (filterOption)
    {
        case FilterItems.DateRange:
            return FilterByDateRange(expenses);
        case FilterItems.Category:
            return FilterByCategory(expenses);
        default:
            return expenses;
    }
}

string GetFilterOption()
{
    Console.WriteLine("\nDo you want to filter expenses? (Y/N): ");
    var filterChoice = GetUserInput("").Trim().ToUpper();

    if (filterChoice != "Y") return FilterItems.NoFilter;

    Console.WriteLine("Select filter criteria:");
    Console.WriteLine("1. Filter by Date Range");
    Console.WriteLine("2. Filter by Category");
    Console.WriteLine("3. No filtering");

    var filterOption = GetUserInput("Enter your choice (1-3): ");

    return filterOption switch
    {
        "1" => FilterItems.DateRange,
        "2" => FilterItems.Category,
        _ => FilterItems.NoFilter,
    };
}

IEnumerable<Expense> FilterByDateRange(IEnumerable<Expense> expenses)
{
    var startDate = DateTime.ParseExact(GetUserInput("Enter start date (dd-mm-yyyy): "), "dd-MM-yyyy", CultureInfo.InvariantCulture);
    var endDate = DateTime.ParseExact(GetUserInput("Enter end date (dd-mm-yyyy): "), "dd-MM-yyyy", CultureInfo.InvariantCulture);

    return expenses.Where(e => e.Date >= startDate && e.Date <= endDate);
}

IEnumerable<Expense> FilterByCategory(IEnumerable<Expense> expenses)
{
    Console.WriteLine("Select Category:");
    var categoriesTable = categoryService.GetCategoriesAsTable();
    Console.WriteLine($"    {"ID",-5} | {"Name",-30}");
    var selectedCategoryId = int.Parse(DisplayMenuAndGetSelection(categoriesTable)[0].ToString());

    return expenses.Where(e => e.CategoryId == selectedCategoryId);
}

IList<string> GetExpensesAsTable(IEnumerable<Expense> expenses)
{
    var expenseDescriptions = new List<string>();

    foreach (var expense in expenses)
    {
        expenseDescriptions.Add(expense.ToString());
    }

    return expenseDescriptions;
}

void ViewBudgetSummary()
{
    var budgetName = budgetService.SelectedBudget.Name;
    var startDate = budgetService.SelectedBudget.StartDate.ToString("dd-MM-yyyy");
    var endDate = budgetService.SelectedBudget.EndDate.ToString("dd-MM-yyyy");
    var totalAmount = budgetService.SelectedBudget.Amount;
    var amountUsed = budgetService.GetSelectedBudgetTotalSpent();
    var remainingBalance = budgetService.GetSelectedBudgetRemainingBalance();

    var percentageUsed = (totalAmount > 0) ? Math.Round((amountUsed / totalAmount) * 100, 2) : 0;
    var percentageRemaining = 100 - percentageUsed;

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine(new string('=', 85));
    Console.WriteLine($" Budget Summary - {budgetName} ");
    Console.WriteLine(new string('=', 85));
    Console.ResetColor();

    // Print detailed budget information
    Console.WriteLine($"\nStart Date: {startDate}");
    Console.WriteLine($"End Date:   {endDate}");
    Console.WriteLine($"Total Amount: {totalAmount:C}");
    Console.WriteLine($"Amount Used:  {amountUsed:C} ({percentageUsed}% of total)");
    Console.WriteLine($"Remaining Balance: {remainingBalance:C} ({percentageRemaining}% of total)\n");

    Console.WriteLine(new string('-', 85));

    DisplayProgressBar(percentageUsed);
    DisplayTotalExpensesByCategories();
}

void DisplayTotalExpensesByCategories()
{
    Console.WriteLine("\nExpenses Breakdown by Category:");
    Console.WriteLine(new string('-', 85));
    Console.WriteLine($"{"Category",-30} | {"Total Expense",-15}");
    Console.WriteLine(new string('-', 85));

    // Fetch and group expenses by category
    var expensesByCategory = expenseService
        .GetExpensesByBudgetId(budgetService.SelectedBudget.Id)
        .GroupBy(e => e.CategoryId)
        .Select(group => new
        {
            CategoryId = group.Key,
            TotalAmount = group.Sum(e => e.Amount),
            CategoryName = categoryService.GetCategoryById(group.Key)?.Name ?? "Unknown"
        })
        .OrderByDescending(g => g.TotalAmount);

    foreach (var category in expensesByCategory)
    {
        Console.WriteLine($"{category.CategoryName,-30} | {category.TotalAmount:C}");
    }

    Console.WriteLine(new string('-', 85));

    Console.WriteLine("\nPress any key to return to the menu...");
    Console.ReadKey();
}

void DisplayProgressBar(decimal percentage)
{
    const int progressBarWidth = 50;
    int filledWidth = (int)(percentage / 100 * progressBarWidth);

    Console.Write("Progress: [");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write(new string('#', filledWidth)); // Filled portion
    Console.ResetColor();
    Console.Write(new string(' ', progressBarWidth - filledWidth)); // Empty portion
    Console.WriteLine($"] {percentage}%");
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

void DeleteUser()
{
    var selectedUser = userService.ActiveUser;

    if (!selectedUser.Exists())
    {
        Console.WriteLine("No user selected.");
        return;
    }

    Console.Write("Are you sure you want to delete the current user? (Y/N): ");
    var confirmation = Console.ReadLine()?.Trim().ToUpper();

    if (confirmation == "Y")
    {
        budgetService.DeleteBudgetByUserId(selectedUser.Id);
        var result = userService.RemoveUser();

        if (result)
        {
            Console.WriteLine("User removed successfully.");
        }
        else
        {
            Console.WriteLine("There was a problem deleting the user.");
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