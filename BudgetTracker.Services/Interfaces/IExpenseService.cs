using BudgetTracker.Models;

namespace BudgetTracker.Services.Interfaces
{
    public interface IExpenseService
    {
        Response<Expense> AddExpense(int budgetId, int categoryId, string description, DateTime date, decimal amount);

        bool DeleteExpense(int expenseId);

        IEnumerable<Expense> GetExpensesByBudgetId(int budgetId);

        Response<Expense> UpdateExpense(int expenseId, int categoryId, string description, string date, string amount);

        Response<Budget> UpdateBudgetAmount(int budgetId, string amount);
    }
}