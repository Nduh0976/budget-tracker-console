using BudgetTracker.Data.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services.Interfaces;

namespace BudgetTracker.Services
{
    public class CategoryService : ICategoryService
    {
        private Category selectedCategory { get; set; }

        private readonly IDataStore _dataStore;

        public CategoryService(IDataStore dataStore)
        {
            selectedCategory = new Category() { Name = string.Empty };
            _dataStore = dataStore;
        }

        public Response<Category> CreateCategory(string name)
        {
            if (string.IsNullOrWhiteSpace(name?.Trim()))
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

            var newCategory = new Category
            {
                Id = _dataStore.GenerateNextCategoryId(),
                Name = name
            };

            _dataStore.AddCategory(newCategory);

            return new Response<Category>
            {
                Success = true,
                Message = $"'{name}' Category has been successfully created.",
                Data = newCategory
            };
        }

        public bool DeleteCategory()
        {
            return _dataStore.RemoveCategory(selectedCategory.Id);
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

        public Category? GetCategoryById(int categoryId)
        {
            return _dataStore.GetCategoryById(categoryId);
        }

        public bool HasSelectedCategory()
        {
            return selectedCategory != null;
        }

        public void SetSelectedCategoryById(int categoryId)
        {
            selectedCategory = _dataStore.GetCategoryById(categoryId)!;
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

            var category = _dataStore.GetCategoryById(selectedCategory.Id);
           
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
                _dataStore.UpdateCategoryName(category.Id, name);

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
    }
}