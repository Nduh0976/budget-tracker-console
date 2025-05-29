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
        private string _cachedWelcomeMessage = string.Empty;

        public MenuDisplayService(
            IUserService userService,
            IBudgetService budgetService,
            ICategoryService categoryService,
            IExpenseService expenseService)
        {
            _userService = userService;
            _budgetService = budgetService;
            _categoryService = categoryService;
            _expenseService = expenseService;
        }

        public string DisplayMenuAndGetSelection(IList<string> menuItems, bool includeBackOption = false, bool includeExitOption = false, bool isDataList = false)
        {
            if (menuItems == null || menuItems.Count == 0)
            {
                if (!includeBackOption && !includeExitOption)
                {
                    var message = isDataList
                        ? MenuMessages.NoData
                        : MenuMessages.NoMenuItems;
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
                Console.SetCursorPosition(Left, Top);
                Console.WriteLine(GetMenu(selectedOption, extendedMenuItems));

                switch (Console.ReadKey(true).Key)
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
                }
            }

            // Show cursor for any subsequent input
            Console.CursorVisible = true;

            var selectedUserMenuOption = extendedMenuItems[selectedOption];
            DisplayMessage($"{ForeColorConfig.GreenForeColor}You selected {selectedUserMenuOption}{ForeColorConfig.ForeColorReset}");

            return selectedUserMenuOption;
        }

        public string GetSortOption()
        {
            DisplayMessage(MenuMessages.SortOption);
            DisplayMessage(MenuMessages.SortByDate);
            DisplayMessage(MenuMessages.SortByAmount);
            DisplayMessage(MenuMessages.SortByCategory);
            DisplayMessage(MenuMessages.NoSorting);
            DisplayMessage(MenuMessages.SortingBack);

            var input = GetUserInput(MenuMessages.InputSortingChoice);

            return input switch
            {
                "1" => SortItems.Date,
                "2" => SortItems.Amount,
                "3" => SortItems.Category,
                "4" => SortItems.NoSorting,
                "5" => SortItems.Back,
                _ => SortItems.NoSorting,
            };
        }

        public string GetFilterOption()
        {
            DisplayMessage(MenuMessages.FilterOptionPrompt);
            DisplayMessage(MenuMessages.FilteringBack);
            var filterChoice = GetUserInput(MenuMessages.FilterOptionPromptOptions).Trim().ToUpper();

            if (filterChoice == MenuMessages.Back)
            {
                return FilterItems.Back;
            }

            if (filterChoice != MenuMessages.Yes)
            {
                return FilterItems.NoFilter;
            }

            DisplayMessage(MenuMessages.FilterOption);
            DisplayMessage(MenuMessages.FilterByDateRange);
            DisplayMessage(MenuMessages.FilterByCategory);
            DisplayMessage(MenuMessages.NoFiltering);
            DisplayMessage(MenuMessages.FilteringSecondaryBack);

            var filterOption = GetUserInput(MenuMessages.InputFilteringChoice);

            return filterOption switch
            {
                "1" => FilterItems.DateRange,
                "2" => FilterItems.Category,
                "3" => FilterItems.NoFilter,
                "4" => FilterItems.Back,
                _ => FilterItems.NoFilter,
            };
        }

        public string GetWelcomeMessage()
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

        public void RefreshWelcomeMessage()
        {
            _cachedWelcomeMessage = string.Empty;
        }

        public string GetUserInput(string message)
        {
            Console.CursorVisible = true;

            if (!string.IsNullOrEmpty(message))
            {
                DisplayMessage(message);
            }

            var input = Console.ReadLine();
            Console.CursorVisible = false;

            return input?.Trim() ?? string.Empty;
        }

        public void DisplayMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine(message);
            }
        }

        public void DisplayMessageAndWait(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Console.WriteLine(message);
                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey(true);
            }
        }

        public IList<string> GetExpensesAsTable(IEnumerable<Expense> expenses)
        {
            var expenseDescriptions = new List<string>();

            foreach (var expense in expenses)
            {
                expenseDescriptions.Add(expense.ToString());
            }

            return expenseDescriptions;
        }

        public void ViewBudgetTotals()
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
        }

        public void ViewBudgetSummary()
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
        }

        public void SetupFreshDisplay()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(GetWelcomeMessage());
            Console.ResetColor();
            Console.CursorVisible = false;
        }

        private static void DisplayProgressBar(decimal percentage)
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

        private void DisplayTotalExpensesByCategories()
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

            foreach (var category in expensesByCategory)
            {
                DisplayMessage($"{category.CategoryName,-30} | {category.TotalAmount,-15:C} | {category.PercentageOfBudget,-12:F1}%");
            }

            DisplayMessage(new string('-', 85));
        }

        private static string GetMenu(int selectedOption, IList<string> userMenuItems)
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
    }
}