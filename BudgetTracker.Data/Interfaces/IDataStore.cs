using BudgetTracker.Models;

namespace BudgetTracker.Data.Interfaces
{
    public interface IDataStore
    {
        void AddBudget(Budget budget);

        void AddCategory(Category category);

        void AddExpense(Expense expense);

        void AddUser(User newUser);

        bool CategoryExists(string name);

        bool RemoveCategory(int categoryId);

        void UpdateCategoryName(int categoryId, string name);

        bool RemoveExpense(int expenseId);

        void RemoveExpensesByBudgetId(int budgetId);

        void RemoveBudget(int budgetId);

        bool UserExists(string username);

        bool RemoveUser(int userId);

        void UpdateNameOfUser(int userId, string name);

        int GenerateNextBudgetId();

        int GenerateNextCategoryId();

        int GenerateNextExpenseId();

        int GenerateNextUserId();

        IEnumerable<Budget> GetBudgets();

        Budget? GetBudgetById(int budgetId);

        IEnumerable<Budget> GetBudgetsByUserId(int userId);

        IEnumerable<Category> GetCategories();

        Category? GetCategoryById(int categoryId);

        IEnumerable<Expense> GetExpenses();

        IEnumerable<Expense> GetExpensesByBudgetId(int budgetId);

        Expense? GetExpenseById(int expenseId);

        IEnumerable<User> GetUsers();

        User? GetUserById(int userId);

        void UpdateData();
    }
}