using System.Globalization;
using BudgetTracker.Data.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services.Interfaces;

namespace BudgetTracker.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IDataStore _dataStore;

        public ExpenseService(IDataStore dataStore)
        {
            _dataStore = dataStore;
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

            var category = _dataStore.GetCategoryById(categoryId);

            var newExpense = new Expense
            {
                Id = _dataStore.GenerateNextExpenseId(),
                BudgetId = budgetId,
                CategoryId = categoryId,
                Category = category,
                Amount = amount,
                Date = date,
                Description = description
            };

            _dataStore.AddExpense(newExpense);

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

        public IEnumerable<Expense> GetExpensesByBudgetId(int budgetId)
        {
            return _dataStore.GetExpensesByBudgetId(budgetId);
        }

        public Response<Budget> UpdateBudgetAmount(int budgetId, string amount)
        {
            var budget = _dataStore.GetBudgetById(budgetId)!;

            var newAmount = string.IsNullOrEmpty(amount.Trim())
                ? budget.Amount
                : decimal.Parse(amount, CultureInfo.InvariantCulture);

            if (newAmount < 0)
            {
                return new Response<Budget>
                {
                    Success = false,
                    Message = "Amount cannot be negative.",
                };
            }

            budget.Amount = newAmount;

            _dataStore.UpdateData();

            return new Response<Budget>
            {
                Success = true,
                Message = $"Budget amount for '{budget.Name}' has been successfully updated.",
                Data = budget
            };
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

            _dataStore.UpdateData();

            return new Response<Expense>
            {
                Success = true,
                Message = $"Expense '{newDescription}' has been successfully updated.",
                Data = expense
            };
        }
    }
}