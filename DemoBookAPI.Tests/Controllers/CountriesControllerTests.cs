using AutoFixture;
using DemoBookAPI.Controllers;
using DemoBookAPI.Dtos;
using DemoBookAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DemoBookAPI.Tests.Controllers
{
    public class CountriesControllerTests
    {
        private readonly IFixture _fixture = TestFixtureFactory.Create();
        private readonly Mock<ICountryRepository> _countryRepo = new();
        private readonly Mock<IAuthorRepository> _authorRepo = new();
        private readonly CountriesController _sut;

        public CountriesControllerTests()
        {
            _sut = new CountriesController(_countryRepo.Object, _authorRepo.Object);
        }

        [Fact]
        public void GetCountries_ReturnsOkWithDtos()
        {
            var countries = _fixture.CreateMany<Country>(3).ToList();
            _countryRepo.Setup(r => r.GetCountries()).Returns(countries);

            var result = _sut.GetCountries();

            var dtos = Assert.IsAssignableFrom<IEnumerable<CountryDto>>(Assert.IsType<OkObjectResult>(result).Value);
            Assert.Equal(countries.Select(c => c.Id), dtos.Select(d => d.Id));
        }

        [Fact]
        public void GetCountries_InvalidModelState_ReturnsBadRequest()
        {
            _countryRepo.Setup(r => r.GetCountries()).Returns(new List<Country>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetCountries());
        }

        [Fact]
        public void GetCountry_NotExisting_ReturnsNotFound()
        {
            _countryRepo.Setup(r => r.CountryExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetCountry(1));
        }

        [Fact]
        public void GetCountry_Existing_ReturnsOk()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.CountryExists(country.Id)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(country.Id)).Returns(country);

            var dto = Assert.IsType<CountryDto>(Assert.IsType<OkObjectResult>(_sut.GetCountry(country.Id)).Value);

            Assert.Equal(country.Name, dto.Name);
        }

        [Fact]
        public void GetCountry_InvalidModelState_ReturnsBadRequest()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.CountryExists(country.Id)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(country.Id)).Returns(country);
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetCountry(country.Id));
        }

        [Fact]
        public void GetCountryOfAnAuthor_AuthorNotFound_ReturnsNotFound()
        {
            _authorRepo.Setup(r => r.AuthorExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetCountryOfAnAuthor(1));
        }

        [Fact]
        public void GetCountryOfAnAuthor_Existing_ReturnsOk()
        {
            var country = _fixture.Create<Country>();
            _authorRepo.Setup(r => r.AuthorExists(5)).Returns(true);
            _countryRepo.Setup(r => r.GetCountryofAnAuthor(5)).Returns(country);

            var dto = Assert.IsType<CountryDto>(Assert.IsType<OkObjectResult>(_sut.GetCountryOfAnAuthor(5)).Value);

            Assert.Equal(country.Id, dto.Id);
        }

        [Fact]
        public void GetCountryOfAnAuthor_InvalidModelState_ReturnsBadRequest()
        {
            _authorRepo.Setup(r => r.AuthorExists(5)).Returns(true);
            _countryRepo.Setup(r => r.GetCountryofAnAuthor(5)).Returns(_fixture.Create<Country>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetCountryOfAnAuthor(5));
        }

        [Fact]
        public void GetAuthorsFromACounty_NotFound_ReturnsNotFound()
        {
            _countryRepo.Setup(r => r.CountryExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.GetAuthorsFromACounty(1));
        }

        [Fact]
        public void GetAuthorsFromACounty_Existing_ReturnsOk()
        {
            var authors = _fixture.CreateMany<Author>(2).ToList();
            _countryRepo.Setup(r => r.CountryExists(1)).Returns(true);
            _countryRepo.Setup(r => r.GetAuthorsFromACountry(1)).Returns(authors);

            var dtos = Assert.IsAssignableFrom<IEnumerable<AuthorDto>>(Assert.IsType<OkObjectResult>(_sut.GetAuthorsFromACounty(1)).Value);

            Assert.Equal(2, dtos.Count());
        }

        [Fact]
        public void GetAuthorsFromACounty_InvalidModelState_ReturnsBadRequest()
        {
            _countryRepo.Setup(r => r.CountryExists(1)).Returns(true);
            _countryRepo.Setup(r => r.GetAuthorsFromACountry(1)).Returns(new List<Author>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.GetAuthorsFromACounty(1));
        }

        [Fact]
        public void CreateCountry_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestObjectResult>(_sut.CreateCountry(null));
        }

        [Fact]
        public void CreateCountry_Duplicate_Returns422()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.GetCountries()).Returns(new List<Country> { country });

            var result = Assert.IsType<ObjectResult>(_sut.CreateCountry(new Country { Name = " " + country.Name.ToLower() + " " }));

            Assert.Equal(422, result.StatusCode);
        }

        [Fact]
        public void CreateCountry_InvalidModelState_ReturnsBadRequest()
        {
            _countryRepo.Setup(r => r.GetCountries()).Returns(new List<Country>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.CreateCountry(_fixture.Create<Country>()));
        }

        [Fact]
        public void CreateCountry_SaveFails_Returns500()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.GetCountries()).Returns(new List<Country>());
            _countryRepo.Setup(r => r.CreateCountry(country)).Returns(false);

            var result = Assert.IsType<ObjectResult>(_sut.CreateCountry(country));

            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public void CreateCountry_Success_ReturnsCreatedAtRoute()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.GetCountries()).Returns(new List<Country>());
            _countryRepo.Setup(r => r.CreateCountry(country)).Returns(true);

            var result = Assert.IsType<CreatedAtRouteResult>(_sut.CreateCountry(country));

            Assert.Equal("GetCountry", result.RouteName);
        }

        [Fact]
        public void UpdateCountry_Null_ReturnsBadRequest()
        {
            Assert.IsType<BadRequestObjectResult>(_sut.UpdateCountry(1, null));
        }

        [Fact]
        public void UpdateCountry_IdMismatch_ReturnsBadRequest()
        {
            var country = _fixture.Build<Country>().With(c => c.Id, 2).Create();

            Assert.IsType<BadRequestObjectResult>(_sut.UpdateCountry(1, country));
        }

        [Fact]
        public void UpdateCountry_NotExisting_ReturnsNotFound()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.CountryExists(country.Id)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.UpdateCountry(country.Id, country));
        }

        [Fact]
        public void UpdateCountry_DuplicateName_Returns422()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.CountryExists(country.Id)).Returns(true);
            _countryRepo.Setup(r => r.IsDublicateCountryName(country.Id, country.Name)).Returns(true);

            var result = Assert.IsType<ObjectResult>(_sut.UpdateCountry(country.Id, country));

            Assert.Equal(422, result.StatusCode);
        }

        [Fact]
        public void UpdateCountry_InvalidModelState_ReturnsBadRequest()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.CountryExists(country.Id)).Returns(true);
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.UpdateCountry(country.Id, country));
        }

        [Fact]
        public void UpdateCountry_SaveFails_Returns500()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.CountryExists(country.Id)).Returns(true);
            _countryRepo.Setup(r => r.UpdateCountry(country)).Returns(false);

            var result = Assert.IsType<ObjectResult>(_sut.UpdateCountry(country.Id, country));

            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public void UpdateCountry_Success_ReturnsNoContent()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.CountryExists(country.Id)).Returns(true);
            _countryRepo.Setup(r => r.UpdateCountry(country)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.UpdateCountry(country.Id, country));
        }

        [Fact]
        public void DeleteCountry_NotExisting_ReturnsNotFound()
        {
            _countryRepo.Setup(r => r.CountryExists(1)).Returns(false);

            Assert.IsType<NotFoundResult>(_sut.DeleteCountry(1));
        }

        [Fact]
        public void DeleteCountry_UsedByAuthors_Returns409()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.CountryExists(1)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(1)).Returns(country);
            _countryRepo.Setup(r => r.GetAuthorsFromACountry(1)).Returns(_fixture.CreateMany<Author>(1).ToList());

            var result = Assert.IsType<ObjectResult>(_sut.DeleteCountry(1));

            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public void DeleteCountry_InvalidModelState_ReturnsBadRequest()
        {
            _countryRepo.Setup(r => r.CountryExists(1)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(1)).Returns(_fixture.Create<Country>());
            _countryRepo.Setup(r => r.GetAuthorsFromACountry(1)).Returns(new List<Author>());
            _sut.ModelState.AddModelError("k", "e");

            Assert.IsType<BadRequestObjectResult>(_sut.DeleteCountry(1));
        }

        [Fact]
        public void DeleteCountry_DeleteFails_Returns500()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.CountryExists(1)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(1)).Returns(country);
            _countryRepo.Setup(r => r.GetAuthorsFromACountry(1)).Returns(new List<Author>());
            _countryRepo.Setup(r => r.DeleteCountry(country)).Returns(false);

            var result = Assert.IsType<ObjectResult>(_sut.DeleteCountry(1));

            Assert.Equal(500, result.StatusCode);
        }

        [Fact]
        public void DeleteCountry_Success_ReturnsNoContent()
        {
            var country = _fixture.Create<Country>();
            _countryRepo.Setup(r => r.CountryExists(1)).Returns(true);
            _countryRepo.Setup(r => r.GetCountry(1)).Returns(country);
            _countryRepo.Setup(r => r.GetAuthorsFromACountry(1)).Returns(new List<Author>());
            _countryRepo.Setup(r => r.DeleteCountry(country)).Returns(true);

            Assert.IsType<NoContentResult>(_sut.DeleteCountry(1));
        }
    }
}
