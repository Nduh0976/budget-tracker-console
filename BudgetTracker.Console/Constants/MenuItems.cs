﻿namespace BudgetTracker.Console.Constants
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

        public static readonly IList<string> ActiveMenuItems =
        [
            AddExpense,
            ViewExpenses,
            EditExpense,
            DeleteExpense,
            SetMonthlyBudget,
            ViewBudgetSummary,
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
    }
}
