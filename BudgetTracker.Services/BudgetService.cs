using BudgetTracker.Data.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services.Interfaces;

namespace BudgetTracker.Services
{
    public class BudgetService : IBudgetService
    {
        private Budget selectedBudget { get; set; }

        private readonly IDataStore _dataStore;

        public BudgetService(IDataStore dataStore)
        {
            selectedBudget = new Budget() { Name = string.Empty };
            _dataStore = dataStore;
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

            var newBudget = new Budget
            {
                Id = _dataStore.GenerateNextBudgetId(),
                UserId = userId,
                Name = name,
                StartDate = startDate,
                EndDate = endDate,
                Amount = amount
            };

            _dataStore.AddBudget(newBudget);

            return new Response<Budget>
            {
                Success = true,
                Message = $"Budget '{name}' has been successfully created.",
                Data = newBudget
            };
        }

        public void DeleteBudgetByUserId(int userId)
        {
            var budgets = _dataStore.GetBudgetsByUserId(userId).ToList();

            foreach (var budgetId in budgets.Select(b => b.Id))
            {
                _dataStore.RemoveExpensesByBudgetId(budgetId);
                _dataStore.RemoveBudget(budgetId);
            }
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

        public int GetSelectedBudgetId()
        {
            return selectedBudget.Id;
        }

        public decimal GetSelectedBudgetAmount()
        {
            return selectedBudget.Amount;
        }

        public decimal GetSelectedBudgetRemainingBalance()
        {
            return selectedBudget.Amount - GetSelectedBudgetTotalSpent();
        }

        public decimal GetSelectedBudgetTotalSpent()
        {
            var expenses = _dataStore.GetExpensesByBudgetId(selectedBudget.Id);

            return expenses.Sum(e => e.Amount);
        }

        public string GetSelectedBudgetName()
        {
            return selectedBudget.Name;
        }

        public DateTime GetSelectedBudgetStartDate()
        {
            return selectedBudget.StartDate;
        }

        public DateTime GetSelectedBudgetEndDate()
        {
            return selectedBudget.EndDate;
        }

        public bool SelectedBudgetExists()
        {
            return selectedBudget.Exists();
        }

        public void SetSelectedBudgetById(int budgetId)
        {
            selectedBudget = _dataStore.GetBudgetById(budgetId)!;
        }
    }
}
