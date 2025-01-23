using BudgetTracker.Models;

namespace BudgetTracker.Data
{
    public class ApplicationData
    {
        public List<User> Users { get; set; } = [];

        public List<Budget> Budgets { get; set; } = [];

        public List<Category> Categories { get; set; } = [];

        public List<Expense> Expenses { get; set; } = [];
    }
}
