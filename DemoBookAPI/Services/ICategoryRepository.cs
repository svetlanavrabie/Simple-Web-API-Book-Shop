using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoBookAPI.Services
{
    public interface ICategoryRepository
    {
        ICollection<Category> GetCategories();

        Category GetCategory(int categoryId);

        ICollection<Category> GetAllCategoriesForABook(int bookId);

        ICollection<Book> GetBookForCategory(int categoryId);

        bool CategoryExists(int categoryId);


    }
}
