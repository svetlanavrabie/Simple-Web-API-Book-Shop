using AutoFixture;
using DemoBookAPI.Controllers;
using DemoBookAPI.Dtos;
using DemoBookAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DemoBookAPI.Tests.Controllers
{
    public class CategoriesControllerTests
    {
        private readonly IFixture _fixture = TestFixtureFactory.Create();
        private readonly Mock<ICategoryRepository> _categoryRepo = new();
        private readonly Mock<IBookRepository> _bookRepo = new();
        private readonly CategoriesController _sut;

        public CategoriesControllerTests()
        {
            _sut = new CategoriesController(_categoryRepo.Object, _bookRepo.Object);
        }

        private static void AssertStatus(IActionResult result, int status) =>
            Assert.Equal(status, Assert.IsType<ObjectResult>(result).StatusCode);

        [Fact]
        public void GetCategories_ReturnsOk()
        {
            _categoryRepo.Setup(r => r.GetCategories()).Returns(_fixture.CreateMany<Category>(3).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(Assert.IsType<OkObjectResult>(_sut.GetCategories()).Value);

            Assert.Equal(3, dtos.Count());
        }

        [Fact]
        public void GetCategories_InvalidModelState_ReturnsBadRequest()
        {
            _categoryRepo.Setup(r => r.GetCategories()).Returns(new List<Category>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetCategories());
        }

        [Fact]
        public void GetCategory_NotFound()
        {
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetCategory(1));
        }

        [Fact]
        public void GetCategory_ReturnsOk()
        {
            var category = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.GetCategory(1)).Returns(category);

            var dto = Assert.IsType<CategoryDto>(Assert.IsType<OkObjectResult>(_sut.GetCategory(1)).Value);

            Assert.Equal(category.Name, dto.Name);
        }

        [Fact]
        public void GetCategory_InvalidModelState_ReturnsBadRequest()
        {
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.GetCategory(1)).Returns(_fixture.Create<Category>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetCategory(1));
        }

        [Fact]
        public void GetAllCategoriesForABook_NotFound()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetAllCategoriesForABook(1));
        }

        [Fact]
        public void GetAllCategoriesForABook_ReturnsOk()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.GetAllCategoriesForABook(1)).Returns(_fixture.CreateMany<Category>(2).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(Assert.IsType<OkObjectResult>(_sut.GetAllCategoriesForABook(1)).Value);

            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public void GetAllCategoriesForABook_InvalidModelState_ReturnsBadRequest()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.GetAllCategoriesForABook(1)).Returns(new List<Category>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetAllCategoriesForABook(1));
        }

        [Fact]
        public void GetAllBooksForCategory_NotFound()
        {
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetAllBooksForCategory(1));
        }

        [Fact]
        public void GetAllBooksForCategory_ReturnsOk()
        {
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.GetBookForCategory(1)).Returns(_fixture.CreateMany<Book>(2).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<BookDto>>(Assert.IsType<OkObjectResult>(_sut.GetAllBooksForCategory(1)).Value);

            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public void GetAllBooksForCategory_InvalidModelState_ReturnsBadRequest()
        {
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.GetBookForCategory(1)).Returns(new List<Book>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetAllBooksForCategory(1));
        }

        [Fact]
        public void CreateCategory_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestObjectResult>(_sut.CreateCategory(null));
        }

        [Fact]
        public void CreateCategory_Duplicate_Returns422()
        {
            var existing = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.GetCategories()).Returns(new List<Category> { existing });

            AssertStatus(_sut.CreateCategory(new Category { Name = existing.Name.ToLower() }), 422);
        }

        [Fact]
        public void CreateCategory_InvalidModelState_ReturnsBadRequest()
        {
            _categoryRepo.Setup(r => r.GetCategories()).Returns(new List<Category>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.CreateCategory(_fixture.Create<Category>()));
        }

        [Fact]
        public void CreateCategory_SaveFails_Returns500()
        {
            var category = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.GetCategories()).Returns(new List<Category>());
            _categoryRepo.Setup(r => r.CreateCategory(category)).Returns(false);

            AssertStatus(_sut.CreateCategory(category), 500);
        }

        [Fact]
        public void CreateCategory_Success_ReturnsCreatedAtRoute()
        {
            var category = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.GetCategories()).Returns(new List<Category>());
            _categoryRepo.Setup(r => r.CreateCategory(category)).Returns(true);

            Assert.Equal("GetCategory", Assert.IsType<CreatedAtRouteResult>(_sut.CreateCategory(category)).RouteName);
        }

        [Fact]
        public void UpdateCategroy_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestObjectResult>(_sut.UpdateCategroy(1, null));
        }

        [Fact]
        public void UpdateCategroy_IdMismatch_ReturnsBadRequest()
        {
            var category = _fixture.Build<Category>().With(c => c.Id, 2).Create();

            Assert.IsType<BadRequestObjectResult>(_sut.UpdateCategroy(1, category));
        }

        [Fact]
        public void UpdateCategroy_NotFound()
        {
            var category = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.CategoryExists(category.Id)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.UpdateCategroy(category.Id, category));
        }

        [Fact]
        public void UpdateCategroy_Duplicate_Returns422()
        {
            var category = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.CategoryExists(category.Id)).Returns(true);
            _categoryRepo.Setup(r => r.IsDublicateCategoryName(category.Id, category.Name)).Returns(true);

            AssertStatus(_sut.UpdateCategroy(category.Id, category), 422);
        }

        [Fact]
        public void UpdateCategroy_InvalidModelState_ReturnsBadRequest()
        {
            var category = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.CategoryExists(category.Id)).Returns(true);
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.UpdateCategroy(category.Id, category));
        }

        [Fact]
        public void UpdateCategroy_SaveFails_Returns500()
        {
            var category = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.CategoryExists(category.Id)).Returns(true);
            _categoryRepo.Setup(r => r.UpdateCategory(category)).Returns(false);

            AssertStatus(_sut.UpdateCategroy(category.Id, category), 500);
        }

        [Fact]
        public void UpdateCategroy_Success_ReturnsNoContent()
        {
            var category = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.CategoryExists(category.Id)).Returns(true);
            _categoryRepo.Setup(r => r.UpdateCategory(category)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.UpdateCategroy(category.Id, category));
        }

        [Fact]
        public void DeleteCategories_NotFound()
        {
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.DeleteCategories(1));
        }

        [Fact]
        public void DeleteCategories_UsedByBooks_Returns409()
        {
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.GetCategory(1)).Returns(_fixture.Create<Category>());
            _categoryRepo.Setup(r => r.GetBookForCategory(1)).Returns(_fixture.CreateMany<Book>(1).ToList());

            AssertStatus(_sut.DeleteCategories(1), 409);
        }

        [Fact]
        public void DeleteCategories_InvalidModelState_ReturnsBadRequest()
        {
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.GetCategory(1)).Returns(_fixture.Create<Category>());
            _categoryRepo.Setup(r => r.GetBookForCategory(1)).Returns(new List<Book>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.DeleteCategories(1));
        }

        [Fact]
        public void DeleteCategories_DeleteFails_Returns500()
        {
            var category = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.GetCategory(1)).Returns(category);
            _categoryRepo.Setup(r => r.GetBookForCategory(1)).Returns(new List<Book>());
            _categoryRepo.Setup(r => r.DeleteCategory(category)).Returns(false);

            AssertStatus(_sut.DeleteCategories(1), 500);
        }

        [Fact]
        public void DeleteCategories_Success_ReturnsNoContent()
        {
            var category = _fixture.Create<Category>();
            _categoryRepo.Setup(r => r.CategoryExists(1)).Returns(true);
            _categoryRepo.Setup(r => r.GetCategory(1)).Returns(category);
            _categoryRepo.Setup(r => r.GetBookForCategory(1)).Returns(new List<Book>());
            _categoryRepo.Setup(r => r.DeleteCategory(category)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.DeleteCategories(1));
        }
    }
}
