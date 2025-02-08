using BudgetTracker.Models;

namespace BudgetTracker.Services.Interfaces
{
    public interface ICategoryService
    {
        Response<Category> CreateCategory(string name);

        bool DeleteCategory();

        IList<string> GetCategoriesAsTable();

        Category? GetCategoryById(int categoryId);

        bool HasSelectedCategory();

        void SetSelectedCategoryById(int categoryId);

        Response<Category> UpdateCategory(string name);
    }
}