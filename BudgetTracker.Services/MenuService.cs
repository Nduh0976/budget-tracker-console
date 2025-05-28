using System.Globalization;
using BudgetTracker.Models;
using BudgetTracker.Services.Constants;
using BudgetTracker.Services.Extensions;
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
            _menuDisplayService.SetupFreshDisplay();
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
                _menuDisplayService.RefreshWelcomeMessage();
            }

            _menuDisplayService.DisplayMessageAndWait(response.Message);

            if (_userService.ActiveUserExists())
            {
                _menuDisplayService.SetupFreshDisplay();
            }
        }

        private void ViewUserSelection()
        {
            // Refresh display before showing user table
            _menuDisplayService.SetupFreshDisplay();

            Console.WriteLine("    ID    | Username        | Name");
            Console.WriteLine(new string('-', 40));

            var usersTable = _userService.GetUsersAsTable();

            var selectedUserId = _menuDisplayService.DisplayMenuAndGetSelection(usersTable).GetFirstNumber().GetValueOrDefault();
            _userService.SetActiveUserById(selectedUserId);

            if (_userService.ActiveUserExists())
            {
                _menuDisplayService.RefreshWelcomeMessage();
                _menuDisplayService.SetupFreshDisplay();
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
                        HandleBudgetMenuSelection(selectedBudgetMenuOption);
                        selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                        break;

                    case MenuItems.ManageCategories:
                        var selectedCategoryMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.CategoriesMenuItems);
                        HandleCategoryMenuSelection(selectedCategoryMenuOption);
                        selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                        break;

                    default:
                        appRunning = false;
                        break;
                }
            }
        }

        private void HandleBudgetMenuSelection(string selectedBudgetMenuOption)
        {
            switch (selectedBudgetMenuOption)
            {
                case MenuItems.ViewBudgets:
                    ViewBudgetSelection();
                    break;
                case MenuItems.CreateBudget:
                    CreateBudget();
                    break;
            }
        }

        private void HandleCategoryMenuSelection(string selectedCategoryMenuOption)
        {
            switch (selectedCategoryMenuOption)
            {
                case MenuItems.ViewCategories:
                    ViewCategorySelection();
                    break;
                case MenuItems.CreateCategory:
                    CreateCategory();
                    break;
            }
        }

        private void DeleteUser()
        {
            var selectedUserId = _userService.GetActiveUserId();

            if (selectedUserId <= 0)
            {
                _menuDisplayService.DisplayMessageAndWait(MenuMessages.NoUserSelected);
                return;
            }

            Console.Write(MenuMessages.DeleteCurrentUser);
            var confirmation = Console.ReadLine()?.Trim().ToUpper();

            string resultMessage;
            if (confirmation == MenuMessages.Yes)
            {
                _budgetService.DeleteBudgetByUserId(selectedUserId);
                var result = _userService.RemoveUser();
                resultMessage = result
                    ? MenuMessages.UserRemoved
                    : MenuMessages.ProblemDeletingUser;
            }
            else
            {
                resultMessage = MenuMessages.DeletionCancelled;
            }

            _menuDisplayService.DisplayMessageAndWait(resultMessage);
        }

        private void UpdateUser()
        {
            var name = _menuDisplayService.GetUserInput(MenuMessages.NewNamePrompt);

            var response = _userService.UpdateUser(name ?? string.Empty);
            if (response.Success)
            {
                _userService.SetActiveUser(response.Data ?? new User() { Name = string.Empty, Username = string.Empty });
                _menuDisplayService.RefreshWelcomeMessage();
            }

            _menuDisplayService.DisplayMessageAndWait(response.Message);

            if (response.Success)
            {
                _menuDisplayService.SetupFreshDisplay();
            }
        }

        private void ViewBudgetSelection()
        {
            _menuDisplayService.SetupFreshDisplay();

            Console.WriteLine(new string('=', 85));
            Console.WriteLine($"    {"ID",-5} | {"Name",-30} | {"Start Date",-12} | {"End Date",-12} | {"Amount",-10}");
            Console.WriteLine(new string('-', 85));

            var budgetsTable = _budgetService.GetBudgetsAsTableByUserId(_userService.GetActiveUserId());

            var selectedBudgetId = _menuDisplayService.DisplayMenuAndGetSelection(budgetsTable).GetFirstNumber().GetValueOrDefault();
            _budgetService.SetSelectedBudgetById(selectedBudgetId);

            if (_budgetService.SelectedBudgetExists())
            {
                var selectedBudgetMenu = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.SelectedBudgetMenuItems);
                HandleSelectedBudgetAction(selectedBudgetMenu);
            }
        }

        private void HandleSelectedBudgetAction(string selectedBudgetMenu)
        {
            switch (selectedBudgetMenu)
            {
                case MenuItems.AddExpense:
                    AddExpense();
                    break;

                case MenuItems.ViewExpenses:
                    ViewExpensesWithSortingAndFiltering();
                    break;

                case MenuItems.ViewBudgetSummary:
                    _menuDisplayService.ViewBudgetSummary();
                    break;

                case MenuItems.SetMonthlyBudget:
                    UpdateBudgetAmount();
                    break;
            }
        }

        private void ViewExpensesWithSortingAndFiltering()
        {
            var expenses = _expenseService.GetExpensesByBudgetId(_budgetService.GetSelectedBudgetId());

            var sortedExpenses = GetSortedExpenses(expenses);
            var filteredExpenses = GetFilteredExpenses(sortedExpenses);

            _menuDisplayService.SetupFreshDisplay();
            _menuDisplayService.ViewBudgetTotals();
            ViewExpenseSelection(filteredExpenses);
        }

        private void AddExpense()
        {
            var description = _menuDisplayService.GetUserInput(MenuMessages.DescriptionPrompt);
            var dateInput = _menuDisplayService.GetUserInput(MenuMessages.DatePrompt);

            if (!DateTime.TryParseExact(dateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                _menuDisplayService.DisplayMessageAndWait("Invalid date format. Please use dd-MM-yyyy format.");
                return;
            }

            var categoryId = GetExpenseCategory();
            var amountInput = _menuDisplayService.GetUserInput(MenuMessages.AmountPrompt);

            if (!decimal.TryParse(amountInput, out var amount))
            {
                _menuDisplayService.DisplayMessageAndWait("Invalid amount format. Please enter a valid number.");
                return;
            }

            var response = _expenseService.AddExpense(_budgetService.GetSelectedBudgetId(), categoryId, description, date, amount);
            _menuDisplayService.DisplayMessageAndWait(response.Message);
        }

        private void UpdateBudgetAmount()
        {
            Console.WriteLine(MenuMessages.SkipUpdate);

            var amount = _menuDisplayService.GetUserInput(MenuMessages.AmountPrompt);
            var response = _expenseService.UpdateBudgetAmount(_budgetService.GetSelectedBudgetId(), amount);

            _menuDisplayService.DisplayMessageAndWait(response.Message);
        }

        private int GetExpenseCategory()
        {
            Console.WriteLine(MenuMessages.CategoryPrompt);
            var categoriesTable = _categoryService.GetCategoriesAsTable();
            Console.WriteLine($"    {"ID",-5} | {"Name",-30}");

            return _menuDisplayService.DisplayMenuAndGetSelection(categoriesTable).GetFirstNumber().GetValueOrDefault();
        }

        private IEnumerable<Expense> GetSortedExpenses(IEnumerable<Expense> expenses)
        {
            var sortOption = _menuDisplayService.GetSortOption();

            return sortOption switch
            {
                SortItems.Date => expenses.OrderBy(e => e.Date),
                SortItems.Amount => expenses.OrderByDescending(e => e.Amount),
                SortItems.Category => expenses.OrderBy(e => _categoryService.GetCategoryById(e.CategoryId)?.Name ?? "Unknown"),
                _ => expenses
            };
        }

        private IEnumerable<Expense> GetFilteredExpenses(IEnumerable<Expense> expenses)
        {
            var filterOption = _menuDisplayService.GetFilterOption();

            return filterOption switch
            {
                FilterItems.DateRange => FilterByDateRange(expenses),
                FilterItems.Category => FilterByCategory(expenses),
                _ => expenses
            };
        }

        private IEnumerable<Expense> FilterByDateRange(IEnumerable<Expense> expenses)
        {
            var startDateInput = _menuDisplayService.GetUserInput(MenuMessages.StartDatePrompt);
            var endDateInput = _menuDisplayService.GetUserInput(MenuMessages.EndDatePrompt);

            if (!DateTime.TryParseExact(startDateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) ||
                !DateTime.TryParseExact(endDateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            {
                _menuDisplayService.DisplayMessageAndWait("Invalid date format. Returning all expenses.");
                return expenses;
            }

            return expenses.Where(e => e.Date >= startDate && e.Date <= endDate);
        }

        private IEnumerable<Expense> FilterByCategory(IEnumerable<Expense> expenses)
        {
            Console.WriteLine(MenuMessages.CategoryPrompt);
            var categoriesTable = _categoryService.GetCategoriesAsTable();
            Console.WriteLine($"    {"ID",-5} | {"Name",-30}");
            var selectedCategoryId = _menuDisplayService.DisplayMenuAndGetSelection(categoriesTable).GetFirstNumber().GetValueOrDefault();

            return expenses.Where(e => e.CategoryId == selectedCategoryId);
        }

        private void CreateBudget()
        {
            var name = _menuDisplayService.GetUserInput(MenuMessages.NamePrompt);
            var startDateInput = _menuDisplayService.GetUserInput(MenuMessages.StartDatePrompt);
            var endDateInput = _menuDisplayService.GetUserInput(MenuMessages.EndDatePrompt);
            var amountInput = _menuDisplayService.GetUserInput(MenuMessages.AmountPrompt);

            if (!DateTime.TryParseExact(startDateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
            {
                _menuDisplayService.DisplayMessageAndWait("Invalid start date format. Please use dd-MM-yyyy format.");
                return;
            }

            if (!DateTime.TryParseExact(endDateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
            {
                _menuDisplayService.DisplayMessageAndWait("Invalid end date format. Please use dd-MM-yyyy format.");
                return;
            }

            if (!decimal.TryParse(amountInput, out var amount))
            {
                _menuDisplayService.DisplayMessageAndWait("Invalid amount format. Please enter a valid number.");
                return;
            }

            var response = _budgetService.CreateBudget(_userService.GetActiveUserId(), name, startDate, endDate, amount);
            _menuDisplayService.DisplayMessageAndWait(response.Message);
        }

        private void ViewCategorySelection()
        {
            _menuDisplayService.SetupFreshDisplay();

            Console.WriteLine(new string('=', 85));
            Console.WriteLine($"    {"ID",-5} | {"Name",-30}");
            Console.WriteLine(new string('-', 85));

            var categoriesTable = _categoryService.GetCategoriesAsTable();

            var selectedCategoryId = _menuDisplayService.DisplayMenuAndGetSelection(categoriesTable).GetFirstNumber().GetValueOrDefault();
            _categoryService.SetSelectedCategoryById(selectedCategoryId);

            if (_categoryService.HasSelectedCategory())
            {
                var selectedCategoryMenu = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.SelectedCategoryMenuItems);
                HandleSelectedCategoryAction(selectedCategoryMenu);
            }
        }

        private void HandleSelectedCategoryAction(string selectedCategoryMenu)
        {
            switch (selectedCategoryMenu)
            {
                case MenuItems.EditCategory:
                    UpdateCategory();
                    break;
                case MenuItems.DeleteCategory:
                    DeleteCategory();
                    break;
            }
        }

        private void UpdateCategory()
        {
            var name = _menuDisplayService.GetUserInput(MenuMessages.NewNamePrompt);
            var response = _categoryService.UpdateCategory(name ?? string.Empty);
            _menuDisplayService.DisplayMessageAndWait(response.Message);
        }

        private void DeleteCategory()
        {
            Console.Write(MenuMessages.DeleteCategory);
            var confirmation = Console.ReadLine()?.Trim().ToUpper();

            string resultMessage;
            if (confirmation == MenuMessages.Yes)
            {
                resultMessage = _categoryService.DeleteCategory()
                    ? MenuMessages.CategoryRemoved
                    : MenuMessages.ProblemDeletingCategory;
            }
            else
            {
                resultMessage = MenuMessages.DeletionCancelled;
            }

            _menuDisplayService.DisplayMessageAndWait(resultMessage);
        }

        private void CreateCategory()
        {
            var name = _menuDisplayService.GetUserInput(MenuMessages.NamePrompt);
            var response = _categoryService.CreateCategory(name);
            _menuDisplayService.DisplayMessageAndWait(response.Message);
        }

        private void ViewExpenseSelection(IEnumerable<Expense> expenses)
        {
            Console.WriteLine(new string('=', 90));
            Console.WriteLine($"    {"ID",-5} | {"Description",-30} | {"Category",-20} | {"Date",-12} | {"Amount",-10}");
            Console.WriteLine(new string('-', 90));

            var expensesTable = _menuDisplayService.GetExpensesAsTable(expenses);
            var selectedExpenseId = _menuDisplayService.DisplayMenuAndGetSelection(expensesTable).GetFirstNumber().GetValueOrDefault();

            var selectedExpenseMenu = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.SelectedExpenseMenuItems);
            HandleSelectedExpenseAction(selectedExpenseMenu, selectedExpenseId);
        }

        private void HandleSelectedExpenseAction(string selectedExpenseMenu, int expenseId)
        {
            switch (selectedExpenseMenu)
            {
                case MenuItems.EditExpense:
                    UpdateExpense(expenseId);
                    break;
                case MenuItems.DeleteExpense:
                    DeleteExpense(expenseId);
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
            _menuDisplayService.DisplayMessageAndWait(response.Message);
        }

        private void DeleteExpense(int expenseId)
        {
            Console.Write(MenuMessages.DeleteExpense);
            var confirmation = Console.ReadLine()?.Trim().ToUpper();

            string resultMessage;
            if (confirmation == MenuMessages.Yes)
            {
                resultMessage =_expenseService.DeleteExpense(expenseId)
                    ? MenuMessages.ExpenseRemoved
                    : MenuMessages.ProblemDeletingExpense;
            }
            else
            {
                resultMessage = MenuMessages.DeletionCancelled;
            }

            _menuDisplayService.DisplayMessageAndWait(resultMessage);
        }
    }
}