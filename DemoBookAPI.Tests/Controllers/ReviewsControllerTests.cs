using AutoFixture;
using DemoBookAPI.Controllers;
using DemoBookAPI.Dtos;
using DemoBookAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DemoBookAPI.Tests.Controllers
{
    public class ReviewsControllerTests
    {
        private readonly IFixture _fixture = TestFixtureFactory.Create();
        private readonly Mock<IReviewerRepository> _reviewerRepo = new();
        private readonly Mock<IReviewRepository> _reviewRepo = new();
        private readonly Mock<IBookRepository> _bookRepo = new();
        private readonly ReviewsController _sut;

        public ReviewsControllerTests()
        {
            _sut = new ReviewsController(_reviewerRepo.Object, _reviewRepo.Object, _bookRepo.Object);
        }

        private static void AssertStatus(IActionResult result, int status) =>
            Assert.Equal(status, Assert.IsType<ObjectResult>(result).StatusCode);

        private Review SetupRelations(bool reviewerExists = true, bool bookExists = true, bool reviewExists = true)
        {
            var review = _fixture.Create<Review>();
            _reviewerRepo.Setup(r => r.ReviewerExists(review.Reviewer.Id)).Returns(reviewerExists);
            _bookRepo.Setup(r => r.BookExists(review.Book.Id)).Returns(bookExists);
            _reviewRepo.Setup(r => r.ReviewExists(review.Id)).Returns(reviewExists);
            _bookRepo.Setup(r => r.GetBook(review.Book.Id)).Returns(review.Book);
            _reviewerRepo.Setup(r => r.GetReviewer(review.Reviewer.Id)).Returns(review.Reviewer);
            return review;
        }

        [Fact]
        public void GetReviewers_ReturnsOk()
        {
            _reviewRepo.Setup(r => r.GetReviews()).Returns(_fixture.CreateMany<Review>(3).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(Assert.IsType<OkObjectResult>(_sut.GetReviewers()).Value);

            Assert.Equal(3, dtos.Count());
        }

        [Fact]
        public void GetReviewers_InvalidModelState_ReturnsBadRequest()
        {
            _reviewRepo.Setup(r => r.GetReviews()).Returns(new List<Review>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetReviewers());
        }

        [Fact]
        public void GetReview_NotFound()
        {
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetReview(1));
        }

        [Fact]
        public void GetReview_ReturnsOk()
        {
            var review = _fixture.Create<Review>();
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(true);
            _reviewRepo.Setup(r => r.GetReview(1)).Returns(review);

            var dto = Assert.IsType<ReviewDto>(Assert.IsType<OkObjectResult>(_sut.GetReview(1)).Value);

            Assert.Equal(review.Headline, dto.Headline);
        }

        [Fact]
        public void GetReview_InvalidModelState_ReturnsBadRequest()
        {
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(true);
            _reviewRepo.Setup(r => r.GetReview(1)).Returns(_fixture.Create<Review>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetReview(1));
        }

        [Fact]
        public void GetReviewsForABook_NotFound()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetReviewsForABook(1));
        }

        [Fact]
        public void GetReviewsForABook_ReturnsOk()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _reviewRepo.Setup(r => r.GetReviewsOfABook(1)).Returns(_fixture.CreateMany<Review>(2).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(Assert.IsType<OkObjectResult>(_sut.GetReviewsForABook(1)).Value);

            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public void GetReviewsForABook_InvalidModelState_ReturnsBadRequest()
        {
            _bookRepo.Setup(r => r.BookExists(1)).Returns(true);
            _reviewRepo.Setup(r => r.GetReviewsOfABook(1)).Returns(new List<Review>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetReviewsForABook(1));
        }

        [Fact]
        public void GetBookOfAReview_NotFound()
        {
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetBookOfAReview(1));
        }

        [Fact]
        public void GetBookOfAReview_ReturnsOk()
        {
            var book = _fixture.Create<Book>();
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(true);
            _reviewRepo.Setup(r => r.GetBookOfAReview(1)).Returns(book);

            var dto = Assert.IsType<BookDto>(Assert.IsType<OkObjectResult>(_sut.GetBookOfAReview(1)).Value);

            Assert.Equal(book.Id, dto.Id);
        }

        [Fact]
        public void GetBookOfAReview_InvalidModelState_ReturnsBadRequest()
        {
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(true);
            _reviewRepo.Setup(r => r.GetBookOfAReview(1)).Returns(_fixture.Create<Book>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetBookOfAReview(1));
        }

        [Fact]
        public void CreateReview_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestObjectResult>(_sut.CreateReview(null));
        }

        [Fact]
        public void CreateReview_ReviewerNotFound_Returns404()
        {
            var review = SetupRelations(reviewerExists: false);

            AssertStatus(_sut.CreateReview(review), 404);
        }

        [Fact]
        public void CreateReview_BookNotFound_Returns404()
        {
            var review = SetupRelations(bookExists: false);

            AssertStatus(_sut.CreateReview(review), 404);
        }

        [Fact]
        public void CreateReview_SaveFails_Returns500()
        {
            var review = SetupRelations();
            _reviewRepo.Setup(r => r.CreateReview(review)).Returns(false);

            AssertStatus(_sut.CreateReview(review), 500);
        }

        [Fact]
        public void CreateReview_Success_ReturnsCreatedAtRoute()
        {
            var review = SetupRelations();
            _reviewRepo.Setup(r => r.CreateReview(review)).Returns(true);

            Assert.Equal("GetReview", Assert.IsType<CreatedAtRouteResult>(_sut.CreateReview(review)).RouteName);
        }

        [Fact]
        public void UpdateReview_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestObjectResult>(_sut.UpdateReview(1, null));
        }

        [Fact]
        public void UpdateReview_IdMismatch_ReturnsBadRequest()
        {
            var review = _fixture.Build<Review>().With(r => r.Id, 2).Create();

            Assert.IsType<BadRequestObjectResult>(_sut.UpdateReview(1, review));
        }

        [Fact]
        public void UpdateReview_ReviewNotFound_Returns404()
        {
            var review = SetupRelations(reviewExists: false);

            AssertStatus(_sut.UpdateReview(review.Id, review), 404);
        }

        [Fact]
        public void UpdateReview_ReviewerNotFound_Returns404()
        {
            var review = SetupRelations(reviewerExists: false);

            AssertStatus(_sut.UpdateReview(review.Id, review), 404);
        }

        [Fact]
        public void UpdateReview_BookNotFound_Returns404()
        {
            var review = SetupRelations(bookExists: false);

            AssertStatus(_sut.UpdateReview(review.Id, review), 404);
        }

        [Fact]
        public void UpdateReview_SaveFails_Returns500()
        {
            var review = SetupRelations();
            _reviewRepo.Setup(r => r.UpdateReview(review)).Returns(false);

            AssertStatus(_sut.UpdateReview(review.Id, review), 500);
        }

        [Fact]
        public void UpdateReview_Success_ReturnsNoContent()
        {
            var review = SetupRelations();
            _reviewRepo.Setup(r => r.UpdateReview(review)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.UpdateReview(review.Id, review));
        }

        [Fact]
        public void DeleteReviews_NotFound()
        {
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.DeleteReviews(1));
        }

        [Fact]
        public void DeleteReviews_InvalidModelState_ReturnsBadRequest()
        {
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(true);
            _reviewRepo.Setup(r => r.GetReview(1)).Returns(_fixture.Create<Review>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.DeleteReviews(1));
        }

        [Fact]
        public void DeleteReviews_DeleteFails_Returns500()
        {
            var review = _fixture.Create<Review>();
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(true);
            _reviewRepo.Setup(r => r.GetReview(1)).Returns(review);
            _reviewRepo.Setup(r => r.DeleteReview(review)).Returns(false);

            AssertStatus(_sut.DeleteReviews(1), 500);
        }

        [Fact]
        public void DeleteReviews_Success_ReturnsNoContent()
        {
            var review = _fixture.Create<Review>();
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(true);
            _reviewRepo.Setup(r => r.GetReview(1)).Returns(review);
            _reviewRepo.Setup(r => r.DeleteReview(review)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.DeleteReviews(1));
        }
    }
}
