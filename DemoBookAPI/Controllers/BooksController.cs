using DemoBookAPI.Dtos;
using DemoBookAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace DemoBookAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : Controller
    {
       
        private IBookRepository _bookRepository;

        private IAuthorRepository _authorRepository;

        public BooksController(IBookRepository bookRepository, IAuthorRepository authorRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
        }

        //api/books

        [HttpGet]
        [ProducesResponseType(400)]
        [ProducesResponseType(200, Type = typeof(IEnumerable<BookDto>))]
        public IActionResult GetAuthors()
        {
            var books = _bookRepository.GetBooks().ToList();
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

        //api/books/bookId
        [HttpGet("{bookId}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(BookDto))]
        public IActionResult GetAuthor(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
            {
                return NotFound();
            }

            var book= _bookRepository.GetBook(bookId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDto = new BookDto()
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                DatePublished = book.DatePublished
            };

            return Ok(bookDto);
        }

        //api/books/isbn/bookIsbn
        [HttpGet("ISBN/{bookIsbn}")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(BookDto))]
        public IActionResult GetAuthor(string bookIsbn)
        {
            if (!_bookRepository.BookExists(bookIsbn))
            {
                return NotFound();
            }

            var book = _bookRepository.GetBook(bookIsbn);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookDto = new BookDto()
            {
                Id = book.Id,
                Title = book.Title,
                Isbn = book.Isbn,
                DatePublished = book.DatePublished
            };

            return Ok(bookDto);
        }

        //api/books/bookId/rating
        [HttpGet("{bookId}/rating")]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(200, Type = typeof(decimal))]
        public IActionResult GetBookRating(int bookId)
        {
            if (!_bookRepository.BookExists(bookId))
            {
                return NotFound();
            }

            var rating = _bookRepository.GetBookRating(bookId);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(rating);
        }

    }
}