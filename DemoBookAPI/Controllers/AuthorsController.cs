using DemoBookAPI.Dtos;
using DemoBookAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace DemoBookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : Controller
    {
        private IAuthorRepository _authorRepository;
        private IBookRepository _bookRepository;

        public AuthorsController(IAuthorRepository authorRepository, IBookRepository bookRepository)
        {
            _authorRepository = authorRepository;
            _bookRepository = bookRepository;
        }

        //api/authors

        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
        public IActionResult GetAuthors()
        {
            var authors = _authorRepository.GetAuthors().ToList();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorsDto = new List<AuthorDto>();

            foreach (var author in authors)
            {
                authorsDto.Add(new AuthorDto
                {
                    Id = author.Id,
                    FirstName = author.FirstName,
                    LastName = author.LastName
                });

            }
            return Ok(authorsDto);
        }

        //api/authors/authorId
        [HttpGet("{authorId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(AuthorDto))]
        public IActionResult GetAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var author = _authorRepository.GetAuthor(authorId);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorDto = new AuthorDto()
            {
                Id = author.Id,
                FirstName = author.FirstName,
                LastName = author.LastName
            };

            return Ok(authorDto);
        }


        //api/authors/authorId/books

        [HttpGet("{authorId}/books")]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<BookDto>))]
        public IActionResult GetBooksbyAuthor(int authorId)
        {
            if (!_authorRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var books = _authorRepository.GetBooksByAuthor(authorId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var booksDto = new List<BookDto>();

            foreach (var book in books)
            {
                booksDto.Add(new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Isbn = book.Isbn,
                    DatePublished = book.DatePublished
                });

            }
            return Ok(booksDto);
        }

        //api/authors/books/bookId/

        [HttpGet("books/{bookId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
        public IActionResult GetAuthorsByABook(int bookId)
        {

            if (!_bookRepository.BookExists(bookId))
            {
                return NotFound();
            }
            var authors = _authorRepository.GetAuthorsOfABook(bookId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var authorsDto = new List<AuthorDto>();

            foreach (var author in authors)
            {
                authorsDto.Add(new AuthorDto
                {
                    Id = author.Id,
                    FirstName = author.FirstName,
                    LastName = author.LastName
                });

            }
            return Ok(authorsDto);
        }

    }
}