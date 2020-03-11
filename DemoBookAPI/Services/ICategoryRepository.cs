using System.Collections.Generic;

namespace DemoBookAPI.Services
{
    public interface ICategoryRepository
    {
        ICollection<Category> GetCategories();

        Category GetCategory(int categoryId);

        ICollection<Category> GetAllCategoriesForABook(int bookId);

        ICollection<Book> GetBookForCategory(int categoryId);

        bool CategoryExists(int categoryId);

        bool IsDublicateCategoryName(int categoryId, string categoryName);
    }
}
