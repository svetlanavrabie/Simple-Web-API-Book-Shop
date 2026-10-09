using AutoFixture;
using DemoBookAPI.Controllers;
using DemoBookAPI.Dtos;
using DemoBookAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DemoBookAPI.Tests.Controllers
{
    public class AuthorsControllerTests
    {
        private readonly IFixture _fixture = TestFixtureFactory.Create();
        private readonly Mock<IAuthorRepository> _authorRepo = new();
        private readonly Mock<IBookRepository> _bookRepo = new();
        private readonly Mock<ICountryRepository> _countryRepo = new();
        private readonly AuthorsController _sut;

        public AuthorsControllerTests()
        {
            _sut = new AuthorsController(_authorRepo.Object, _bookRepo.Object, _countryRepo.Object);
        }

        private static void AssertStatus(IActionResult result, int status) =>
            Assert.Equal(status, Assert.IsType<ObjectResult>(result).StatusCode);

        [Fact]
        public void GetAuthors_ReturnsOk()
        {
            _authorRepo.Setup(r => r.GetAuthors()).Returns(_fixture.CreateMany<Author>(3).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<AuthorDto>>(Assert.IsType<OkObjectResult>(_sut.GetAuthors()).Value);

            Assert.Equal(3, dtos.Count());
        }

        [Fact]
        public void GetAuthors_InvalidModelState_ReturnsBadRequest()
        {
            _authorRepo.Setup(r => r.GetAuthors()).Returns(new List<Author>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetAuthors());
        }

        [Fact]
        public void GetAuthor_NotFound()
        {
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetAuthor(1));
        }

        [Fact]
        public void GetAuthor_ReturnsOk()
        {
            var author = _fixture.Create<Author>();
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(true);
            _authorRepo.Setup(r => r.GetAuthor(1)).Returns(author);

            var dto = Assert.IsType<AuthorDto>(Assert.IsType<OkObjectResult>(_sut.GetAuthor(1)).Value);

            Assert.Equal(author.LastName, dto.LastName);
        }

        [Fact]
        public void GetAuthor_InvalidModelState_ReturnsBadRequest()
        {
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(true);
            _authorRepo.Setup(r => r.GetAuthor(1)).Returns(_fixture.Create<Author>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetAuthor(1));
        }

        [Fact]
        public void GetBooksbyAuthor_NotFound()
        {
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetBooksbyAuthor(1));
        }

        [Fact]
        public void GetBooksbyAuthor_ReturnsOk()
        {
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(true);
            _authorRepo.Setup(r => r.GetBooksByAuthor(1)).Returns(_fixture.CreateMany<Book>(2).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<BookDto>>(Assert.IsType<OkObjectResult>(_sut.GetBooksbyAuthor(1)).Value);

            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public void GetBooksbyAuthor_InvalidModelState_ReturnsBadRequest()
        {
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(true);
            _authorRepo.Setup(r => r.GetBooksByAuthor(1)).Returns(new List<Book>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetBooksbyAuthor(1));
        }

        [Fact]
        public void GetAuthorsByABook_NotFound()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetAuthorsByABook(1));
        }

        [Fact]
        public void GetAuthorsByABook_ReturnsOk()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _authorRepo.Setup(r => r.GetAuthorsOfABook(1)).Returns(_fixture.CreateMany<Author>(2).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<AuthorDto>>(Assert.IsType<OkObjectResult>(_sut.GetAuthorsByABook(1)).Value);

            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public void GetAuthorsByABook_InvalidModelState_ReturnsBadRequest()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _authorRepo.Setup(r => r.GetAuthorsOfABook(1)).Returns(new List<Author>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetAuthorsByABook(1));
        }

        [Fact]
        public void CreateAuthor_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestObjectResult>(_sut.CreateAuthor(null));
        }

        [Fact]
        public void CreateAuthor_CountryNotFound_Returns404()
        {
            var author = _fixture.Create<Author>();
            _countryRepo.Setup(r => r.CountryExists(author.Country.Id)).Returns(false);

            AssertStatus(_sut.CreateAuthor(author), 404);
        }

        [Fact]
        public void CreateAuthor_InvalidModelState_ReturnsBadRequest()
        {
            var author = _fixture.Create<Author>();
            _countryRepo.Setup(r => r.CountryExists(author.Country.Id)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(author.Country.Id)).Returns(author.Country);
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.CreateAuthor(author));
        }

        [Fact]
        public void CreateAuthor_SaveFails_Returns500()
        {
            var author = _fixture.Create<Author>();
            _countryRepo.Setup(r => r.CountryExists(author.Country.Id)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(author.Country.Id)).Returns(author.Country);
            _authorRepo.Setup(r => r.CreateAuthor(author)).Returns(false);

            AssertStatus(_sut.CreateAuthor(author), 500);
        }

        [Fact]
        public void CreateAuthor_Success_ReturnsCreatedAtRoute()
        {
            var author = _fixture.Create<Author>();
            _countryRepo.Setup(r => r.CountryExists(author.Country.Id)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(author.Country.Id)).Returns(author.Country);
            _authorRepo.Setup(r => r.CreateAuthor(author)).Returns(true);

            Assert.Equal("GetAuthor", Assert.IsType<CreatedAtRouteResult>(_sut.CreateAuthor(author)).RouteName);
        }

        [Fact]
        public void UpdateAuthor_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestObjectResult>(_sut.UpdateAuthor(1, null));
        }

        [Fact]
        public void UpdateAuthor_IdMismatch_ReturnsBadRequest()
        {
            var author = _fixture.Build<Author>().With(a => a.Id, 2).Create();

            Assert.IsType<BadRequestObjectResult>(_sut.UpdateAuthor(1, author));
        }

        [Fact]
        public void UpdateAuthor_AuthorNotFound_Returns404()
        {
            var author = _fixture.Create<Author>();
            _authorRepo.Setup(r => r.AuthorExists(author.Id)).Returns(false);
            _countryRepo.Setup(r => r.CountryExists(author.Country.Id)).Returns(true);

            AssertStatus(_sut.UpdateAuthor(author.Id, author), 404);
        }

        [Fact]
        public void UpdateAuthor_CountryNotFound_Returns404()
        {
            var author = _fixture.Create<Author>();
            _authorRepo.Setup(r => r.AuthorExists(author.Id)).Returns(true);
            _countryRepo.Setup(r => r.CountryExists(author.Country.Id)).Returns(false);

            AssertStatus(_sut.UpdateAuthor(author.Id, author), 404);
        }

        [Fact]
        public void UpdateAuthor_SaveFails_Returns500()
        {
            var author = _fixture.Create<Author>();
            _authorRepo.Setup(r => r.AuthorExists(author.Id)).Returns(true);
            _countryRepo.Setup(r => r.CountryExists(author.Country.Id)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(author.Country.Id)).Returns(author.Country);
            _authorRepo.Setup(r => r.UpdateAuthor(author)).Returns(false);

            AssertStatus(_sut.UpdateAuthor(author.Id, author), 500);
        }

        [Fact]
        public void UpdateAuthor_Success_ReturnsNoContent()
        {
            var author = _fixture.Create<Author>();
            _authorRepo.Setup(r => r.AuthorExists(author.Id)).Returns(true);
            _countryRepo.Setup(r => r.CountryExists(author.Country.Id)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(author.Country.Id)).Returns(author.Country);
            _authorRepo.Setup(r => r.UpdateAuthor(author)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.UpdateAuthor(author.Id, author));
        }

        [Fact]
        public void DeleteAuthor_NotFound()
        {
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.DeleteAuthor(1));
        }

        [Fact]
        public void DeleteAuthor_HasBooks_Returns409()
        {
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(true);
            _authorRepo.Setup(r => r.GetAuthor(1)).Returns(_fixture.Create<Author>());
            _authorRepo.Setup(r => r.GetBooksByAuthor(1)).Returns(_fixture.CreateMany<Book>(1).ToList());

            AssertStatus(_sut.DeleteAuthor(1), 409);
        }

        [Fact]
        public void DeleteAuthor_InvalidModelState_ReturnsBadRequest()
        {
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(true);
            _authorRepo.Setup(r => r.GetAuthor(1)).Returns(_fixture.Create<Author>());
            _authorRepo.Setup(r => r.GetBooksByAuthor(1)).Returns(new List<Book>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.DeleteAuthor(1));
        }

        [Fact]
        public void DeleteAuthor_DeleteFails_Returns500()
        {
            var author = _fixture.Create<Author>();
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(true);
            _authorRepo.Setup(r => r.GetAuthor(1)).Returns(author);
            _authorRepo.Setup(r => r.GetBooksByAuthor(1)).Returns(new List<Book>());
            _authorRepo.Setup(r => r.DeleteAuthor(author)).Returns(false);

            AssertStatus(_sut.DeleteAuthor(1), 500);
        }

        [Fact]
        public void DeleteAuthor_Success_ReturnsNoContent()
        {
            var author = _fixture.Create<Author>();
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(true);
            _authorRepo.Setup(r => r.GetAuthor(1)).Returns(author);
            _authorRepo.Setup(r => r.GetBooksByAuthor(1)).Returns(new List<Book>());
            _authorRepo.Setup(r => r.DeleteAuthor(author)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.DeleteAuthor(1));
        }
    }
}
