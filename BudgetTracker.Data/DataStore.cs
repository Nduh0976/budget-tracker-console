using BudgetTracker.Data.Interfaces;
using BudgetTracker.Models;
using Newtonsoft.Json;

namespace BudgetTracker.Data
{
    public class DataStore : IDataStore
    {
        private readonly string _filePath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\BudgetTracker.Console\bin\Debug\net8.0\", @"\BudgetTracker.Data\ApplicationData.json");
        private ApplicationData _applicationData;

        public DataStore()
        {
            _applicationData = new ApplicationData();
            LoadData();

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                {
                    IgnoreSerializableAttribute = true
                },
                Formatting = Formatting.Indented
            };
        }

        public void AddBudget(Budget budget)
        {
            _applicationData.Budgets.Add(budget);
            UpdateData();
        }

        public void AddCategory(Category category)
        {
            _applicationData.Categories.Add(category);
            UpdateData();
        }

        public void AddExpense(Expense expense)
        {
            _applicationData.Expenses.Add(expense);
            UpdateData();
        }

        public void AddUser(User newUser)
        {
            _applicationData.Users.Add(newUser);
            UpdateData();
        }

        public IEnumerable<Budget> GetBudgets()
        {
            return _applicationData.Budgets;
        }

        public Budget? GetBudgetById(int budgetId)
        {
            return _applicationData.Budgets.FirstOrDefault(b => b.Id == budgetId);
        }

        public IEnumerable<Budget> GetBudgetsByUserId(int userId)
        {
            return _applicationData.Budgets.Where(b => b.UserId == userId);
        }

        public IEnumerable<Category> GetCategories()
        {
            return _applicationData.Categories;
        }

        public Category? GetCategoryById(int categoryId)
        {
            return _applicationData.Categories.FirstOrDefault(c => c.Id == categoryId);
        }

        public IEnumerable<Expense> GetExpenses()
        {
            return _applicationData.Expenses;
        }

        public IEnumerable<Expense> GetExpensesByBudgetId(int budgetId)
        {
            return _applicationData.Expenses.Where(e => e.BudgetId == budgetId);
        }

        public Expense? GetExpenseById(int expenseId)
        {
            return _applicationData.Expenses.FirstOrDefault(e => e.Id == expenseId);
        }

        public IEnumerable<User> GetUsers()
        {
            return _applicationData.Users;
        }

        public User? GetUserById(int userId)
        {
            return _applicationData.Users.FirstOrDefault(u => u.Id == userId);
        }

        public bool CategoryExists(string name)
        {
            return _applicationData.Categories.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public bool UserExists(string username)
        {
            return _applicationData.Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public bool RemoveCategory(int categoryId)
        {
            var categoryToRemove = _applicationData.Categories.FirstOrDefault(c => c.Id == categoryId);
            var hasDependencies = _applicationData.Expenses.Any(e => e.CategoryId == categoryId);

            if (categoryToRemove != null && !hasDependencies)
            {
                _applicationData.Categories.Remove(categoryToRemove);
                UpdateData();

                return true;
            }

            return false;
        }

        public bool RemoveExpense(int expenseId)
        {
            var expenseToRemove = _applicationData.Expenses.FirstOrDefault(e => e.Id == expenseId);

            if (expenseToRemove != null)
            {
                _applicationData.Expenses.Remove(expenseToRemove);
                UpdateData();

                return true;
            }

            return false;
        }

        public bool RemoveUser(int userId)
        {
            var userToRemove = _applicationData.Users.FirstOrDefault(e => e.Id == userId);

            if (userToRemove != null)
            {
                _applicationData.Users.Remove(userToRemove);
                UpdateData();

                return true;
            }

            return false;
        }

        public void RemoveExpensesByBudgetId(int budgetId)
        {
            var expensesToRemove = _applicationData.Expenses.Where(e => e.BudgetId == budgetId).ToList();

            foreach (var expenseId in expensesToRemove.Select(e => e.Id))
            {
                RemoveExpense(expenseId);
            }

            UpdateData();
        }

        public void RemoveBudget(int budgetId)
        {
            var budgetToRemove = _applicationData.Budgets.FirstOrDefault(e => e.Id == budgetId);

            if (budgetToRemove != null)
            {
                _applicationData.Budgets.Remove(budgetToRemove);
                UpdateData();
            }
        }

        public void UpdateCategoryName(int categoryId, string name)
        {
            var category = _applicationData.Categories.First(e => e.Id == categoryId);
            category.Name = name;
            UpdateData();
        }

        public void UpdateNameOfUser(int userId, string name)
        {
            var user = _applicationData.Users.First(u => u.Id == userId);
            user.Name = name;
            UpdateData();
        }

        public int GenerateNextBudgetId()
        {
            return _applicationData.Budgets.Any()
               ? _applicationData.Budgets.Max(b => b.Id) + 1
               : 1;
        }

        public int GenerateNextCategoryId()
        {
            return _applicationData.Categories.Any()
               ? _applicationData.Categories.Max(c => c.Id) + 1
               : 1;
        }

        public int GenerateNextExpenseId()
        {
            return _applicationData.Expenses.Any()
               ? _applicationData.Expenses.Max(e => e.Id) + 1
               : 1;
        }

        public int GenerateNextUserId()
        {
            return _applicationData.Users.Any()
               ? _applicationData.Users.Max(u => u.Id) + 1
               : 1;
        }

        private void UpdateData()
        {
            var jsonData = JsonConvert.SerializeObject(_applicationData);
            File.WriteAllText(_filePath, jsonData);
        }

        private void LoadData()
        {
            if (File.Exists(_filePath))
            {
                var jsonData = File.ReadAllText(_filePath);
                var appData = JsonConvert.DeserializeObject<ApplicationData>(jsonData);

                if (appData != null)
                {
                    _applicationData = appData;

                    foreach (var expense in appData.Expenses)
                    {
                        expense.Category = appData.Categories.First(c => c.Id == expense.CategoryId);
                    }
                }
            }
            else
            {
                Console.WriteLine("Data file not found.");
            }
        }
    }
}