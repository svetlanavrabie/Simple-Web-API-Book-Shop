using AutoFixture;
using DemoBookAPI.Controllers;
using DemoBookAPI.Dtos;
using DemoBookAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DemoBookAPI.Tests.Controllers
{
    public class ReviewersControllerTests
    {
        private readonly IFixture _fixture = TestFixtureFactory.Create();
        private readonly Mock<IReviewerRepository> _reviewerRepo = new();
        private readonly Mock<IReviewRepository> _reviewRepo = new();
        private readonly ReviewersController _sut;

        public ReviewersControllerTests()
        {
            _sut = new ReviewersController(_reviewerRepo.Object, _reviewRepo.Object);
        }

        private static void AssertStatus(IActionResult result, int status) =>
            Assert.Equal(status, Assert.IsType<ObjectResult>(result).StatusCode);

        [Fact]
        public void GetReviewers_ReturnsOk()
        {
            _reviewerRepo.Setup(r => r.GetReviewers()).Returns(_fixture.CreateMany<Reviewer>(3).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<ReviewerDto>>(Assert.IsType<OkObjectResult>(_sut.GetReviewers()).Value);

            Assert.Equal(3, dtos.Count());
        }

        [Fact]
        public void GetReviewers_InvalidModelState_ReturnsBadRequest()
        {
            _reviewerRepo.Setup(r => r.GetReviewers()).Returns(new List<Reviewer>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetReviewers());
        }

        [Fact]
        public void GetReviewer_NotFound()
        {
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetReviewer(1));
        }

        [Fact]
        public void GetReviewer_ReturnsOk()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(true);
            _reviewerRepo.Setup(r => r.GetReviewer(1)).Returns(reviewer);

            var dto = Assert.IsType<ReviewerDto>(Assert.IsType<OkObjectResult>(_sut.GetReviewer(1)).Value);

            Assert.Equal(reviewer.FirstName, dto.FirstName);
        }

        [Fact]
        public void GetReviewer_InvalidModelState_ReturnsBadRequest()
        {
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(true);
            _reviewerRepo.Setup(r => r.GetReviewer(1)).Returns(_fixture.Create<Reviewer>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetReviewer(1));
        }

        [Fact]
        public void GetReviewsByReviewer_NotFound()
        {
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetReviewsByReviewer(1));
        }

        [Fact]
        public void GetReviewsByReviewer_ReturnsOk()
        {
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(true);
            _reviewerRepo.Setup(r => r.GetReviewsByReviewer(1)).Returns(_fixture.CreateMany<Review>(2).ToList());

            var dtos = Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(Assert.IsType<OkObjectResult>(_sut.GetReviewsByReviewer(1)).Value);

            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public void GetReviewsByReviewer_InvalidModelState_ReturnsBadRequest()
        {
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(true);
            _reviewerRepo.Setup(r => r.GetReviewsByReviewer(1)).Returns(new List<Review>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetReviewsByReviewer(1));
        }

        [Fact]
        public void GetReviewerByOfAReview_NotFound()
        {
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetReviewerByOfAReview(1));
        }

        [Fact]
        public void GetReviewerByOfAReview_ReturnsOk()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(true);
            _reviewerRepo.Setup(r => r.GetReviewerOfAReview(1)).Returns(reviewer);

            var dto = Assert.IsType<ReviewerDto>(Assert.IsType<OkObjectResult>(_sut.GetReviewerByOfAReview(1)).Value);

            Assert.Equal(reviewer.Id, dto.Id);
        }

        [Fact]
        public void GetReviewerByOfAReview_InvalidModelState_ReturnsBadRequest()
        {
            _reviewRepo.Setup(r => r.ReviewExists(1)).Returns(true);
            _reviewerRepo.Setup(r => r.GetReviewerOfAReview(1)).Returns(_fixture.Create<Reviewer>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetReviewerByOfAReview(1));
        }

        [Fact]
        public void CreateReviewer_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestObjectResult>(_sut.CreateReviewer(null));
        }

        [Fact]
        public void CreateReviewer_InvalidModelState_ReturnsBadRequest()
        {
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.CreateReviewer(_fixture.Create<Reviewer>()));
        }

        [Fact]
        public void CreateReviewer_SaveFails_Returns500()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewerRepo.Setup(r => r.CreateReviewer(reviewer)).Returns(false);

            AssertStatus(_sut.CreateReviewer(reviewer), 500);
        }

        [Fact]
        public void CreateReviewer_Success_ReturnsCreatedAtRoute()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewerRepo.Setup(r => r.CreateReviewer(reviewer)).Returns(true);

            Assert.Equal("GetReviewer", Assert.IsType<CreatedAtRouteResult>(_sut.CreateReviewer(reviewer)).RouteName);
        }

        [Fact]
        public void UpdateReviewer_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestObjectResult>(_sut.UpdateReviewer(1, null));
        }

        [Fact]
        public void UpdateReviewer_IdMismatch_ReturnsBadRequest()
        {
            var reviewer = _fixture.Build<Reviewer>().With(r => r.Id, 2).Create();

            Assert.IsType<BadRequestObjectResult>(_sut.UpdateReviewer(1, reviewer));
        }

        [Fact]
        public void UpdateReviewer_NotFound()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewerRepo.Setup(r => r.ReviewerExists(reviewer.Id)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.UpdateReviewer(reviewer.Id, reviewer));
        }

        [Fact]
        public void UpdateReviewer_InvalidModelState_ReturnsBadRequest()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewerRepo.Setup(r => r.ReviewerExists(reviewer.Id)).Returns(true);
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.UpdateReviewer(reviewer.Id, reviewer));
        }

        [Fact]
        public void UpdateReviewer_SaveFails_Returns500()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewerRepo.Setup(r => r.ReviewerExists(reviewer.Id)).Returns(true);
            _reviewerRepo.Setup(r => r.UpdateReviewer(reviewer)).Returns(false);

            AssertStatus(_sut.UpdateReviewer(reviewer.Id, reviewer), 500);
        }

        [Fact]
        public void UpdateReviewer_Success_ReturnsNoContent()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewerRepo.Setup(r => r.ReviewerExists(reviewer.Id)).Returns(true);
            _reviewerRepo.Setup(r => r.UpdateReviewer(reviewer)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.UpdateReviewer(reviewer.Id, reviewer));
        }

        [Fact]
        public void DeleteReviewe_NotFound()
        {
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.DeleteReviewe(1));
        }

        [Fact]
        public void DeleteReviewe_InvalidModelState_ReturnsBadRequest()
        {
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(true);
            _reviewerRepo.Setup(r => r.GetReviewer(1)).Returns(_fixture.Create<Reviewer>());
            _reviewerRepo.Setup(r => r.GetReviewsByReviewer(1)).Returns(new List<Review>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.DeleteReviewe(1));
        }

        [Fact]
        public void DeleteReviewe_DeleteReviewerFails_Returns500()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(true);
            _reviewerRepo.Setup(r => r.GetReviewer(1)).Returns(reviewer);
            _reviewerRepo.Setup(r => r.GetReviewsByReviewer(1)).Returns(new List<Review>());
            _reviewerRepo.Setup(r => r.DeleteReviewer(reviewer)).Returns(false);

            AssertStatus(_sut.DeleteReviewe(1), 500);
        }

        [Fact]
        public void DeleteReviewe_DeleteReviewsFails_Returns500()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(true);
            _reviewerRepo.Setup(r => r.GetReviewer(1)).Returns(reviewer);
            _reviewerRepo.Setup(r => r.GetReviewsByReviewer(1)).Returns(new List<Review>());
            _reviewerRepo.Setup(r => r.DeleteReviewer(reviewer)).Returns(true);
            _reviewRepo.Setup(r => r.DeleteReviews(It.IsAny<List<Review>>())).Returns(false);

            AssertStatus(_sut.DeleteReviewe(1), 500);
        }

        [Fact]
        public void DeleteReviewe_Success_ReturnsNoContent()
        {
            var reviewer = _fixture.Create<Reviewer>();
            _reviewerRepo.Setup(r => r.ReviewerExists(1)).Returns(true);
            _reviewerRepo.Setup(r => r.GetReviewer(1)).Returns(reviewer);
            _reviewerRepo.Setup(r => r.GetReviewsByReviewer(1)).Returns(new List<Review>());
            _reviewerRepo.Setup(r => r.DeleteReviewer(reviewer)).Returns(true);
            _reviewRepo.Setup(r => r.DeleteReviews(It.IsAny<List<Review>>())).Returns(true);

            Assert.IsType<NoContentResult>(_sut.DeleteReviewe(1));
        }
    }
}
