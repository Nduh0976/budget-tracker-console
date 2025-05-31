using System.Text;
using BudgetTracker.Models;
using BudgetTracker.Services.Constants;
using BudgetTracker.Services.Interfaces;

namespace BudgetTracker.Services
{
    public class MenuDisplayService
    {
        private readonly IUserService _userService;
        private readonly IBudgetService _budgetService;
        private readonly ICategoryService _categoryService;
        private readonly IExpenseService _expenseService;
        private readonly ILoggingService _loggingService;
        private string _cachedWelcomeMessage = string.Empty;

        public MenuDisplayService(
            IUserService userService,
            IBudgetService budgetService,
            ICategoryService categoryService,
            IExpenseService expenseService,
            ILoggingService loggingService)
        {
            _userService = userService;
            _budgetService = budgetService;
            _categoryService = categoryService;
            _expenseService = expenseService;
            _loggingService = loggingService;
        }

        public string DisplayMenuAndGetSelection(IList<string> menuItems, bool includeBackOption = false, bool includeExitOption = false, bool isDataList = false)
        {
            try
            {
                if (menuItems == null || menuItems.Count == 0)
                {
                    var message = isDataList
                        ? MenuMessages.NoData
                        : MenuMessages.NoMenuItems;

                    _loggingService.LogWarning($"Menu displayed with no items. IsDataList: {isDataList}");

                    if (!includeBackOption && !includeExitOption)
                    {
                        DisplayMessageAndWait(message);
                        return string.Empty;
                    }

                    // If we have navigation options, create a list with just those options
                    menuItems = [];
                }

                // Create a new list with navigation options
                var extendedMenuItems = new List<string>(menuItems);

                if (includeBackOption)
                {
                    extendedMenuItems.Add(MenuItems.GoBack);
                }

                if (includeExitOption)
                {
                    extendedMenuItems.Add(MenuItems.ExitApplication);
                }

                // Clear and setup fresh display before showing menu
                SetupFreshDisplay();

                if (isDataList && (menuItems == null || menuItems.Count == 0))
                {
                    DisplayMessage($"{MenuMessages.NoData}\n");
                }

                // Only show the navigation hint only if there are multiple items to scroll through
                if (extendedMenuItems.Count > 1)
                {
                    DisplayMessage(MenuMessages.MenuNavigationHint);
                }

                var (Left, Top) = Console.GetCursorPosition();
                var selectedOption = 0;
                var isSelected = false;

                while (!isSelected)
                {
                    try
                    {
                        Console.SetCursorPosition(Left, Top);
                        Console.WriteLine(GetMenu(selectedOption, extendedMenuItems));

                        var keyPressed = Console.ReadKey(true).Key;

                        switch (keyPressed)
                        {
                            case ConsoleKey.DownArrow:
                                selectedOption = (selectedOption + 1) % extendedMenuItems.Count;
                                break;

                            case ConsoleKey.UpArrow:
                                selectedOption = (selectedOption - 1 + extendedMenuItems.Count) % extendedMenuItems.Count;
                                break;

                            case ConsoleKey.Enter:
                                isSelected = true;
                                break;

                            case ConsoleKey.Escape:
                                if (includeBackOption)
                                {
                                    selectedOption = extendedMenuItems.IndexOf(MenuItems.GoBack);
                                    isSelected = true;
                                }
                                else if (includeExitOption)
                                {
                                    selectedOption = extendedMenuItems.IndexOf(MenuItems.ExitApplication);
                                    isSelected = true;
                                }
                                break;

                            default:
                                // Log unexpected key presses for debugging
                                _loggingService.LogInfo($"Unexpected key pressed in menu navigation: {keyPressed}");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError("Error during menu navigation", ex);
                        // Continue the loop to allow user to try again
                    }
                }

                // Show cursor for any subsequent input
                Console.CursorVisible = true;

                var selectedUserMenuOption = extendedMenuItems[selectedOption];
                DisplayMessage($"{ForeColorConfig.GreenForeColor}You selected {selectedUserMenuOption}{ForeColorConfig.ForeColorReset}");

                _loggingService.LogInfo($"User selected menu option: {selectedUserMenuOption}");
                return selectedUserMenuOption;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Critical error in DisplayMenuAndGetSelection", ex);
                DisplayMessageAndWait("An error occurred while displaying the menu. Please try again.");
                return string.Empty;
            }
        }

        public string GetSortOption()
        {
            try
            {
                DisplayMessage(MenuMessages.SortOption);
                DisplayMessage(MenuMessages.SortByDate);
                DisplayMessage(MenuMessages.SortByAmount);
                DisplayMessage(MenuMessages.SortByCategory);
                DisplayMessage(MenuMessages.NoSorting);
                DisplayMessage(MenuMessages.SortingBack);

                var input = GetUserInputWithValidation(MenuMessages.InputSortingChoice,
                    [SortItems.DateOption, SortItems.AmountOption, SortItems.CategoryOption, SortItems.NoSortingOption, SortItems.BackOption],
                    "Invalid selection. Please choose 1-5.");

                var result = input switch
                {
                    SortItems.DateOption => SortItems.Date,
                    SortItems.AmountOption => SortItems.Amount,
                    SortItems.CategoryOption => SortItems.Category,
                    SortItems.NoSortingOption => SortItems.NoSorting,
                    SortItems.BackOption => SortItems.Back,
                    _ => SortItems.NoSorting,
                };

                _loggingService.LogInfo($"User selected sort option: {result}");
                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in GetSortOption", ex);
                return SortItems.NoSorting;
            }
        }

        public string GetFilterOption()
        {
            try
            {
                DisplayMessage(MenuMessages.FilterOptionPrompt);
                DisplayMessage(MenuMessages.FilteringBack);

                var filterChoice = GetUserInputWithValidation(MenuMessages.FilterOptionPromptOptions,
                    [FilterItems.YesChoiceOption, FilterItems.NoChoiceOption, FilterItems.BackChoiceOption],
                    "Invalid input. Please enter Y (Yes), N (No), or B (Back).").ToUpper();

                if (filterChoice == FilterItems.BackChoiceOption)
                {
                    return FilterItems.Back;
                }

                if (filterChoice != FilterItems.YesChoiceOption)
                {
                    return FilterItems.NoFilter;
                }

                DisplayMessage(MenuMessages.FilterOption);
                DisplayMessage(MenuMessages.FilterByDateRange);
                DisplayMessage(MenuMessages.FilterByCategory);
                DisplayMessage(MenuMessages.NoFiltering);
                DisplayMessage(MenuMessages.FilteringSecondaryBack);

                var filterOption = GetUserInputWithValidation(MenuMessages.InputFilteringChoice,
                    [FilterItems.DateRangeOption, FilterItems.CategoryOption, FilterItems.NoFilterOption, FilterItems.BackOption],
                    "Invalid selection. Please choose 1-4.");

                var result = filterOption switch
                {  
                    FilterItems.DateRangeOption => FilterItems.DateRange,
                    FilterItems.CategoryOption => FilterItems.Category,
                    FilterItems.NoFilterOption => FilterItems.NoFilter,
                    FilterItems.BackOption => FilterItems.Back,
                    _ => FilterItems.NoFilter,
                };

                _loggingService.LogInfo($"User selected filter option: {result}");
                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in GetFilterOption", ex);
                return FilterItems.NoFilter;
            }
        }

        public string GetWelcomeMessage()
        {
            try
            {
                if (!string.IsNullOrEmpty(_cachedWelcomeMessage))
                {
                    return _cachedWelcomeMessage;
                }

                var currentDate = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                var welcomeMessage = new StringBuilder();

                // Add top border
                welcomeMessage.AppendLine(new string('*', 50));
                welcomeMessage.AppendLine(MenuMessages.WelcomeSegmentOne);
                welcomeMessage.AppendLine(MenuMessages.WelcomeSegmentTwo);
                welcomeMessage.AppendLine(new string('*', 50));

                // Add date
                welcomeMessage.AppendLine($"\nToday is: {currentDate}");

                // Add active user info if available
                var activeUserName = _userService.GetActiveUserName();
                if (!string.IsNullOrEmpty(activeUserName))
                {
                    welcomeMessage.AppendLine($"Active User: {activeUserName}");
                }
                else
                {
                    welcomeMessage.AppendLine(MenuMessages.NoActiveUser);
                }

                // Add bottom border
                welcomeMessage.AppendLine(new string('*', 50));

                _cachedWelcomeMessage = welcomeMessage.ToString();
                return _cachedWelcomeMessage;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error generating welcome message", ex);
                return "Welcome to Budget Tracker\n" + new string('*', 50);
            }
        }

        public void RefreshWelcomeMessage()
        {
            try
            {
                _cachedWelcomeMessage = string.Empty;
                _loggingService.LogInfo("Welcome message cache refreshed");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error refreshing welcome message", ex);
            }
        }

        public string GetUserInput(string message)
        {
            try
            {
                Console.CursorVisible = true;

                if (!string.IsNullOrEmpty(message))
                {
                    DisplayMessage(message);
                }

                var input = Console.ReadLine();
                Console.CursorVisible = false;

                var result = input?.Trim() ?? string.Empty;
                _loggingService.LogInfo($"User input received for prompt: {message?.Substring(0, Math.Min(message.Length, 50))}...");
                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error getting user input for message: {message}", ex);
                Console.CursorVisible = false;
                return string.Empty;
            }
        }

        public string GetUserInputWithValidation(string message, string[] validOptions, string errorMessage)
        {
            try
            {
                string input;
                do
                {
                    input = GetUserInput(message).Trim().ToUpper();

                    if (validOptions.Contains(input.ToUpper()))
                    {
                        return input;
                    }

                    DisplayMessage($"{ForeColorConfig.RedForeColor}{errorMessage}{ForeColorConfig.ForeColorReset}");
                    _loggingService.LogWarning($"Invalid user input '{input}' for prompt: {message}");

                } while (true);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error in GetUserInputWithValidation for message: {message}", ex);
                return validOptions.FirstOrDefault() ?? string.Empty;
            }
        }

        public void DisplayMessage(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error displaying message: {message}", ex);
            }
        }

        public void DisplayMessageAndWait(string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    Console.WriteLine(message);
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey(true);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error in DisplayMessageAndWait: {message}", ex);
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }

        public IList<string> GetExpensesAsTable(IEnumerable<Expense> expenses)
        {
            try
            {
                if (expenses == null)
                {
                    _loggingService.LogWarning("GetExpensesAsTable called with null expenses");
                    return new List<string>();
                }

                var expenseDescriptions = new List<string>();
                var expenseList = expenses.ToList();

                if (!expenseList.Any())
                {
                    _loggingService.LogInfo("No expenses found for table display");
                    return expenseDescriptions;
                }

                foreach (var expense in expenseList)
                {
                    if (expense != null)
                    {
                        expenseDescriptions.Add(expense.ToString());
                    }
                }

                _loggingService.LogInfo($"Generated table for {expenseDescriptions.Count} expenses");
                return expenseDescriptions;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in GetExpensesAsTable", ex);
                return new List<string>();
            }
        }

        public void ViewBudgetTotals()
        {
            try
            {
                var amountUsed = _budgetService.GetSelectedBudgetTotalSpent();
                var remainingBalance = _budgetService.GetSelectedBudgetRemainingBalance();
                var budgetName = _budgetService.GetSelectedBudgetName();
                var totalAmount = _budgetService.GetSelectedBudgetAmount();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(new string('=', 90));
                Console.WriteLine($" Budget Details - {budgetName} ");
                Console.WriteLine(new string('=', 90));
                Console.ResetColor();

                DisplayMessage($"Total Amount: {totalAmount:C}");
                DisplayMessage($"Amount Used:  {amountUsed:C}");
                DisplayMessage($"Remaining Balance: {remainingBalance:C}\n");

                _loggingService.LogInfo($"Displayed budget totals for: {budgetName}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error displaying budget totals", ex);
                DisplayMessage("Error loading budget information.");
            }
        }

        public void ViewBudgetSummary()
        {
            try
            {
                SetupFreshDisplay();

                var budgetName = _budgetService.GetSelectedBudgetName();
                var startDate = _budgetService.GetSelectedBudgetStartDate().ToString("dd-MM-yyyy");
                var endDate = _budgetService.GetSelectedBudgetEndDate().ToString("dd-MM-yyyy");
                var totalAmount = _budgetService.GetSelectedBudgetAmount();
                var amountUsed = _budgetService.GetSelectedBudgetTotalSpent();
                var remainingBalance = _budgetService.GetSelectedBudgetRemainingBalance();

                var percentageUsed = (totalAmount > 0) ? Math.Round((amountUsed / totalAmount) * 100, 2) : 0;
                var percentageRemaining = 100 - percentageUsed;

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(new string('=', 85));
                Console.WriteLine($" Budget Summary - {budgetName} ");
                Console.WriteLine(new string('=', 85));
                Console.ResetColor();

                // Print detailed budget information
                DisplayMessage($"\nBudget Period: {startDate} to {endDate}");
                DisplayMessage($"Total Amount: {totalAmount:C}");
                DisplayMessage($"Amount Used:  {amountUsed:C} ({percentageUsed}% of total)");
                DisplayMessage($"Remaining Balance: {remainingBalance:C} ({percentageRemaining}% of total)\n");

                DisplayMessage(new string('-', 85));

                DisplayProgressBar(percentageUsed);
                DisplayTotalExpensesByCategories();

                DisplayMessage(MenuMessages.ReturnToMenu);
                Console.ReadKey(true);

                _loggingService.LogInfo($"Displayed budget summary for: {budgetName}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error displaying budget summary", ex);
                DisplayMessageAndWait("Error loading budget summary.");
            }
        }

        public void SetupFreshDisplay()
        {
            try
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine(GetWelcomeMessage());
                Console.ResetColor();
                Console.CursorVisible = false;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error setting up fresh display", ex);
                Console.Clear();
                Console.WriteLine("Budget Tracker");
            }
        }

        private static void DisplayProgressBar(decimal percentage)
        {
            try
            {
                const int progressBarWidth = 50;
                int filledWidth = (int)(percentage / 100 * progressBarWidth);

                Console.Write("Progress: [");

                // Color based on usage percentage
                if (percentage >= 90)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (percentage >= 70)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.Green;

                Console.Write(new string('█', filledWidth));
                Console.ResetColor();
                Console.Write(new string('░', progressBarWidth - filledWidth));
                Console.WriteLine($"] {percentage}%");
            }
            catch (Exception)
            {
                Console.WriteLine($"Progress: {percentage}%");
            }
        }

        private void DisplayTotalExpensesByCategories()
        {
            try
            {
                DisplayMessage("\n" + MenuMessages.ExpensesBreakDownByCategory);
                DisplayMessage(new string('-', 85));
                DisplayMessage($"{"Category",-30} | {"Total Expense",-15} | {"% of Budget",-12}");
                DisplayMessage(new string('-', 85));

                var totalBudget = _budgetService.GetSelectedBudgetAmount();

                // Fetch and group expenses by category
                var expensesByCategory = _expenseService
                    .GetExpensesByBudgetId(_budgetService.GetSelectedBudgetId())
                    .GroupBy(e => e.CategoryId)
                    .Select(group => new
                    {
                        CategoryId = group.Key,
                        TotalAmount = group.Sum(e => e.Amount),
                        CategoryName = _categoryService.GetCategoryById(group.Key)?.Name ?? "Unknown",
                        PercentageOfBudget = totalBudget > 0 ? Math.Round((group.Sum(e => e.Amount) / totalBudget) * 100, 2) : 0
                    })
                    .OrderByDescending(g => g.TotalAmount);

                var categoryList = expensesByCategory.ToList();

                if (!categoryList.Any())
                {
                    DisplayMessage("No expenses found for this budget.");
                    _loggingService.LogInfo("No expenses found for category breakdown");
                }
                else
                {
                    foreach (var category in categoryList)
                    {
                        DisplayMessage($"{category.CategoryName,-30} | {category.TotalAmount,-15:C} | {category.PercentageOfBudget,-12:F1}%");
                    }
                    _loggingService.LogInfo($"Displayed expense breakdown for {categoryList.Count} categories");
                }

                DisplayMessage(new string('-', 85));
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error displaying expenses by categories", ex);
                DisplayMessage("Error loading expense breakdown by categories.");
            }
        }

        private static string GetMenu(int selectedOption, IList<string> userMenuItems)
        {
            try
            {
                var menuBuilder = new StringBuilder();
                var selectedOptionMarker = $"✅  {ForeColorConfig.GreenForeColor}";

                for (var i = 0; i < userMenuItems.Count; i++)
                {
                    var marker = selectedOption == i ? selectedOptionMarker : "    ";
                    menuBuilder.AppendLine($"{marker}{userMenuItems[i]}{ForeColorConfig.ForeColorReset}");
                }

                return menuBuilder.ToString();
            }
            catch (Exception)
            {
                return "Error displaying menu items.";
            }
        }
    }
}