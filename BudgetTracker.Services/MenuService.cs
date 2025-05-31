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
        private readonly ILoggingService _loggingService;

        public MenuService(
            IUserService userService,
            IBudgetService budgetService,
            ICategoryService categoryService,
            IExpenseService expenseService,
            MenuDisplayService menuDisplayService,
            ILoggingService loggingService)
        {
            _userService = userService;
            _budgetService = budgetService;
            _categoryService = categoryService;
            _expenseService = expenseService;
            _menuDisplayService = menuDisplayService;
            _loggingService = loggingService;
        }

        public void Run()
        {
            try
            {
                _loggingService.LogInfo("Application started");
                _menuDisplayService.SetupFreshDisplay();
                ConfigureActiveUser();
                DisplayMenu();
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Critical error in application startup", ex);
                _menuDisplayService.DisplayMessageAndWait("A critical error occurred. Please check the logs for details.");
            }
        }

        private void ConfigureActiveUser()
        {
            try
            {
                var selectedMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.UserMenuItems, includeExitOption: true);

                if (string.IsNullOrWhiteSpace(selectedMenuOption))
                {
                    _loggingService.LogWarning("Empty menu selection received in ConfigureActiveUser");
                    _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                    ConfigureActiveUser();
                    return;
                }

                if (selectedMenuOption == MenuItems.ExitApplication)
                {
                    _loggingService.LogInfo("User chose to exit application");
                    Console.WriteLine(MenuMessages.GoodBye);
                    Environment.Exit(0);
                }

                ConfigureUser(selectedMenuOption);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in ConfigureActiveUser", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while configuring user. Please try again.");
                ConfigureActiveUser();
            }
        }

        private void ConfigureUser(string selectedMenuOption)
        {
            try
            {
                if (selectedMenuOption == MenuItems.AddUser)
                {
                    CreateUser();
                }
                else if (selectedMenuOption == MenuItems.SelectUser)
                {
                    ViewUserSelection();
                }
                else
                {
                    _loggingService.LogWarning($"Invalid menu option selected: {selectedMenuOption}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid option selected. Please try again.");
                    ConfigureActiveUser();
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error in ConfigureUser with option: {selectedMenuOption}", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred. Please try again.");
                ConfigureActiveUser();
            }
        }

        private void CreateUser()
        {
            try
            {
                var username = GetValidatedInput(MenuMessages.UserNamePrompt, "Username", 3, 50);
                if (username == null) return;

                var name = GetValidatedInput(MenuMessages.NamePrompt, "Name", 2, 100);
                if (name == null) return;

                var response = _userService.CreateUser(username, name);

                if (response.Success)
                {
                    _userService.SetActiveUser(response.Data ?? new User() { Name = string.Empty, Username = string.Empty });
                    _menuDisplayService.RefreshWelcomeMessage();
                    _loggingService.LogInfo($"User created successfully: {username}");
                }
                else
                {
                    _loggingService.LogWarning($"Failed to create user: {response.Message}");
                }

                _menuDisplayService.DisplayMessageAndWait(response.Message);

                if (_userService.ActiveUserExists())
                {
                    _menuDisplayService.SetupFreshDisplay();
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error creating user", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while creating the user. Please try again.");
            }
        }

        private void ViewUserSelection()
        {
            try
            {
                _menuDisplayService.SetupFreshDisplay();

                var usersTable = _userService.GetUsersAsTable();

                if (!usersTable.Any())
                {
                    _loggingService.LogInfo("No users found when attempting to view user selection");
                    _menuDisplayService.DisplayMessageAndWait("No users found. Please create a user first.");
                    ConfigureActiveUser();
                    return;
                }

                Console.WriteLine("    ID    | Username        | Name");
                Console.WriteLine(new string('-', 40));

                var selectedResult = _menuDisplayService.DisplayMenuAndGetSelection(usersTable, includeBackOption: true, isDataList: true);

                if (selectedResult == MenuItems.GoBack)
                {
                    ConfigureActiveUser();
                    return;
                }

                if (string.IsNullOrWhiteSpace(selectedResult))
                {
                    _loggingService.LogWarning("Empty selection in ViewUserSelection");
                    _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                    ViewUserSelection();
                    return;
                }

                var selectedUserId = selectedResult.GetFirstNumber().GetValueOrDefault();
                if (selectedUserId <= 0)
                {
                    _loggingService.LogWarning($"Invalid user ID selected: {selectedResult}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid user selection. Please try again.");
                    ViewUserSelection();
                    return;
                }

                _userService.SetActiveUserById(selectedUserId);

                if (_userService.ActiveUserExists())
                {
                    _menuDisplayService.RefreshWelcomeMessage();
                    _menuDisplayService.SetupFreshDisplay();
                    _loggingService.LogInfo($"User selected: ID {selectedUserId}");
                }
                else
                {
                    _loggingService.LogError($"Failed to set active user with ID: {selectedUserId}");
                    _menuDisplayService.DisplayMessageAndWait("Failed to select user. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in ViewUserSelection", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while selecting user. Please try again.");
                ConfigureActiveUser();
            }
        }

        private void DisplayMenu()
        {
            try
            {
                var appRunning = true;
                var selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);

                while (appRunning)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(selectedActiveMenuOption))
                        {
                            _loggingService.LogWarning("Empty menu selection in DisplayMenu");
                            _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                            selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                            continue;
                        }

                        switch (selectedActiveMenuOption)
                        {
                            case MenuItems.ExitApplication:
                                appRunning = false;
                                _loggingService.LogInfo("User exited application");
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
                                HandleBudgetMenu();
                                selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                                break;

                            case MenuItems.ManageCategories:
                                HandleCategoryMenu();
                                selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                                break;

                            default:
                                _loggingService.LogWarning($"Invalid menu option selected: {selectedActiveMenuOption}");
                                _menuDisplayService.DisplayMessageAndWait("Invalid option selected. Please try again.");
                                selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError($"Error handling menu option: {selectedActiveMenuOption}", ex);
                        _menuDisplayService.DisplayMessageAndWait("An error occurred. Please try again.");
                        selectedActiveMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.ActiveMenuItems);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Critical error in DisplayMenu", ex);
                _menuDisplayService.DisplayMessageAndWait("A critical error occurred. The application will exit.");
                Environment.Exit(1);
            }
        }

        private void HandleBudgetMenu()
        {
            try
            {
                while (true)
                {
                    var selectedBudgetMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.BudgetsMenuItems, includeBackOption: true);

                    if (selectedBudgetMenuOption == MenuItems.GoBack)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(selectedBudgetMenuOption))
                    {
                        _loggingService.LogWarning("Empty selection in HandleBudgetMenu");
                        _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                        continue;
                    }

                    HandleBudgetMenuSelection(selectedBudgetMenuOption);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in HandleBudgetMenu", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred in budget menu. Returning to main menu.");
            }
        }

        private void HandleCategoryMenu()
        {
            try
            {
                while (true)
                {
                    var selectedCategoryMenuOption = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.CategoriesMenuItems, includeBackOption: true);

                    if (selectedCategoryMenuOption == MenuItems.GoBack)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(selectedCategoryMenuOption))
                    {
                        _loggingService.LogWarning("Empty selection in HandleCategoryMenu");
                        _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                        continue;
                    }

                    HandleCategoryMenuSelection(selectedCategoryMenuOption);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in HandleCategoryMenu", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred in category menu. Returning to main menu.");
            }
        }

        private void HandleBudgetMenuSelection(string selectedBudgetMenuOption)
        {
            try
            {
                switch (selectedBudgetMenuOption)
                {
                    case MenuItems.ViewBudgets:
                        ViewBudgetSelection();
                        break;
                    case MenuItems.CreateBudget:
                        CreateBudget();
                        break;
                    default:
                        _loggingService.LogWarning($"Invalid budget menu option: {selectedBudgetMenuOption}");
                        _menuDisplayService.DisplayMessageAndWait("Invalid option selected.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error handling budget menu selection: {selectedBudgetMenuOption}", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred. Please try again.");
            }
        }

        private void HandleCategoryMenuSelection(string selectedCategoryMenuOption)
        {
            try
            {
                switch (selectedCategoryMenuOption)
                {
                    case MenuItems.ViewCategories:
                        ViewCategorySelection();
                        break;
                    case MenuItems.CreateCategory:
                        CreateCategory();
                        break;
                    default:
                        _loggingService.LogWarning($"Invalid category menu option: {selectedCategoryMenuOption}");
                        _menuDisplayService.DisplayMessageAndWait("Invalid option selected.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error handling category menu selection: {selectedCategoryMenuOption}", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred. Please try again.");
            }
        }

        private void DeleteUser()
        {
            try
            {
                var selectedUserId = _userService.GetActiveUserId();

                if (selectedUserId <= 0)
                {
                    _loggingService.LogWarning("Attempted to delete user with invalid ID");
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

                    if (result)
                    {
                        _loggingService.LogInfo($"User deleted successfully: ID {selectedUserId}");
                    }
                    else
                    {
                        _loggingService.LogError($"Failed to delete user: ID {selectedUserId}");
                    }
                }
                else
                {
                    resultMessage = MenuMessages.DeletionCancelled;
                    _loggingService.LogInfo($"User deletion cancelled: ID {selectedUserId}");
                }

                _menuDisplayService.DisplayMessageAndWait(resultMessage);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error deleting user", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while deleting user. Please try again.");
            }
        }

        private void UpdateUser()
        {
            try
            {
                var name = GetValidatedInput(MenuMessages.NewNamePrompt, "Name", 2, 100);
                if (name == null) return;

                var response = _userService.UpdateUser(name);
                if (response.Success)
                {
                    _userService.SetActiveUser(response.Data ?? new User() { Name = string.Empty, Username = string.Empty });
                    _menuDisplayService.RefreshWelcomeMessage();
                    _loggingService.LogInfo($"User updated successfully: {name}");
                }
                else
                {
                    _loggingService.LogWarning($"Failed to update user: {response.Message}");
                }

                _menuDisplayService.DisplayMessageAndWait(response.Message);

                if (response.Success)
                {
                    _menuDisplayService.SetupFreshDisplay();
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error updating user", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while updating user. Please try again.");
            }
        }

        private void ViewBudgetSelection()
        {
            try
            {
                _menuDisplayService.SetupFreshDisplay();

                var budgetsTable = _budgetService.GetBudgetsAsTableByUserId(_userService.GetActiveUserId());

                if (!budgetsTable.Any())
                {
                    _loggingService.LogInfo("No budgets found for current user");
                    _menuDisplayService.DisplayMessageAndWait("No budgets found. Please create a budget first.");
                    return;
                }

                Console.WriteLine(new string('=', 85));
                Console.WriteLine($"    {"ID",-5} | {"Name",-30} | {"Start Date",-12} | {"End Date",-12} | {"Amount",-10}");
                Console.WriteLine(new string('-', 85));

                var selectedResult = _menuDisplayService.DisplayMenuAndGetSelection(budgetsTable, includeBackOption: true, isDataList: true);

                if (selectedResult == MenuItems.GoBack)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(selectedResult))
                {
                    _loggingService.LogWarning("Empty selection in ViewBudgetSelection");
                    _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                    ViewBudgetSelection();
                    return;
                }

                var selectedBudgetId = selectedResult.GetFirstNumber().GetValueOrDefault();
                if (selectedBudgetId <= 0)
                {
                    _loggingService.LogWarning($"Invalid budget ID selected: {selectedResult}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid budget selection. Please try again.");
                    ViewBudgetSelection();
                    return;
                }

                _budgetService.SetSelectedBudgetById(selectedBudgetId);

                if (_budgetService.SelectedBudgetExists())
                {
                    _loggingService.LogInfo($"Budget selected: ID {selectedBudgetId}");
                    HandleSelectedBudget();
                }
                else
                {
                    _loggingService.LogError($"Failed to set selected budget: ID {selectedBudgetId}");
                    _menuDisplayService.DisplayMessageAndWait("Failed to select budget. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in ViewBudgetSelection", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while viewing budgets. Please try again.");
            }
        }

        private void HandleSelectedBudget()
        {
            try
            {
                while (true)
                {
                    var selectedBudgetMenu = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.SelectedBudgetMenuItems, includeBackOption: true);

                    if (selectedBudgetMenu == MenuItems.GoBack)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(selectedBudgetMenu))
                    {
                        _loggingService.LogWarning("Empty selection in HandleSelectedBudget");
                        _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                        continue;
                    }

                    HandleSelectedBudgetAction(selectedBudgetMenu);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in HandleSelectedBudget", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred. Returning to budget menu.");
            }
        }

        private void HandleSelectedBudgetAction(string selectedBudgetMenu)
        {
            try
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

                    default:
                        _loggingService.LogWarning($"Invalid selected budget menu option: {selectedBudgetMenu}");
                        _menuDisplayService.DisplayMessageAndWait("Invalid option selected.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error handling selected budget action: {selectedBudgetMenu}", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred. Please try again.");
            }
        }

        private void ViewExpensesWithSortingAndFiltering()
        {
            try
            {
                var expenses = _expenseService.GetExpensesByBudgetId(_budgetService.GetSelectedBudgetId());

                if (!expenses.Any())
                {
                    _loggingService.LogInfo("No expenses found for selected budget");
                    _menuDisplayService.DisplayMessageAndWait("No expenses found for this budget. Add some expenses first.");
                    return;
                }

                while (true)
                {
                    var sortedExpenses = GetSortedExpenses(expenses);
                    if (sortedExpenses == null) return; // User chose to go back

                    var filteredExpenses = GetFilteredExpenses(sortedExpenses);
                    if (filteredExpenses == null) return; // User chose to go back

                    if (!filteredExpenses.Any())
                    {
                        _loggingService.LogInfo("No expenses match the selected filter criteria");
                        _menuDisplayService.DisplayMessageAndWait("No expenses match your filter criteria. Try different filters.");
                        continue;
                    }

                    _menuDisplayService.SetupFreshDisplay();
                    _menuDisplayService.ViewBudgetTotals();

                    var shouldContinue = ViewExpenseSelection(filteredExpenses);
                    if (!shouldContinue) break;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in ViewExpensesWithSortingAndFiltering", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while viewing expenses. Please try again.");
            }
        }

        private void AddExpense()
        {
            try
            {
                var description = GetValidatedInput(MenuMessages.DescriptionPrompt, "Description", 1, 200);
                if (description == null) return;

                var dateInput = GetValidatedInput(MenuMessages.DatePrompt, "Date", 10, 10);
                if (dateInput == null) return;

                if (!DateTime.TryParseExact(dateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    _loggingService.LogWarning($"Invalid date format entered: {dateInput}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid date format. Please use dd-MM-yyyy format (e.g., 15-03-2024).");
                    return;
                }

                var categoryId = GetExpenseCategory();
                if (categoryId == -1) return; // User chose to go back

                var amount = GetValidatedDecimalInput(MenuMessages.AmountPrompt);
                if (amount == null) return;

                var response = _expenseService.AddExpense(_budgetService.GetSelectedBudgetId(), categoryId, description, date, amount.Value);

                if (response.Success)
                {
                    _loggingService.LogInfo($"Expense added: {description}, Amount: {amount}, Date: {date:dd-MM-yyyy}");
                }
                else
                {
                    _loggingService.LogWarning($"Failed to add expense: {response.Message}");
                }

                _menuDisplayService.DisplayMessageAndWait(response.Message);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error adding expense", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while adding expense. Please try again.");
            }
        }

        private void UpdateBudgetAmount()
        {
            try
            {
                Console.WriteLine(MenuMessages.SkipUpdate);

                var amount = GetValidatedDecimalInput(MenuMessages.AmountPrompt);
                if (amount == null) return;

                var response = _expenseService.UpdateBudgetAmount(_budgetService.GetSelectedBudgetId(), amount.Value.ToString());

                if (response.Success)
                {
                    _loggingService.LogInfo($"Budget amount updated: {amount}");
                }
                else
                {
                    _loggingService.LogWarning($"Failed to update budget amount: {response.Message}");
                }

                _menuDisplayService.DisplayMessageAndWait(response.Message);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error updating budget amount", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while updating budget amount. Please try again.");
            }
        }

        private int GetExpenseCategory()
        {
            try
            {
                Console.WriteLine(MenuMessages.CategoryPrompt);
                var categoriesTable = _categoryService.GetCategoriesAsTable();

                if (!categoriesTable.Any())
                {
                    _loggingService.LogInfo("No categories found when adding expense");
                    _menuDisplayService.DisplayMessageAndWait("No categories found. Please create a category first.");
                    return -1;
                }

                Console.WriteLine($"    {"ID",-5} | {"Name",-30}");

                var selectedResult = _menuDisplayService.DisplayMenuAndGetSelection(categoriesTable, includeBackOption: true, isDataList: true);

                if (selectedResult == MenuItems.GoBack)
                {
                    return -1; // Signal to go back
                }

                if (string.IsNullOrWhiteSpace(selectedResult))
                {
                    _loggingService.LogWarning("Empty category selection");
                    _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                    return GetExpenseCategory();
                }

                var categoryId = selectedResult.GetFirstNumber().GetValueOrDefault();
                if (categoryId <= 0)
                {
                    _loggingService.LogWarning($"Invalid category ID selected: {selectedResult}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid category selection. Please try again.");
                    return GetExpenseCategory();
                }

                return categoryId;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error getting expense category", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while selecting category. Please try again.");
                return -1;
            }
        }

        private IEnumerable<Expense>? GetSortedExpenses(IEnumerable<Expense> expenses)
        {
            try
            {
                var sortOption = _menuDisplayService.GetSortOption();

                if (sortOption == SortItems.Back)
                {
                    return null; // Signal to go back
                }

                return sortOption switch
                {
                    SortItems.Date => expenses.OrderBy(e => e.Date),
                    SortItems.Amount => expenses.OrderByDescending(e => e.Amount),
                    SortItems.Category => expenses.OrderBy(e => _categoryService.GetCategoryById(e.CategoryId)?.Name ?? "Unknown"),
                    _ => expenses
                };
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error sorting expenses", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while sorting expenses. Showing unsorted list.");
                return expenses;
            }
        }

        private IEnumerable<Expense>? GetFilteredExpenses(IEnumerable<Expense> expenses)
        {
            try
            {
                var filterOption = _menuDisplayService.GetFilterOption();

                if (filterOption == FilterItems.Back)
                {
                    return null; // Signal to go back
                }

                return filterOption switch
                {
                    FilterItems.DateRange => FilterByDateRange(expenses),
                    FilterItems.Category => FilterByCategory(expenses),
                    _ => expenses
                };
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error filtering expenses", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while filtering expenses. Showing all expenses.");
                return expenses;
            }
        }

        private IEnumerable<Expense> FilterByDateRange(IEnumerable<Expense> expenses)
        {
            try
            {
                var startDateInput = GetValidatedInput(MenuMessages.StartDatePrompt, "Start Date", 10, 10);
                if (startDateInput == null) return expenses;

                var endDateInput = GetValidatedInput(MenuMessages.EndDatePrompt, "End Date", 10, 10);
                if (endDateInput == null) return expenses;

                if (!DateTime.TryParseExact(startDateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate) ||
                    !DateTime.TryParseExact(endDateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    _loggingService.LogWarning($"Invalid date range format: {startDateInput} to {endDateInput}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid date format. Please use dd-MM-yyyy format. Returning all expenses.");
                    return expenses;
                }

                if (startDate > endDate)
                {
                    _loggingService.LogWarning($"Start date is after end date: {startDate:dd-MM-yyyy} > {endDate:dd-MM-yyyy}");
                    _menuDisplayService.DisplayMessageAndWait("Start date cannot be after end date. Returning all expenses.");
                    return expenses;
                }

                var filteredExpenses = expenses.Where(e => e.Date >= startDate && e.Date <= endDate);
                _loggingService.LogInfo($"Filtered expenses by date range: {startDate:dd-MM-yyyy} to {endDate:dd-MM-yyyy}");
                return filteredExpenses;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error filtering by date range", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while filtering by date. Returning all expenses.");
                return expenses;
            }
        }

        private IEnumerable<Expense> FilterByCategory(IEnumerable<Expense> expenses)
        {
            try
            {
                Console.WriteLine(MenuMessages.CategoryPrompt);
                var categoriesTable = _categoryService.GetCategoriesAsTable();

                if (!categoriesTable.Any())
                {
                    _loggingService.LogInfo("No categories found when filtering expenses");
                    _menuDisplayService.DisplayMessageAndWait("No categories found. Returning all expenses.");
                    return expenses;
                }

                Console.WriteLine($"    {"ID",-5} | {"Name",-30}");

                var selectedResult = _menuDisplayService.DisplayMenuAndGetSelection(categoriesTable, includeBackOption: true, isDataList: true);

                if (selectedResult == MenuItems.GoBack)
                {
                    return expenses; // Return all expenses if user goes back
                }

                if (string.IsNullOrWhiteSpace(selectedResult))
                {
                    _loggingService.LogWarning("Empty category selection in FilterByCategory");
                    _menuDisplayService.DisplayMessageAndWait("Invalid selection. Returning all expenses.");
                    return expenses;
                }

                var selectedCategoryId = selectedResult.GetFirstNumber().GetValueOrDefault();
                if (selectedCategoryId <= 0)
                {
                    _loggingService.LogWarning($"Invalid category ID selected for filtering: {selectedResult}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid category selection. Returning all expenses.");
                    return expenses;
                }

                var filteredExpenses = expenses.Where(e => e.CategoryId == selectedCategoryId);
                _loggingService.LogInfo($"Filtered expenses by category ID: {selectedCategoryId}");
                return filteredExpenses;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error filtering by category", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while filtering by category. Returning all expenses.");
                return expenses;
            }
        }

        private void CreateBudget()
        {
            try
            {
                var name = GetValidatedInput(MenuMessages.NamePrompt, "Budget Name", 1, 100);
                if (name == null) return;

                var startDateInput = GetValidatedInput(MenuMessages.StartDatePrompt, "Start Date", 10, 10);
                if (startDateInput == null) return;

                var endDateInput = GetValidatedInput(MenuMessages.EndDatePrompt, "End Date", 10, 10);
                if (endDateInput == null) return;

                if (!DateTime.TryParseExact(startDateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
                {
                    _loggingService.LogWarning($"Invalid start date format: {startDateInput}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid start date format. Please use dd-MM-yyyy format (e.g., 01-01-2024).");
                    return;
                }

                if (!DateTime.TryParseExact(endDateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
                {
                    _loggingService.LogWarning($"Invalid end date format: {endDateInput}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid end date format. Please use dd-MM-yyyy format (e.g., 31-12-2024).");
                    return;
                }

                if (startDate >= endDate)
                {
                    _loggingService.LogWarning($"Start date is not before end date: {startDate:dd-MM-yyyy} >= {endDate:dd-MM-yyyy}");
                    _menuDisplayService.DisplayMessageAndWait("Start date must be before end date. Please try again.");
                    return;
                }

                var amount = GetValidatedDecimalInput(MenuMessages.AmountPrompt);
                if (amount == null) return;

                if (amount <= 0)
                {
                    _loggingService.LogWarning($"Invalid budget amount entered: {amount}");
                    _menuDisplayService.DisplayMessageAndWait("Budget amount must be greater than 0. Please try again.");
                    return;
                }

                var response = _budgetService.CreateBudget(_userService.GetActiveUserId(), name, startDate, endDate, amount.Value);

                if (response.Success)
                {
                    _loggingService.LogInfo($"Budget created successfully: {name}, Amount: {amount}, Period: {startDate:dd-MM-yyyy} to {endDate:dd-MM-yyyy}");
                }
                else
                {
                    _loggingService.LogWarning($"Failed to create budget: {response.Message}");
                }

                _menuDisplayService.DisplayMessageAndWait(response.Message);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error creating budget", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while creating budget. Please try again.");
            }
        }

        private void ViewCategorySelection()
        {
            try
            {
                _menuDisplayService.SetupFreshDisplay();

                var categoriesTable = _categoryService.GetCategoriesAsTable();

                if (!categoriesTable.Any())
                {
                    _loggingService.LogInfo("No categories found when viewing category selection");
                    _menuDisplayService.DisplayMessageAndWait("No categories found. Please create a category first.");
                    return;
                }

                Console.WriteLine(new string('=', 85));
                Console.WriteLine($"    {"ID",-5} | {"Name",-30}");
                Console.WriteLine(new string('-', 85));

                var selectedResult = _menuDisplayService.DisplayMenuAndGetSelection(categoriesTable, includeBackOption: true, isDataList: true);

                if (selectedResult == MenuItems.GoBack)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(selectedResult))
                {
                    _loggingService.LogWarning("Empty selection in ViewCategorySelection");
                    _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                    ViewCategorySelection();
                    return;
                }

                var selectedCategoryId = selectedResult.GetFirstNumber().GetValueOrDefault();
                if (selectedCategoryId <= 0)
                {
                    _loggingService.LogWarning($"Invalid category ID selected: {selectedResult}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid category selection. Please try again.");
                    ViewCategorySelection();
                    return;
                }

                _categoryService.SetSelectedCategoryById(selectedCategoryId);

                if (_categoryService.HasSelectedCategory())
                {
                    _loggingService.LogInfo($"Category selected: ID {selectedCategoryId}");
                    HandleSelectedCategory();
                }
                else
                {
                    _loggingService.LogError($"Failed to set selected category: ID {selectedCategoryId}");
                    _menuDisplayService.DisplayMessageAndWait("Failed to select category. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in ViewCategorySelection", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while viewing categories. Please try again.");
            }
        }

        private void HandleSelectedCategory()
        {
            try
            {
                while (true)
                {
                    var selectedCategoryMenu = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.SelectedCategoryMenuItems, includeBackOption: true);

                    if (selectedCategoryMenu == MenuItems.GoBack)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(selectedCategoryMenu))
                    {
                        _loggingService.LogWarning("Empty selection in HandleSelectedCategory");
                        _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                        continue;
                    }

                    HandleSelectedCategoryAction(selectedCategoryMenu);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in HandleSelectedCategory", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred. Returning to category menu.");
            }
        }

        private void HandleSelectedCategoryAction(string selectedCategoryMenu)
        {
            try
            {
                switch (selectedCategoryMenu)
                {
                    case MenuItems.EditCategory:
                        UpdateCategory();
                        break;
                    case MenuItems.DeleteCategory:
                        DeleteCategory();
                        break;
                    default:
                        _loggingService.LogWarning($"Invalid selected category menu option: {selectedCategoryMenu}");
                        _menuDisplayService.DisplayMessageAndWait("Invalid option selected.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error handling selected category action: {selectedCategoryMenu}", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred. Please try again.");
            }
        }

        private void UpdateCategory()
        {
            try
            {
                var name = GetValidatedInput(MenuMessages.NewNamePrompt, "Category Name", 1, 100);
                if (name == null) return;

                var response = _categoryService.UpdateCategory(name);

                if (response.Success)
                {
                    _loggingService.LogInfo($"Category updated successfully: {name}");
                }
                else
                {
                    _loggingService.LogWarning($"Failed to update category: {response.Message}");
                }

                _menuDisplayService.DisplayMessageAndWait(response.Message);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error updating category", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while updating category. Please try again.");
            }
        }

        private void DeleteCategory()
        {
            try
            {
                Console.Write(MenuMessages.DeleteCategory);
                var confirmation = Console.ReadLine()?.Trim().ToUpper();

                string resultMessage;
                if (confirmation == MenuMessages.Yes)
                {
                    var result = _categoryService.DeleteCategory();
                    resultMessage = result
                        ? MenuMessages.CategoryRemoved
                        : MenuMessages.ProblemDeletingCategory;

                    if (result)
                    {
                        _loggingService.LogInfo("Category deleted successfully");
                    }
                    else
                    {
                        _loggingService.LogError("Failed to delete category");
                    }
                }
                else
                {
                    resultMessage = MenuMessages.DeletionCancelled;
                    _loggingService.LogInfo("Category deletion cancelled");
                }

                _menuDisplayService.DisplayMessageAndWait(resultMessage);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error deleting category", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while deleting category. Please try again.");
            }
        }

        private void CreateCategory()
        {
            try
            {
                var name = GetValidatedInput(MenuMessages.NamePrompt, "Category Name", 1, 100);
                if (name == null) return;

                var response = _categoryService.CreateCategory(name);

                if (response.Success)
                {
                    _loggingService.LogInfo($"Category created successfully: {name}");
                }
                else
                {
                    _loggingService.LogWarning($"Failed to create category: {response.Message}");
                }

                _menuDisplayService.DisplayMessageAndWait(response.Message);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error creating category", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while creating category. Please try again.");
            }
        }

        private bool ViewExpenseSelection(IEnumerable<Expense> expenses)
        {
            try
            {
                if (!expenses.Any())
                {
                    _loggingService.LogInfo("No expenses to display in ViewExpenseSelection");
                    _menuDisplayService.DisplayMessageAndWait("No expenses found to display.");
                    return false;
                }

                Console.WriteLine(new string('=', 90));
                Console.WriteLine($"    {"ID",-5} | {"Description",-30} | {"Category",-20} | {"Date",-12} | {"Amount",-10}");
                Console.WriteLine(new string('-', 90));

                var expensesTable = _menuDisplayService.GetExpensesAsTable(expenses);
                var selectedResult = _menuDisplayService.DisplayMenuAndGetSelection(expensesTable, includeBackOption: true, isDataList: true);

                if (selectedResult == MenuItems.GoBack)
                {
                    return false; // Signal to stop the loop in ViewExpensesWithSortingAndFiltering
                }

                if (string.IsNullOrWhiteSpace(selectedResult))
                {
                    _loggingService.LogWarning("Empty selection in ViewExpenseSelection");
                    _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                    return true; // Continue the loop
                }

                var selectedExpenseId = selectedResult.GetFirstNumber().GetValueOrDefault();
                if (selectedExpenseId <= 0)
                {
                    _loggingService.LogWarning($"Invalid expense ID selected: {selectedResult}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid expense selection. Please try again.");
                    return true; // Continue the loop
                }

                HandleSelectedExpense(selectedExpenseId);
                return true; // Continue the loop
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in ViewExpenseSelection", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while viewing expenses. Please try again.");
                return false;
            }
        }

        private void HandleSelectedExpense(int selectedExpenseId)
        {
            try
            {
                while (true)
                {
                    var selectedExpenseMenu = _menuDisplayService.DisplayMenuAndGetSelection(MenuItems.SelectedExpenseMenuItems, includeBackOption: true);

                    if (selectedExpenseMenu == MenuItems.GoBack)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(selectedExpenseMenu))
                    {
                        _loggingService.LogWarning("Empty selection in HandleSelectedExpense");
                        _menuDisplayService.DisplayMessageAndWait("Invalid selection. Please try again.");
                        continue;
                    }

                    HandleSelectedExpenseAction(selectedExpenseMenu, selectedExpenseId);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error in HandleSelectedExpense for expense ID: {selectedExpenseId}", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred. Returning to expense list.");
            }
        }

        private void HandleSelectedExpenseAction(string selectedExpenseMenu, int expenseId)
        {
            try
            {
                switch (selectedExpenseMenu)
                {
                    case MenuItems.EditExpense:
                        UpdateExpense(expenseId);
                        break;
                    case MenuItems.DeleteExpense:
                        DeleteExpense(expenseId);
                        break;
                    default:
                        _loggingService.LogWarning($"Invalid selected expense menu option: {selectedExpenseMenu}");
                        _menuDisplayService.DisplayMessageAndWait("Invalid option selected.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error handling selected expense action: {selectedExpenseMenu} for expense ID: {expenseId}", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred. Please try again.");
            }
        }

        private void UpdateExpense(int expenseId)
        {
            try
            {
                Console.WriteLine(MenuMessages.SkipUpdate);

                var description = GetValidatedInput(MenuMessages.DescriptionPrompt, "Description", 1, 200);
                if (description == null) return;

                var dateInput = GetValidatedInput(MenuMessages.DatePrompt, "Date", 10, 10);
                if (dateInput == null) return;

                if (!DateTime.TryParseExact(dateInput, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    _loggingService.LogWarning($"Invalid date format for expense update: {dateInput}");
                    _menuDisplayService.DisplayMessageAndWait("Invalid date format. Please use dd-MM-yyyy format (e.g., 15-03-2024).");
                    return;
                }

                var categoryId = GetExpenseCategory();
                if (categoryId == -1) return; // User chose to go back

                var amount = GetValidatedDecimalInput(MenuMessages.AmountPrompt);
                if (amount == null) return;

                if (amount <= 0)
                {
                    _loggingService.LogWarning($"Invalid amount for expense update: {amount}");
                    _menuDisplayService.DisplayMessageAndWait("Amount must be greater than 0. Please try again.");
                    return;
                }

                var response = _expenseService.UpdateExpense(expenseId, categoryId, description, dateInput, amount.Value.ToString());

                if (response.Success)
                {
                    _loggingService.LogInfo($"Expense updated successfully: ID {expenseId}, Description: {description}, Amount: {amount}");
                }
                else
                {
                    _loggingService.LogWarning($"Failed to update expense ID {expenseId}: {response.Message}");
                }

                _menuDisplayService.DisplayMessageAndWait(response.Message);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error updating expense ID: {expenseId}", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while updating expense. Please try again.");
            }
        }

        private void DeleteExpense(int expenseId)
        {
            try
            {
                Console.Write(MenuMessages.DeleteExpense);
                var confirmation = Console.ReadLine()?.Trim().ToUpper();

                string resultMessage;
                if (confirmation == MenuMessages.Yes)
                {
                    var result = _expenseService.DeleteExpense(expenseId);
                    resultMessage = result
                        ? MenuMessages.ExpenseRemoved
                        : MenuMessages.ProblemDeletingExpense;

                    if (result)
                    {
                        _loggingService.LogInfo($"Expense deleted successfully: ID {expenseId}");
                    }
                    else
                    {
                        _loggingService.LogError($"Failed to delete expense: ID {expenseId}");
                    }
                }
                else
                {
                    resultMessage = MenuMessages.DeletionCancelled;
                    _loggingService.LogInfo($"Expense deletion cancelled: ID {expenseId}");
                }

                _menuDisplayService.DisplayMessageAndWait(resultMessage);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error deleting expense ID: {expenseId}", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while deleting expense. Please try again.");
            }
        }

        private string? GetValidatedInput(string prompt, string fieldName, int minLength, int maxLength)
        {
            try
            {
                while (true)
                {
                    var input = _menuDisplayService.GetUserInput(prompt);

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        _loggingService.LogWarning($"Empty input provided for {fieldName}");
                        _menuDisplayService.DisplayMessageAndWait($"{fieldName} cannot be empty. Please try again.");
                        continue;
                    }

                    input = input.Trim();

                    if (input.Length < minLength)
                    {
                        _loggingService.LogWarning($"{fieldName} too short: {input.Length} characters (minimum: {minLength})");
                        _menuDisplayService.DisplayMessageAndWait($"{fieldName} must be at least {minLength} characters long. Please try again.");
                        continue;
                    }

                    if (input.Length > maxLength)
                    {
                        _loggingService.LogWarning($"{fieldName} too long: {input.Length} characters (maximum: {maxLength})");
                        _menuDisplayService.DisplayMessageAndWait($"{fieldName} cannot be longer than {maxLength} characters. Please try again.");
                        continue;
                    }

                    return input;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error getting validated input for {fieldName}", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while getting input. Please try again.");
                return null;
            }
        }

        private decimal? GetValidatedDecimalInput(string prompt)
        {
            try
            {
                while (true)
                {
                    var input = _menuDisplayService.GetUserInput(prompt);

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        _loggingService.LogWarning("Empty input provided for decimal value");
                        _menuDisplayService.DisplayMessageAndWait("Amount cannot be empty. Please enter a valid number.");
                        continue;
                    }

                    input = input.Trim();

                    if (!decimal.TryParse(input, CultureInfo.InvariantCulture, out var amount))
                    {
                        _loggingService.LogWarning($"Invalid decimal format entered: {input}");
                        _menuDisplayService.DisplayMessageAndWait("Invalid amount format. Please enter a valid number (e.g., 25.50).");
                        continue;
                    }

                    if (amount < 0)
                    {
                        _loggingService.LogWarning($"Negative amount entered: {amount}");
                        _menuDisplayService.DisplayMessageAndWait("Amount cannot be negative. Please enter a positive number.");
                        continue;
                    }

                    return amount;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error getting validated decimal input", ex);
                _menuDisplayService.DisplayMessageAndWait("An error occurred while getting amount input. Please try again.");
                return null;
            }
        }
    }
}