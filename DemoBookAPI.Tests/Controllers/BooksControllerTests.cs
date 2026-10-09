using AutoFixture;
using DemoBookAPI.Controllers;
using DemoBookAPI.Dtos;
using DemoBookAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Moq;

namespace DemoBookAPI.Tests.Controllers
{
    public class BooksControllerTests
    {
        private readonly IFixture _fixture = TestFixtureFactory.Create();
        private readonly Mock<IBookRepository> _bookRepo = new();
        private readonly Mock<IAuthorRepository> _authorRepo = new();
        private readonly Mock<ICategoryRepository> _categoryRepo = new();
        private readonly Mock<IReviewRepository> _reviewRepo = new();
        private readonly BooksController _sut;

        public BooksControllerTests()
        {
            _sut = new BooksController(_bookRepo.Object, _authorRepo.Object, _categoryRepo.Object, _reviewRepo.Object);
        }

        private static void AssertStatus(IActionResult result, int status) =>
            Assert.Equal(status, Assert.IsAssignableFrom<IStatusCodeActionResult>(result).StatusCode);

        private void SetupValidBook(Book book, List<int> authors, List<int> categories)
        {
            _bookRepo.Setup(r => r.IsDublicateIsbn(book.Id, book.Isbn)).Returns(false);
            _authorRepo.Setup(r => r.AuthorExists(It.IsAny<int>())).Returns(true);
            _categoryRepo.Setup(r => r.CategoryExists(It.IsAny<int>())).Returns(true);
        }

        [Fact]
        public void GetAuthors_ReturnsOkWithBooks()
        {
            _bookRepo.Setup(r => r.GetBooks()).Returns(_fixture.CreateMany<Book>(3).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<BookDto>>(Assert.IsType<OkObjectResult>(_sut.GetAuthors()).Value);

            Assert.Equal(3, dtos.Count());
        }

        [Fact]
        public void GetAuthors_InvalidModelState_ReturnsBadRequest()
        {
            _bookRepo.Setup(r => r.GetBooks()).Returns(new List<Book>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetAuthors());
        }

        [Fact]
        public void GetBook_NotFound()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetBook(1));
        }

        [Fact]
        public void GetBook_ReturnsOk()
        {
            var book = _fixture.Create<Book>();
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _bookRepo.Setup(r => r.GetBook(1)).Returns(book);

            var dto = Assert.IsType<BookDto>(Assert.IsType<OkObjectResult>(_sut.GetBook(1)).Value);

            Assert.Equal(book.Isbn, dto.Isbn);
        }

        [Fact]
        public void GetBook_InvalidModelState_ReturnsBadRequest()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _bookRepo.Setup(r => r.GetBook(1)).Returns(_fixture.Create<Book>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetBook(1));
        }

        [Fact]
        public void GetAuthor_ByIsbn_NotFound()
        {
            _bookRepo.Setup(r => r.BookExists("isbn")).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetAuthor("isbn"));
        }

        [Fact]
        public void GetAuthor_ByIsbn_ReturnsOk()
        {
            var book = _fixture.Create<Book>();
            _bookRepo.Setup(r => r.BookExists("isbn")).Returns(true);
            _bookRepo.Setup(r => r.GetBook("isbn")).Returns(book);

            var dto = Assert.IsType<BookDto>(Assert.IsType<OkObjectResult>(_sut.GetAuthor("isbn")).Value);

            Assert.Equal(book.Title, dto.Title);
        }

        [Fact]
        public void GetAuthor_ByIsbn_InvalidModelState_ReturnsBadRequest()
        {
            _bookRepo.Setup(r => r.BookExists("isbn")).Returns(true);
            _bookRepo.Setup(r => r.GetBook("isbn")).Returns(_fixture.Create<Book>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetAuthor("isbn"));
        }

        [Fact]
        public void GetBookRating_NotFound()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetBookRating(1));
        }

        [Fact]
        public void GetBookRating_ReturnsOk()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _bookRepo.Setup(r => r.GetBookRating(1)).Returns(4.5m);

            Assert.Equal(4.5m, Assert.IsType<OkObjectResult>(_sut.GetBookRating(1)).Value);
        }

        [Fact]
        public void GetBookRating_InvalidModelState_ReturnsBadRequest()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetBookRating(1));
        }

        [Fact]
        public void CreateBook_NoAuthors_Returns400()
        {
            AssertStatus(_sut.CreateBook(new List<int>(), new List<int> { 1 }, _fixture.Create<Book>()), 400);
        }

        [Fact]
        public void CreateBook_NoCategories_Returns400()
        {
            AssertStatus(_sut.CreateBook(new List<int> { 1 }, new List<int>(), _fixture.Create<Book>()), 400);
        }

        [Fact]
        public void CreateBook_NullBook_Returns400()
        {
            AssertStatus(_sut.CreateBook(new List<int> { 1 }, new List<int> { 1 }, null), 400);
        }

        [Fact]
        public void CreateBook_DuplicateIsbn_Returns422()
        {
            var book = _fixture.Create<Book>();
            _bookRepo.Setup(r => r.IsDublicateIsbn(book.Id, book.Isbn)).Returns(true);

            AssertStatus(_sut.CreateBook(new List<int> { 1 }, new List<int> { 1 }, book), 422);
        }

        [Fact]
        public void CreateBook_AuthorNotFound_Returns404()
        {
            var book = _fixture.Create<Book>();
            _bookRepo.Setup(r => r.IsDublicateIsbn(book.Id, book.Isbn)).Returns(false);
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(false);

            AssertStatus(_sut.CreateBook(new List<int> { 1 }, new List<int> { 1 }, book), 404);
        }

        [Fact]
        public void CreateBook_CategoryNotFound_Returns404()
        {
            var book = _fixture.Create<Book>();
            _bookRepo.Setup(r => r.IsDublicateIsbn(book.Id, book.Isbn)).Returns(false);
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(false);

            AssertStatus(_sut.CreateBook(new List<int> { 1 }, new List<int> { 1 }, book), 404);
        }

        [Fact]
        public void CreateBook_SaveFails_Returns500()
        {
            var book = _fixture.Create<Book>();
            var authors = new List<int> { 1 };
            var categories = new List<int> { 1 };
            SetupValidBook(book, authors, categories);
            _bookRepo.Setup(r => r.CreateBook(authors, categories, book)).Returns(false);

            AssertStatus(_sut.CreateBook(authors, categories, book), 500);
        }

        [Fact]
        public void CreateBook_Success_ReturnsCreatedAtRoute()
        {
            var book = _fixture.Create<Book>();
            var authors = new List<int> { 1 };
            var categories = new List<int> { 1 };
            SetupValidBook(book, authors, categories);
            _bookRepo.Setup(r => r.CreateBook(authors, categories, book)).Returns(true);

            Assert.Equal("GetBook", Assert.IsType<CreatedAtRouteResult>(_sut.CreateBook(authors, categories, book)).RouteName);
        }

        [Fact]
        public void UpdateBook_IdMismatch_ReturnsBadRequest()
        {
            var book = _fixture.Build<Book>().With(b => b.Id, 2).Create();
            SetupValidBook(book, null, null);

            Assert.IsType<BadRequestResult>(_sut.UpdateBook(1, new List<int> { 1 }, new List<int> { 1 }, book));
        }

        [Fact]
        public void UpdateBook_NotFound()
        {
            var book = _fixture.Create<Book>();
            SetupValidBook(book, null, null);
            _bookRepo.Setup(r => r.BookExists(book.Id)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.UpdateBook(book.Id, new List<int> { 1 }, new List<int> { 1 }, book));
        }

        [Fact]
        public void UpdateBook_InvalidRelations_ReturnsStatusFromValidation()
        {
            var book = _fixture.Create<Book>();
            _bookRepo.Setup(r => r.IsDublicateIsbn(book.Id, book.Isbn)).Returns(true);
            _bookRepo.Setup(r => r.BookExists(book.Id)).Returns(true);

            AssertStatus(_sut.UpdateBook(book.Id, new List<int> { 1 }, new List<int> { 1 }, book), 422);
        }

        [Fact]
        public void UpdateBook_SaveFails_Returns500()
        {
            var book = _fixture.Create<Book>();
            var authors = new List<int> { 1 };
            var categories = new List<int> { 1 };
            SetupValidBook(book, authors, categories);
            _bookRepo.Setup(r => r.BookExists(book.Id)).Returns(true);
            _bookRepo.Setup(r => r.UpdateBook(authors, categories, book)).Returns(false);

            AssertStatus(_sut.UpdateBook(book.Id, authors, categories, book), 500);
        }

        [Fact]
        public void UpdateBook_Success_ReturnsNoContent()
        {
            var book = _fixture.Create<Book>();
            var authors = new List<int> { 1 };
            var categories = new List<int> { 1 };
            SetupValidBook(book, authors, categories);
            _bookRepo.Setup(r => r.BookExists(book.Id)).Returns(true);
            _bookRepo.Setup(r => r.UpdateBook(authors, categories, book)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.UpdateBook(book.Id, authors, categories, book));
        }

        [Fact]
        public void DeleteBook_NotFound()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.DeleteBook(1));
        }

        [Fact]
        public void DeleteBook_InvalidModelState_ReturnsBadRequest()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _bookRepo.Setup(r => r.GetBook(1)).Returns(_fixture.Create<Book>());
            _reviewRepo.Setup(r => r.GetReviewsOfABook(1)).Returns(new List<Review>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.DeleteBook(1));
        }

        [Fact]
        public void DeleteBook_DeleteReviewsFails_Returns500()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _bookRepo.Setup(r => r.GetBook(1)).Returns(_fixture.Create<Book>());
            _reviewRepo.Setup(r => r.GetReviewsOfABook(1)).Returns(new List<Review>());
            _reviewRepo.Setup(r => r.DeleteReviews(It.IsAny<List<Review>>())).Returns(false);

            AssertStatus(_sut.DeleteBook(1), 500);
        }

        [Fact]
        public void DeleteBook_DeleteBookFails_Returns500()
        {
            var book = _fixture.Create<Book>();
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _bookRepo.Setup(r => r.GetBook(1)).Returns(book);
            _reviewRepo.Setup(r => r.GetReviewsOfABook(1)).Returns(new List<Review>());
            _reviewRepo.Setup(r => r.DeleteReviews(It.IsAny<List<Review>>())).Returns(true);
            _bookRepo.Setup(r => r.DeleteBook(book)).Returns(false);

            AssertStatus(_sut.DeleteBook(1), 500);
        }

        [Fact]
        public void DeleteBook_Success_ReturnsNoContent()
        {
            var book = _fixture.Create<Book>();
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _bookRepo.Setup(r => r.GetBook(1)).Returns(book);
            _reviewRepo.Setup(r => r.GetReviewsOfABook(1)).Returns(new List<Review>());
            _reviewRepo.Setup(r => r.DeleteReviews(It.IsAny<List<Review>>())).Returns(true);
            _bookRepo.Setup(r => r.DeleteBook(book)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.DeleteBook(1));
        }
    }
}
