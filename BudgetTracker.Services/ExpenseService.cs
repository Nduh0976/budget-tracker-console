using System.Globalization;
using BudgetTracker.Data;
using BudgetTracker.Models;

namespace BudgetTracker.Services
{
    public class ExpenseService
    {
        private readonly DataStore _dataStore;

        public ExpenseService()
        {
            _dataStore = new DataStore();
        }

        public Response<Expense> AddExpense(int budgetId, int categoryId, string description, DateTime date, decimal amount)
        {
            var budget = _dataStore.GetBudgetById(budgetId)!;

            if (string.IsNullOrWhiteSpace(description?.Trim()))
            {
                return new Response<Expense>
                {
                    Success = false,
                    Message = "Description cannot be empty or whitespace.",
                };
            }

            if (budget.StartDate > date || budget.EndDate < date)
            {
                return new Response<Expense>
                {
                    Success = false,
                    Message = "Date must be within budget start and end date.",
                };
            }

            if (amount < 0)
            {
                return new Response<Expense>
                {
                    Success = false,
                    Message = "Amount cannot be negative.",
                };
            }

            var expenses = _dataStore.GetExpenses();

            var newId = expenses.Any()
                ? expenses.Max(e => e.Id) + 1
                : 1;

            var category = _dataStore.GetCategoryById(categoryId);

            var newExpense = new Expense
            {
                Id = newId,
                BudgetId = budgetId,
                CategoryId = categoryId,
                Category = category,
                Amount = amount,
                Date = date,
                Description = description
            };

            _dataStore.AddExpense(newExpense);
            _dataStore.UpdateData();

            return new Response<Expense>
            {
                Success = true,
                Message = $"Expense '{description}' has been successfully added.",
                Data = newExpense
            };
        }

        public bool DeleteExpense(int expenseId)
        {
            return _dataStore.RemoveExpense(expenseId);
        }

        public IList<string> GetExpensesAsTableByBudgetId(int budgetId)
        {
            var expenseDescriptions = new List<string>();

            foreach (var expense in _dataStore.GetExpensesByBudgetId(budgetId))
            {
                expenseDescriptions.Add(expense.ToString());
            }

            return expenseDescriptions;
        }

        public IEnumerable<Expense> GetExpensesByBudgetId(int budgetId)
        {
            return _dataStore.GetExpensesByBudgetId(budgetId);
        }

        public Response<Expense> UpdateExpense(int expenseId, int categoryId, string description, string date, string amount)
        {
            var expense = _dataStore.GetExpenseById(expenseId)!;

            var newDescription = string.IsNullOrEmpty(description.Trim())
                ? expense.Description
                : description;

            var newDate = string.IsNullOrWhiteSpace(date.Trim())
                ? expense.Date
                : DateTime.ParseExact(date, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var newAmount = string.IsNullOrEmpty(amount.Trim())
                ? expense.Amount
                : decimal.Parse(amount);

            var budget = _dataStore.GetBudgetById(expense.BudgetId)!;

            if (budget.StartDate > newDate || budget.EndDate < newDate)
            {
                return new Response<Expense>
                {
                    Success = false,
                    Message = "Date must be within budget start and end date.",
                };
            }

            if (newAmount < 0)
            {
                return new Response<Expense>
                {
                    Success = false,
                    Message = "Amount cannot be negative.",
                };
            }

            expense.CategoryId = categoryId;
            expense.Description = newDescription;
            expense.Date = newDate;
            expense.Amount = newAmount;

            return new Response<Expense>
            {
                Success = true,
                Message = $"Expense '{newDescription}' has been successfully updated.",
                Data = expense
            };
        }
    }
}
