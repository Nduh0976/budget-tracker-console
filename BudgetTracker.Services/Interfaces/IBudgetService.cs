using BudgetTracker.Models;

namespace BudgetTracker.Services.Interfaces
{
    public interface IBudgetService
    {
        Response<Budget> CreateBudget(int userId, string? name, DateTime startDate, DateTime endDate, decimal amount);

        void DeleteBudgetByUserId(int userId);

        IList<string> GetBudgetsAsTableByUserId(int userId);

        int GetSelectedBudgetId();

        decimal GetSelectedBudgetAmount();

        decimal GetSelectedBudgetRemainingBalance();

        decimal GetSelectedBudgetTotalSpent();

        string GetSelectedBudgetName();

        DateTime GetSelectedBudgetStartDate();

        DateTime GetSelectedBudgetEndDate();

        bool SelectedBudgetExists();

        void SetSelectedBudgetById(int budgetId);
    }
}