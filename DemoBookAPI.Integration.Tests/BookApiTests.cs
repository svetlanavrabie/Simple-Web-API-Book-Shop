using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DemoBookAPI.Integration.Tests
{
    public class BookApiTests : IClassFixture<BookApiFactory>
    {
        private readonly HttpClient _client;

        public BookApiTests(BookApiFactory factory)
        {
            _client = factory.CreateClient();
        }

        private static string Unique() => Guid.NewGuid().ToString("N")[..8];

        private static async Task<JsonElement> ReadJson(HttpResponseMessage response)
        {
            var text = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(text).RootElement.Clone();
        }

        private async Task<int> CreateCountry()
        {
            var response = await _client.PostAsJsonAsync("api/countries", new { name = "Country-" + Unique() });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await ReadJson(response)).GetProperty("id").GetInt32();
        }

        private async Task<int> CreateAuthor(int countryId, string country = "X")
        {
            var response = await _client.PostAsJsonAsync("api/authors", new
            {
                firstName = "First",
                lastName = "Last",
                country = new { id = countryId, name = country }
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await ReadJson(response)).GetProperty("id").GetInt32();
        }

        private async Task<int> CreateCategory()
        {
            var response = await _client.PostAsJsonAsync("api/categories", new { name = "Category-" + Unique() });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await ReadJson(response)).GetProperty("id").GetInt32();
        }

        private async Task<(int Id, string Isbn)> CreateBook(int authorId, int categoryId)
        {
            var isbn = Unique();
            var response = await _client.PostAsJsonAsync(
                $"api/books?autId={authorId}&catId={categoryId}",
                new { isbn, title = "Title " + isbn });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return ((await ReadJson(response)).GetProperty("id").GetInt32(), isbn);
        }

        private async Task<int> CreateReviewer()
        {
            var response = await _client.PostAsJsonAsync("api/reviewers", new { firstName = "Rev", lastName = "Iewer" });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await ReadJson(response)).GetProperty("id").GetInt32();
        }

        [Fact]
        public async Task GetCountries_ReturnsSeededData()
        {
            var response = await _client.GetAsync("api/countries");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True((await ReadJson(response)).GetArrayLength() > 0);
        }

        [Fact]
        public async Task GetCountry_Unknown_ReturnsNotFound()
        {
            var response = await _client.GetAsync("api/countries/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Countries_CreateGetUpdateDelete_Flow()
        {
            var name = "Country-" + Unique();
            var create = await _client.PostAsJsonAsync("api/countries", new { name });
            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
            var id = (await ReadJson(create)).GetProperty("id").GetInt32();

            var get = await _client.GetAsync($"api/countries/{id}");
            Assert.Equal(HttpStatusCode.OK, get.StatusCode);
            Assert.Equal(name, (await ReadJson(get)).GetProperty("name").GetString());

            var update = await _client.PutAsJsonAsync($"api/countries/{id}", new { id, name = name + "-upd" });
            Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

            var delete = await _client.DeleteAsync($"api/countries/{id}");
            Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"api/countries/{id}")).StatusCode);
        }

        [Fact]
        public async Task CreateCountry_Duplicate_Returns422()
        {
            var name = "Country-" + Unique();
            await _client.PostAsJsonAsync("api/countries", new { name });

            var response = await _client.PostAsJsonAsync("api/countries", new { name });

            Assert.Equal((HttpStatusCode)422, response.StatusCode);
        }

        [Fact]
        public async Task Categories_CreateGetDelete_Flow()
        {
            var id = await CreateCategory();

            Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"api/categories/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("api/categories")).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"api/categories/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"api/categories/{id}")).StatusCode);
        }

        [Fact]
        public async Task Authors_CreateGetByCountryDelete_Flow()
        {
            var countryId = await CreateCountry();
            var authorId = await CreateAuthor(countryId);

            Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"api/authors/{authorId}")).StatusCode);

            var byCountry = await _client.GetAsync($"api/countries/{countryId}/authors");
            Assert.Equal(1, (await ReadJson(byCountry)).GetArrayLength());

            Assert.Equal(HttpStatusCode.Conflict, (await _client.DeleteAsync($"api/countries/{countryId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"api/authors/{authorId}")).StatusCode);
        }

        [Fact]
        public async Task CreateAuthor_UnknownCountry_ReturnsNotFound()
        {
            var response = await _client.PostAsJsonAsync("api/authors", new
            {
                firstName = "First",
                lastName = "Last",
                country = new { id = 999999, name = "Nowhere" }
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Books_CreateGetRatingDelete_Flow()
        {
            var authorId = await CreateAuthor(await CreateCountry());
            var categoryId = await CreateCategory();
            var (bookId, isbn) = await CreateBook(authorId, categoryId);

            Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"api/books/{bookId}")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"api/books/ISBN/{isbn}")).StatusCode);

            var rating = await _client.GetAsync($"api/books/{bookId}/rating");
            Assert.Equal(HttpStatusCode.OK, rating.StatusCode);
            Assert.Equal(0m, (await ReadJson(rating)).GetDecimal());

            Assert.Equal(1, (await ReadJson(await _client.GetAsync($"api/authors/{authorId}/books"))).GetArrayLength());
            Assert.Equal(1, (await ReadJson(await _client.GetAsync($"api/categories/{categoryId}/books"))).GetArrayLength());

            Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"api/books/{bookId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"api/books/{bookId}")).StatusCode);
        }

        [Fact]
        public async Task CreateBook_WithoutAuthors_ReturnsBadRequest()
        {
            var response = await _client.PostAsJsonAsync("api/books", new { isbn = Unique(), title = "T" });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Reviewers_CreateGetUpdateDelete_Flow()
        {
            var id = await CreateReviewer();

            Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"api/reviewers/{id}")).StatusCode);

            var update = await _client.PutAsJsonAsync($"api/reviewers/{id}", new { id, firstName = "New", lastName = "Name" });
            Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);

            Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"api/reviewers/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"api/reviewers/{id}")).StatusCode);
        }

        [Fact]
        public async Task Reviews_CreateGetRatingDelete_Flow()
        {
            var authorId = await CreateAuthor(await CreateCountry());
            var (bookId, isbn) = await CreateBook(authorId, await CreateCategory());
            var reviewerId = await CreateReviewer();

            var create = await _client.PostAsJsonAsync("api/reviews", new
            {
                headline = "A great headline",
                reviewText = new string('x', 60),
                rating = 4,
                book = new { id = bookId, isbn, title = "Title " + isbn },
                reviewer = new { id = reviewerId, firstName = "Rev", lastName = "Iewer" }
            });
            Assert.Equal(HttpStatusCode.Created, create.StatusCode);
            var reviewId = (await ReadJson(create)).GetProperty("id").GetInt32();

            Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync($"api/reviews/{reviewId}")).StatusCode);
            Assert.Equal(1, (await ReadJson(await _client.GetAsync($"api/reviews/books/{bookId}"))).GetArrayLength());
            Assert.Equal(1, (await ReadJson(await _client.GetAsync($"api/reviewers/{reviewerId}/reviews"))).GetArrayLength());
            Assert.Equal(4m, (await ReadJson(await _client.GetAsync($"api/books/{bookId}/rating"))).GetDecimal());

            Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"api/reviews/{reviewId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"api/reviews/{reviewId}")).StatusCode);
        }

        [Fact]
        public async Task CreateReview_UnknownBook_ReturnsNotFound()
        {
            var reviewerId = await CreateReviewer();

            var response = await _client.PostAsJsonAsync("api/reviews", new
            {
                headline = "A great headline",
                reviewText = new string('x', 60),
                rating = 4,
                book = new { id = 999999, isbn = "ABCDE", title = "T" },
                reviewer = new { id = reviewerId, firstName = "Rev", lastName = "Iewer" }
            });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
