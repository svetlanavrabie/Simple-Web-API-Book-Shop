using System.Collections.Generic;

namespace DemoBookAPI.Services
{
    public interface IAuthorRepository
    {
        ICollection<Author> GetAuthors();

        Author GetAuthor(int authorId);

        ICollection<Author> GetAuthorsOfABook(int bookId);

        ICollection<Book> GetBooksByAuthor(int bookId);

        bool AuthorExists(int authorId);
    }
}
