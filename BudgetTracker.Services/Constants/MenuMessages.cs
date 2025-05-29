namespace BudgetTracker.Services.Constants
{
    public static class MenuMessages
    {
        public const string MenuNavigationHint = $"\nUse ⬆ and ⬇ to navigate and key {ForeColorConfig.GreenForeColor}Enter/Return{ForeColorConfig.ForeColorReset} to select.";
        public const string NoMenuItems = "No menu items available.";
        
        public const string DeletionCancelled = "Deletion canceled.";

        public const string ExpensesBreakDownByCategory = "\nExpenses Breakdown by Category:";
        public const string DeleteExpense = "Are you sure you want to delete this expense? (Y/N): ";
        public const string ExpenseRemoved = "Expense removed successfully.";
        public const string ProblemDeletingExpense = "There was a problem deleting the expense.";

        public const string CategoryPrompt = "Select Category:";
        public const string DeleteCategory = "Are you sure you want to delete this category? (Y/N): ";
        public const string CategoryRemoved = "Category removed successfully.";
        public const string ProblemDeletingCategory = "There was a problem deleting the category, there may be dependencies.";


        public const string NoActiveUser = "No active user found. Select or create a user.";
        public const string NoUserSelected = "No user selected.";
        public const string DeleteCurrentUser = "Are you sure you want to delete the current user? (Y/N): ";
        public const string UserRemoved = "User removed successfully.";
        public const string ProblemDeletingUser = "There was a problem deleting the user.";

        public const string FilterOptionPrompt = "\nDo you want to filter expenses?";
        public const string FilterOptionPromptOptions = "\nEnter your choice (Y/N/B): ";
        public const string FilterOption = "Select filter criteria:";
        public const string FilterByDateRange = "1. Filter by Date Range";
        public const string FilterByCategory = "2. Filter by Category";
        public const string NoFiltering = "3. No filtering";
        public const string FilteringBack = "B. ← Go Back";
        public const string FilteringSecondaryBack = "4. ← Go Back";
        public const string InputFilteringChoice = "Enter your choice (1-3): ";

        public const string SortOption = "\nSelect sorting option:";
        public const string SortByDate = "1. Sort by Date";
        public const string SortByAmount = "2. Sort by Amount";
        public const string SortByCategory = "3. Sort by Category";
        public const string NoSorting = "4. No sorting";
        public const string SortingBack = "5. ← Go Back";
        public const string InputSortingChoice = "Enter your choice (1-4): ";

        public const string ReturnToMenu = "\nPress any key to return to the menu...";
        
        public const string WelcomeSegmentOne = "*          Welcome to Budget Tracker!            *";
        public const string WelcomeSegmentTwo = "*        Take control of your finances!          *";

        public const string UserNamePrompt = "Enter username:";
        public const string NamePrompt = "Enter name:";
        public const string NewNamePrompt = "Enter new name:";

        public const string DescriptionPrompt = "Enter description:";
        public const string DatePrompt = "Enter date(dd-mm-yyyy):";
        public const string StartDatePrompt = "Enter start date(dd-mm-yyyy):";
        public const string EndDatePrompt = "Enter end date(dd-mm-yyyy):";
        public const string AmountPrompt = "Enter amount:";

        public const string GoodBye = "Good Bye!";

        public const string Yes = "Y";
        public const string Back = "B";
        
        public const string SkipUpdate = "Leave fields empty to skip update";
    }
}