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

        public string DisplayMenuAndGetSelection(IList<string> menuItems)
        {
            if (menuItems == null || menuItems.Count == 0)
            {
                Console.WriteLine(MenuMessages.NoMenuItems);
                return string.Empty; // Indicates no selection
            }

            var (Left, Top) = Console.GetCursorPosition();
            var selectedOption = 0;
            var isSelected = false;

            while (!isSelected)
            {
                Console.SetCursorPosition(Left, Top);
                Console.WriteLine(GetMenu(selectedOption, menuItems));

                switch (Console.ReadKey(false).Key)
                {
                    case ConsoleKey.DownArrow:
                        selectedOption = (selectedOption + 1) % menuItems.Count;
                        break;

                    case ConsoleKey.UpArrow:
                        selectedOption = (selectedOption - 1 + menuItems.Count) % menuItems.Count;
                        break;

                    case ConsoleKey.Enter:
                        isSelected = true;
                        break;
                }
            }

            ConfigureConsole();

            var selectedUserMenuOption = menuItems[selectedOption];
            Console.WriteLine($"{ForeColorConfig.GreenForeColor}You selected {selectedUserMenuOption}{ForeColorConfig.ForeColorReset}");

            return selectedUserMenuOption;
        }

        public string GetSortOption()
        {
            Console.WriteLine(MenuMessages.SortOption);
            Console.WriteLine(MenuMessages.SortByDate);
            Console.WriteLine(MenuMessages.SortByAmount);
            Console.WriteLine(MenuMessages.SortByCategory);
            Console.WriteLine(MenuMessages.NoSorting);

            var input = GetUserInput(MenuMessages.InputSortingChoice);

            return input switch
            {
                "1" => SortItems.Date,
                "2" => SortItems.Amount,
                "3" => SortItems.Category,
                _ => SortItems.NoSorting,
            };
        }

        public string GetFilterOption()
        {
            Console.WriteLine(MenuMessages.FilterOptionPrompt);
            var filterChoice = GetUserInput("").Trim().ToUpper();

            if (filterChoice != MenuMessages.Yes) return FilterItems.NoFilter;

            Console.WriteLine(MenuMessages.FilterOption);
            Console.WriteLine(MenuMessages.FilterByDateRange);
            Console.WriteLine(MenuMessages.FilterByCategory);
            Console.WriteLine(MenuMessages.NoFiltering);

            var filterOption = GetUserInput(MenuMessages.InputFilteringChoice);

            return filterOption switch
            {
                "1" => FilterItems.DateRange,
                "2" => FilterItems.Category,
                _ => FilterItems.NoFilter,
            };
        }

        public string GetWelcomeMessage()
        {
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
            if (!string.IsNullOrEmpty(_userService.GetActiveUserName()))
            {
                welcomeMessage.AppendLine($"Active User: {_userService.GetActiveUserName()}");
            }
            else
            {
                welcomeMessage.AppendLine(MenuMessages.NoActiveUser);
            }

            // Add bottom border
            welcomeMessage.AppendLine(new string('*', 50));

            return welcomeMessage.ToString();
        }

        public string GetUserInput(string message)
        {
            Console.CursorVisible = !Console.CursorVisible; Console.WriteLine(message);
            var input = Console.ReadLine();
            Console.CursorVisible = !Console.CursorVisible;

            return input ?? string.Empty;
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

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(new string('=', 90));
            Console.WriteLine($" Budget Details - {_budgetService.GetSelectedBudgetName()} ");
            Console.WriteLine(new string('=', 90));
            Console.ResetColor();

            Console.WriteLine($"Total Amount: {_budgetService.GetSelectedBudgetAmount()}");
            Console.WriteLine($"Amount Used:  {amountUsed}");
            Console.WriteLine($"Remaining Balance: {remainingBalance}\n");
        }

        public void ViewBudgetSummary()
        {
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
            Console.WriteLine($"\nStart Date: {startDate}");
            Console.WriteLine($"End Date:   {endDate}");
            Console.WriteLine($"Total Amount: {totalAmount:C}");
            Console.WriteLine($"Amount Used:  {amountUsed:C} ({percentageUsed}% of total)");
            Console.WriteLine($"Remaining Balance: {remainingBalance:C} ({percentageRemaining}% of total)\n");

            Console.WriteLine(new string('-', 85));

            DisplayProgressBar(percentageUsed);
            DisplayTotalExpensesByCategories();
        }

        private void ConfigureConsole()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(GetWelcomeMessage());
            Console.ResetColor();
            Console.WriteLine(MenuMessages.MenuNavigationHint);
            Console.CursorVisible = false;
        }

        private static void DisplayProgressBar(decimal percentage)
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

        private void DisplayTotalExpensesByCategories()
        {
            Console.WriteLine(MenuMessages.ExpensesBreakDownByCategory);
            Console.WriteLine(new string('-', 85));
            Console.WriteLine($"{"Category",-30} | {"Total Expense",-15}");
            Console.WriteLine(new string('-', 85));

            // Fetch and group expenses by category
            var expensesByCategory = _expenseService
                .GetExpensesByBudgetId(_budgetService.GetSelectedBudgetId())
                .GroupBy(e => e.CategoryId)
                .Select(group => new
                {
                    CategoryId = group.Key,
                    TotalAmount = group.Sum(e => e.Amount),
                    CategoryName = _categoryService.GetCategoryById(group.Key)?.Name ?? "Unknown"
                })
                .OrderByDescending(g => g.TotalAmount);

            foreach (var category in expensesByCategory)
            {
                Console.WriteLine($"{category.CategoryName,-30} | {category.TotalAmount:C}");
            }

            Console.WriteLine(new string('-', 85));

            Console.WriteLine(MenuMessages.ReturnToMenu);
            Console.ReadKey();
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