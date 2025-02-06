using BudgetTracker.Data;
using BudgetTracker.Models;

namespace BudgetTracker.Services
{
    public class BudgetService
    {
        public Budget SelectedBudget { get; set; }

        private readonly DataStore _dataStore;

        public BudgetService()
        {
            SelectedBudget = new Budget() { Name = string.Empty };
            _dataStore = new DataStore();
        }

        public Response<Budget> CreateBudget(int userId, string? name, DateTime startDate, DateTime endDate, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(name?.Trim()))
            {
                return new Response<Budget>
                {
                    Success = false,
                    Message = "Name cannot be empty or whitespace.",
                };
            }

            if (startDate > endDate)
            {
                return new Response<Budget>
                {
                    Success = false,
                    Message = "End date cannot be earlier than start date.",
                };
            }

            if (amount < 0) 
            {
                return new Response<Budget>
                {
                    Success = false,
                    Message = "Amount cannot be negative.",
                };
            }

            var budgets = _dataStore.GetBudgets();

            var newId = budgets.Any()
                ? budgets.Max(b => b.Id) + 1
                : 1;

            var newBudget = new Budget
            {
                Id = newId,
                UserId = userId,
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                Amount = amount
            };

            _dataStore.AddBudget(newBudget);
            _dataStore.UpdateData();

            return new Response<Budget>
            {
                Success = true,
                Message = $"Budget '{name}' has been successfully created.",
                Data = newBudget
            };
        }

        public IList<string> GetBudgetsAsTableByUserId(int userId)
        {
            var budgetDescriptions = new List<string>();

            foreach (var budget in _dataStore.GetBudgetsByUserId(userId))
            {
                budgetDescriptions.Add(budget.ToString());
            }

            return budgetDescriptions;
        }

        public void SetSelectedBudgetById(int budgetId)
        {
            SelectedBudget = _dataStore.GetBudgetById(budgetId)!;
        }
    }
}
