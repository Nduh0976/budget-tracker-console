using System.Globalization;
using System.Text;
using BudgetTracker.Models;
using BudgetTracker.Services.Constants;
using BudgetTracker.Services.Interfaces;

namespace BudgetTracker.Services
{
    public class MenuService
    {
        private readonly IUserService _userService;
        private readonly IBudgetService _budgetService;
        private readonly ICategoryService _categoryService;
        private readonly IExpenseService _expenseService;
        private readonly MenuDisplayService _menuDisplayService;

        public MenuService(
            IUserService userService,
            IBudgetService budgetService,
            ICategoryService categoryService,
            IExpenseService expenseService,
            MenuDisplayService menuDisplayService)
        {
            _userService = userService;
            _budgetService = budgetService;
            _categoryService = categoryService;
            _expenseService = expenseService;
            _menuDisplayService = menuDisplayService;
        }

        public void Run()
        {
            ConfigureConsole();
            ConfigureActiveUser();
            DisplayMenu();
        }

        private void ConfigureActiveUser()
        {
            var selectedMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.UserMenuItems);
            ConfigureUser(selectedMenuOption);
        }

        private void ConfigureUser(string selectedMenuOption)
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

        private void CreateUser()
        {
            var username = _menuDisplayService.GetUserInput(MenuMessages.UserNamePrompt);
            var name = _menuDisplayService.GetUserInput(MenuMessages.NamePrompt);
            var response = _userService.CreateUser(username, name);

            if (response.Success)
            {
                _userService.SetActiveUser(response.Data ?? new User() { Name = string.Empty, Username = string.Empty });
            }

            Console.WriteLine(response.Message);

            if (_userService.ActiveUserExists())
            {
                ConfigureConsole();
            }
        }

        private void ViewUserSelection()
        {
            // Print table header
            Console.WriteLine("    ID    | Username        | Name");
            Console.WriteLine(new string('-', 40));

            var usersTable = _userService.GetUsersAsTable();

            var selectedUserId = int.Parse(_menuDisplayService.DisplayMenuAndGetSelection(usersTable)[0].ToString());
            _userService.SetActiveUserById(selectedUserId);

            if (_userService.ActiveUserExists())
            {
                ConfigureConsole();
            }
        }

        private void DisplayMenu()
        {
            var appRunning = true;
            var selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);

            while (appRunning)
            {

                switch (selectedActiveMenuOption)
                {
                    case MenuItems.ExitApplication:
                        appRunning = false;
                        Console.WriteLine(MenuMessages.GoodBye);
                        break;

                    case MenuItems.SwitchUser:
                        ViewUserSelection();
                        selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                        break;

                    case MenuItems.EditUser:
                        UpdateUser();
                        selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                        break;

                    case MenuItems.DeleteUser:
                        DeleteUser();

                        var selectedMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.UserMenuItems);
                        ConfigureUser(selectedMenuOption);

                        selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                        break;

                    case MenuItems.Budgets:
                        var selectedBudgetMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.BudgetsMenuItems);

                        if (selectedBudgetMenuOption == MenuItems.ViewBudgets)
                        {
                            ViewBudgetSelection();
                            selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                        }
                        else if (selectedBudgetMenuOption == MenuItems.CreateBudget)
                        {
                            CreateBudget();
                        }

                        break;

                    case MenuItems.ManageCategories:
                        var selectedCategoryMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.CategoriesMenuItems);

                        if (selectedCategoryMenuOption == MenuItems.ViewCategories)
                        {
                            ViewCategorySelection();
                            selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
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
        }

        private void ConfigureConsole()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(_menuDisplayService.GetWelcomeMessage());
            Console.ResetColor();
            Console.WriteLine(MenuMessages.MenuNavigationHint);
            Console.CursorVisible = false;
        }

        private void DeleteUser()
        {
            var selectedUserId = _userService.GetActiveUserId();

            if (selectedUserId <= 0)
            {
                Console.WriteLine(MenuMessages.NoUserSelected);
                return;
            }

            Console.Write(MenuMessages.DeleteCurrentUser);
            var confirmation = Console.ReadLine()?.Trim().ToUpper();

            if (confirmation == MenuMessages.Yes)
            {
                _budgetService.DeleteBudgetByUserId(selectedUserId);
                var result = _userService.RemoveUser();

                if (result)
                {
                    Console.WriteLine(MenuMessages.UserRemoved);
                }
                else
                {
                    Console.WriteLine(MenuMessages.ProblemDeletingUser);
                }
            }
            else
            {
                Console.WriteLine(MenuMessages.DeletionCancelled);
            }
        }

        private void UpdateUser()
        {
            var name = _menuDisplayService.GetUserInput(MenuMessages.NewNamePrompt);

            var response = _userService.UpdateUser(name ?? string.Empty);
            if (response.Success)
            {
                _userService.SetActiveUser(response.Data ?? new User() { Name = string.Empty, Username = string.Empty });
                ConfigureConsole();
            }

            Console.WriteLine(response.Message);
        }

        private void ViewBudgetSelection()
        {
            // Print table header
            Console.WriteLine(new string('=', 85));
            Console.WriteLine($"    {"ID",-5} | {"Name",-30} | {"Start Date",-12} | {"End Date",-12} | {"Amount",-10}");
            Console.WriteLine(new string('-', 85));

            var budgetsTable = _budgetService.GetBudgetsAsTableByUserId(_userService.GetActiveUserId());

            var selectedBudgetId = int.Parse(_menuDisplayService.DisplayMenuAndGetSelection(budgetsTable)[0].ToString());
            _budgetService.SetSelectedBudgetById(selectedBudgetId);

            if (_budgetService.SelectedBudgetExists())
            {
                var selectedBudgetMenu = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.SelectedBudgetMenuItems);

                switch (selectedBudgetMenu)
                {
                    case MenuItems.AddExpense:
                        AddExpense();
                        break;

                    case MenuItems.ViewExpenses:
                        var expenses = _expenseService.GetExpensesByBudgetId(_budgetService.GetSelectedBudgetId());

                        // Apply sorting and filtering
                        var sortedExpenses = GetSortedExpenses(expenses);
                        var filteredExpenses = GetFilteredExpenses(sortedExpenses);

                        _menuDisplayService.ViewBudgetTotals();
                        ViewExpenseSelection(filteredExpenses);
                        break;

                    case MenuItems.ViewBudgetSummary:
                        _menuDisplayService.ViewBudgetSummary();
                        break;
                }
            }
        }

        private void AddExpense()
        {
            var description = _menuDisplayService.GetUserInput(MenuMessages.DescriptionPrompt);
            var date = DateTime.ParseExact(_menuDisplayService.GetUserInput(MenuMessages.DatePrompt), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var categoryId = GetExpenseCategory();
            var amount = decimal.Parse(_menuDisplayService.GetUserInput(MenuMessages.AmountPrompt));

            var response = _expenseService.AddExpense(_budgetService.GetSelectedBudgetId(), categoryId, description, date, amount);

            Console.WriteLine(response.Message);
        }

        private int GetExpenseCategory()
        {
            Console.WriteLine(MenuMessages.CategoryPrompt);
            var categoriesTable = _categoryService.GetCategoriesAsTable();
            Console.WriteLine($"    {"ID",-5} | {"Name",-30}");

            return int.Parse(_menuDisplayService.DisplayMenuAndGetSelection(categoriesTable)[0].ToString());
        }

        private IEnumerable<Expense> GetSortedExpenses(IEnumerable<Expense> expenses)
        {
            var sortOption = _menuDisplayService.GetSortOption();

            switch (sortOption)
            {
                case SortItems.Date:
                    expenses = expenses.OrderBy(e => e.Date);
                    break;
                case SortItems.Amount:
                    expenses = expenses.OrderByDescending(e => e.Amount);
                    break;
                case SortItems.Category:
                    expenses = expenses.OrderBy(e => _categoryService.GetCategoryById(e.CategoryId)?.Name ?? "Unknown");
                    break;
                default:
                    break;
            }

            return expenses;
        }

        private IEnumerable<Expense> GetFilteredExpenses(IEnumerable<Expense> expenses)
        {
            var filterOption = _menuDisplayService.GetFilterOption();

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

        private IEnumerable<Expense> FilterByDateRange(IEnumerable<Expense> expenses)
        {
            var startDate = DateTime.ParseExact(_menuDisplayService.GetUserInput(MenuMessages.StartDatePrompt), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(_menuDisplayService.GetUserInput(MenuMessages.EndDatePrompt), "dd-MM-yyyy", CultureInfo.InvariantCulture);

            return expenses.Where(e => e.Date >= startDate && e.Date <= endDate);
        }

        private IEnumerable<Expense> FilterByCategory(IEnumerable<Expense> expenses)
        {
            Console.WriteLine(MenuMessages.CategoryPrompt);
            var categoriesTable = _categoryService.GetCategoriesAsTable();
            Console.WriteLine($"    {"ID",-5} | {"Name",-30}");
            var selectedCategoryId = int.Parse(_menuDisplayService.DisplayMenuAndGetSelection(categoriesTable)[0].ToString());

            return expenses.Where(e => e.CategoryId == selectedCategoryId);
        }

        private void CreateBudget()
        {
            var name = _menuDisplayService.GetUserInput(MenuMessages.NamePrompt);
            var startDate = DateTime.ParseExact(_menuDisplayService.GetUserInput(MenuMessages.StartDatePrompt), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var endDate = DateTime.ParseExact(_menuDisplayService.GetUserInput(MenuMessages.EndDatePrompt), "dd-MM-yyyy", CultureInfo.InvariantCulture);
            var amount = decimal.Parse(_menuDisplayService.GetUserInput(MenuMessages.AmountPrompt));

            var response = _budgetService.CreateBudget(_userService.GetActiveUserId(), name, startDate, endDate, amount);

            Console.WriteLine(response.Message);
        }

        private void ViewCategorySelection()
        {
            // Print table header
            Console.WriteLine(new string('=', 85));
            Console.WriteLine($"    {"ID",-5} | {"Name",-30}");
            Console.WriteLine(new string('-', 85));

            var categoriesTable = _categoryService.GetCategoriesAsTable();

            var selectedCategoryId = int.Parse(_menuDisplayService.DisplayMenuAndGetSelection(categoriesTable)[0].ToString());

            _categoryService.SetSelectedCategoryById(selectedCategoryId);

            if (_categoryService.HasSelectedCategory())
            {
                var selectedBudgetMenu = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.SelectedCategoryMenuItems);

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

        private void UpdateCategory()
        {
            var name = _menuDisplayService.GetUserInput(MenuMessages.NewNamePrompt);

            var response = _categoryService.UpdateCategory(name ?? string.Empty);

            Console.WriteLine(response.Message);
        }

        private void DeleteCategory()
        {
            Console.Write(MenuMessages.DeleteCategory);
            var confirmation = Console.ReadLine()?.Trim().ToUpper();

            if (confirmation == MenuMessages.Yes)
            {
                var result = _categoryService.DeleteCategory();

                if (result)
                {
                    Console.WriteLine(MenuMessages.CategoryRemoved);
                }
                else
                {
                    Console.WriteLine(MenuMessages.ProblemDeletingCategory);
                }
            }
            else
            {
                Console.WriteLine(MenuMessages.DeletionCancelled);
            }
        }

        private void CreateCategory()
        {
            var name = _menuDisplayService.GetUserInput(MenuMessages.NamePrompt);

            var response = _categoryService.CreateCategory(name);

            Console.WriteLine(response.Message);
        }

        private void ViewExpenseSelection(IEnumerable<Expense> expenses)
        {
            // Print table header
            Console.WriteLine(new string('=', 90));
            Console.WriteLine($"    {"ID",-5} | {"Description",-30} | {"Category",-20} | {"Date",-12} | {"Amount",-10}");
            Console.WriteLine(new string('-', 90));

            var expensesTable = _menuDisplayService.GetExpensesAsTable(expenses);
            var selectedExpenseId = int.Parse(_menuDisplayService.DisplayMenuAndGetSelection(expensesTable)[0].ToString());

            var selectedExpenseMenu = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.SelectedExpenseMenuItems);

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

        private void UpdateExpense(int expenseId)
        {
            Console.WriteLine(MenuMessages.SkipUpdate);
            var description = _menuDisplayService.GetUserInput(MenuMessages.DescriptionPrompt);
            var date = _menuDisplayService.GetUserInput(MenuMessages.DatePrompt);

            var categoryId = GetExpenseCategory();
            var amount = _menuDisplayService.GetUserInput(MenuMessages.AmountPrompt);

            var response = _expenseService.UpdateExpense(expenseId, categoryId, description, date, amount);

            Console.WriteLine(response.Message);
        }

        private void DeleteExpense(int expenseId)
        {
            Console.Write(MenuMessages.DeleteExpense);
            var confirmation = Console.ReadLine()?.Trim().ToUpper();

            if (confirmation == MenuMessages.Yes)
            {
                var result = _expenseService.DeleteExpense(expenseId);

                if (result)
                {
                    Console.WriteLine(MenuMessages.ExpenseRemoved);
                }
                else
                {
                    Console.WriteLine(MenuMessages.ProblemDeletingExpense);
                }
            }
            else
            {
                Console.WriteLine(MenuMessages.DeletionCancelled);
            }
        }
    }
}