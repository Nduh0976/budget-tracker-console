using BudgetTracker.Data;
using BudgetTracker.Models;

namespace BudgetTracker.Services
{
    public class CategoryService
    {
        public Category SelectedCategory { get; set; }

        private readonly DataStore _dataStore;

        public CategoryService()
        {
            SelectedCategory = new Category() { Name = string.Empty };

            _dataStore = new DataStore();
        }

        public Response<Category> CreateCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name.Trim()))
            {
                return new Response<Category>
                {
                    Success = false,
                    Message = "Name cannot be empty or whitespace.",
                };
            }

            if (!IsCategoryValid(name))
            {
                return new Response<Category>
                {
                    Success = false,
                    Message = $"A category with the name '{name}' already exists.",
                };
            }

            var categories = _dataStore.GetCategories();

            var newId = categories.Any()
                ? categories.Max(b => b.Id) + 1
                : 1;

            var newCategory = new Category
            {
                Id = newId,
                Name = name
            };

            _dataStore.AddCategory(newCategory);
            _dataStore.UpdateData();

            return new Response<Category>
            {
                Success = true,
                Message = $"'{name}' Category has been successfully created.",
                Data = newCategory
            };
        }

        public bool DeleteCategory()
        {
            return _dataStore.RemoveCategory(SelectedCategory.Id);
        }

        public IList<string> GetCategoriesAsTable()
        {
            var categories = new List<string>();

            foreach (var category in _dataStore.GetCategories())
            {
                categories.Add(category.ToString());
            }

            return categories;
        }

        public void SetSelectedCategoryById(int categoryId)
        {
            SelectedCategory = _dataStore.GetCategoryById(categoryId)!;
        }

        public Response<Category> UpdateCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name.Trim()))
            {
                return new Response<Category>
                {
                    Success = false,
                    Message = "Name cannot be empty or whitespace.",
                };
            }

            var category = _dataStore.GetCategoryById(SelectedCategory.Id);
           
            if (!IsCategoryValid(name) && !name.Equals(category?.Name ?? string.Empty, StringComparison.InvariantCultureIgnoreCase))
            {
                return new Response<Category>
                {
                    Success = false,
                    Message = $"A category with the name '{name}' already exists.",
                };
            }


            if (category != null)
            {
                category.Name = name;
                _dataStore.UpdateData();

                return new Response<Category>
                {
                    Success = true,
                    Message = $"'{category.Name}' Category has been successfully updated.",
                    Data = category
                };
            }

            return new Response<Category>
            {
                Success = false,
                Message = "Category not found.",
            };
        }

        private bool IsCategoryValid(string name)
        {
            return !_dataStore.CategoryExists(name);
        }

        public Category? GetCategoryById(int categoryId)
        {
            return _dataStore.GetCategoryById(categoryId);
        }
    }
}
