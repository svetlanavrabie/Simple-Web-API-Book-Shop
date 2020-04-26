using System.Collections.Generic;

namespace DemoBookAPI.Services
{
    public interface IBookRepository
    {
        ICollection<Book> GetBooks();

        Book GetBook(int bookId);

        Book GetBook(string bookIsbn);

        decimal GetBookRating(int bookId);

        bool BookExists(int bookId);

        bool BookExists(string bookIsbn);

        bool IsDublicateIsbn(int bookId, string bookIsbn);

        bool CreateBook(List<int> authorsId, List<int> categoriesId, Book book);
       
        bool UpdateBook(List<int> authorsId, List<int> categoriesId, Book book);

        bool DeleteBook(Book book);

        bool Save();
    }
}
