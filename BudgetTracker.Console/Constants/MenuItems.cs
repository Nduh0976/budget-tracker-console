namespace BudgetTracker.Console.Constants
{
    public static class MenuItems
    {
        public const string AddExpense = "Add Expense";
        public const string ViewExpenses = "View Expenses";
        public const string EditExpense = "Edit Expense";
        public const string DeleteExpense = "Delete Expense";
        public const string SetMonthlyBudget = "Set Monthly Budget";
        public const string ViewBudgetSummary = "View Budget Summary";
        public const string AddFilterAndSortExpenses = "Add, Filter and Sort Expenses";
        public const string ManageCategories = "Manage Categories";
        public const string SwitchUser = "Switch User";
        public const string EditUser = "Edit User";
        public const string ExitApplication = "Exit Application";

        public const string AddUser = "Add User";
        public const string SelectUser = "Select Existing User";


        public const string Budgets = "Budgets";
        public const string ViewBudgets = "View Budgets";
        public const string CreateBudget = "Create Budget";

        public static readonly IList<string> ActiveMenuItems =
        [
            Budgets,
            AddFilterAndSortExpenses,
            ManageCategories,
            SwitchUser,
            EditUser,
            ExitApplication
        ];

        public static readonly IList<string> UserMenuItems =
        [
            AddUser,
            SelectUser
        ];

        public static readonly IList<string> BudgetsMenuItems =
        [
            ViewBudgets,
            CreateBudget
        ];

        public static readonly IList<string> SelectedBudgetMenuItems =
        [
            AddExpense,
            ViewExpenses,
            SetMonthlyBudget,
            ViewBudgetSummary
        ];

        public static readonly IList<string> SelectedExpenseMenuItems =
        [
            EditExpense,
            DeleteExpense
        ];
    }
}
