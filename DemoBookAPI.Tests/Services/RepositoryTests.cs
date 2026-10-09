using DemoBookAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace DemoBookAPI.Tests.Services
{
    internal static class InMemoryDb
    {
        public static BookDbContext Create()
        {
            var options = new DbContextOptionsBuilder<BookDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new BookDbContext(options);
        }
    }

    public class CountryRepositoryTests
    {
        private readonly BookDbContext _db = InMemoryDb.Create();
        private readonly CountryRepository _sut;

        public CountryRepositoryTests()
        {
            _sut = new CountryRepository(_db);
            var de = new Country { Id = 1, Name = "Germany" };
            var fr = new Country { Id = 2, Name = "France" };
            _db.Countries.AddRange(de, fr);
            _db.Authors.Add(new Author { Id = 1, FirstName = "A", LastName = "B", Country = de });
            _db.SaveChanges();
        }

        [Fact]
        public void GetCountries_ReturnsOrderedByName() =>
            Assert.Equal(new[] { "France", "Germany" }, _sut.GetCountries().Select(c => c.Name));

        [Fact]
        public void GetCountry_ReturnsMatching() => Assert.Equal("Germany", _sut.GetCountry(1).Name);

        [Fact]
        public void GetCountry_Missing_ReturnsNull() => Assert.Null(_sut.GetCountry(99));

        [Fact]
        public void GetCountryofAnAuthor_ReturnsCountry() => Assert.Equal(1, _sut.GetCountryofAnAuthor(1).Id);

        [Fact]
        public void GetAuthorsFromACountry_ReturnsAuthors() => Assert.Single(_sut.GetAuthorsFromACountry(1));

        [Fact]
        public void CountryExists_ReturnsExpected()
        {
            Assert.True(_sut.CountryExists(1));
            Assert.False(_sut.CountryExists(99));
        }

        [Fact]
        public void IsDublicateCountryName_DetectsDuplicatesIgnoringCaseAndSelf()
        {
            Assert.True(_sut.IsDublicateCountryName(2, " germany "));
            Assert.False(_sut.IsDublicateCountryName(1, "Germany"));
            Assert.False(_sut.IsDublicateCountryName(1, "Spain"));
        }

        [Fact]
        public void CreateCountry_Persists()
        {
            Assert.True(_sut.CreateCountry(new Country { Id = 3, Name = "Spain" }));
            Assert.True(_sut.CountryExists(3));
        }

        [Fact]
        public void UpdateCountry_Persists()
        {
            var country = _sut.GetCountry(1);
            country.Name = "Deutschland";

            Assert.True(_sut.UpdateCountry(country));
            Assert.Equal("Deutschland", _sut.GetCountry(1).Name);
        }

        [Fact]
        public void DeleteCountry_Removes()
        {
            Assert.True(_sut.DeleteCountry(_sut.GetCountry(2)));
            Assert.False(_sut.CountryExists(2));
        }

        [Fact]
        public void Save_ReturnsTrue() => Assert.True(_sut.Save());
    }

    public class CategoryRepositoryTests
    {
        private readonly BookDbContext _db = InMemoryDb.Create();
        private readonly CategoryRepository _sut;

        public CategoryRepositoryTests()
        {
            _sut = new CategoryRepository(_db);
            var scifi = new Category { Id = 1, Name = "SciFi" };
            var drama = new Category { Id = 2, Name = "Drama" };
            var book = new Book { Id = 1, Isbn = "123", Title = "T" };
            _db.Categories.AddRange(scifi, drama);
            _db.Books.Add(book);
            _db.BookCategories.Add(new BookCategory { Book = book, Category = scifi });
            _db.SaveChanges();
        }

        [Fact]
        public void GetCategories_ReturnsOrderedByName() =>
            Assert.Equal(new[] { "Drama", "SciFi" }, _sut.GetCategories().Select(c => c.Name));

        [Fact]
        public void GetCategory_ReturnsMatching() => Assert.Equal("SciFi", _sut.GetCategory(1).Name);

        [Fact]
        public void GetCategory_Missing_ReturnsNull() => Assert.Null(_sut.GetCategory(99));

        [Fact]
        public void GetAllCategoriesForABook_ReturnsCategories() =>
            Assert.Equal(1, Assert.Single(_sut.GetAllCategoriesForABook(1)).Id);

        [Fact]
        public void GetBookForCategory_ReturnsBooks() =>
            Assert.Equal(1, Assert.Single(_sut.GetBookForCategory(1)).Id);

        [Fact]
        public void CategoryExists_ReturnsExpected()
        {
            Assert.True(_sut.CategoryExists(1));
            Assert.False(_sut.CategoryExists(99));
        }

        [Fact]
        public void IsDublicateCategoryName_DetectsDuplicates()
        {
            Assert.True(_sut.IsDublicateCategoryName(2, " scifi "));
            Assert.False(_sut.IsDublicateCategoryName(1, "SciFi"));
        }

        [Fact]
        public void CreateCategory_Persists()
        {
            Assert.True(_sut.CreateCategory(new Category { Id = 3, Name = "Comedy" }));
            Assert.True(_sut.CategoryExists(3));
        }

        [Fact]
        public void UpdateCategory_Persists()
        {
            var category = _sut.GetCategory(1);
            category.Name = "Fantasy";

            Assert.True(_sut.UpdateCategory(category));
            Assert.Equal("Fantasy", _sut.GetCategory(1).Name);
        }

        [Fact]
        public void DeleteCategory_Removes()
        {
            Assert.True(_sut.DeleteCategory(_sut.GetCategory(2)));
            Assert.False(_sut.CategoryExists(2));
        }

        [Fact]
        public void Save_ReturnsTrue() => Assert.True(_sut.Save());
    }

    public class AuthorRepositoryTests
    {
        private readonly BookDbContext _db = InMemoryDb.Create();
        private readonly AuthorRepository _sut;

        public AuthorRepositoryTests()
        {
            _sut = new AuthorRepository(_db);
            var a1 = new Author { Id = 1, FirstName = "Zed", LastName = "Zulu" };
            var a2 = new Author { Id = 2, FirstName = "Amy", LastName = "Alpha" };
            var book = new Book { Id = 1, Isbn = "123", Title = "T" };
            _db.Authors.AddRange(a1, a2);
            _db.Books.Add(book);
            _db.BookAuthors.Add(new BookAuthor { Book = book, Author = a1 });
            _db.SaveChanges();
        }

        [Fact]
        public void GetAuthors_ReturnsOrderedByLastName() =>
            Assert.Equal(new[] { "Alpha", "Zulu" }, _sut.GetAuthors().Select(a => a.LastName));

        [Fact]
        public void GetAuthor_ReturnsMatching() => Assert.Equal("Zed", _sut.GetAuthor(1).FirstName);

        [Fact]
        public void GetAuthorsOfABook_ReturnsAuthors() =>
            Assert.Equal(1, Assert.Single(_sut.GetAuthorsOfABook(1)).Id);

        [Fact]
        public void GetBooksByAuthor_ReturnsBooks()
        {
            Assert.Single(_sut.GetBooksByAuthor(1));
            Assert.Empty(_sut.GetBooksByAuthor(2));
        }

        [Fact]
        public void AuthorExists_ReturnsExpected()
        {
            Assert.True(_sut.AuthorExists(1));
            Assert.False(_sut.AuthorExists(99));
        }

        [Fact]
        public void CreateAuthor_Persists()
        {
            Assert.True(_sut.CreateAuthor(new Author { Id = 3, FirstName = "N", LastName = "M" }));
            Assert.True(_sut.AuthorExists(3));
        }

        [Fact]
        public void UpdateAuthor_Persists()
        {
            var author = _sut.GetAuthor(2);
            author.FirstName = "Changed";

            Assert.True(_sut.UpdateAuthor(author));
            Assert.Equal("Changed", _sut.GetAuthor(2).FirstName);
        }

        [Fact]
        public void DeleteAuthor_Removes()
        {
            Assert.True(_sut.DeleteAuthor(_sut.GetAuthor(2)));
            Assert.False(_sut.AuthorExists(2));
        }

        [Fact]
        public void Save_ReturnsTrue() => Assert.True(_sut.Save());
    }

    public class BookRepositoryTests
    {
        private readonly BookDbContext _db = InMemoryDb.Create();
        private readonly BookRepository _sut;

        public BookRepositoryTests()
        {
            _sut = new BookRepository(_db);
            var b1 = new Book { Id = 1, Isbn = "ABC1", Title = "Zebra" };
            var b2 = new Book { Id = 2, Isbn = "ABC2", Title = "Apple" };
            _db.Books.AddRange(b1, b2);
            _db.Authors.AddRange(new Author { Id = 1, FirstName = "A", LastName = "A" }, new Author { Id = 2, FirstName = "B", LastName = "B" });
            _db.Categories.AddRange(new Category { Id = 1, Name = "C1" }, new Category { Id = 2, Name = "C2" });
            var reviewer = new Reviewer { Id = 1, FirstName = "R", LastName = "R" };
            _db.Reviewers.Add(reviewer);
            _db.Reviews.AddRange(
                new Review { Id = 1, Headline = "h", ReviewText = "t", Rating = 4, Book = b1, Reviewer = reviewer },
                new Review { Id = 2, Headline = "h", ReviewText = "t", Rating = 5, Book = b1, Reviewer = reviewer });
            _db.SaveChanges();
        }

        [Fact]
        public void GetBooks_ReturnsOrderedByTitle() =>
            Assert.Equal(new[] { "Apple", "Zebra" }, _sut.GetBooks().Select(b => b.Title));

        [Fact]
        public void GetBook_ById_ReturnsMatching() => Assert.Equal("Zebra", _sut.GetBook(1).Title);

        [Fact]
        public void GetBook_ByIsbn_ReturnsMatching() => Assert.Equal(2, _sut.GetBook("ABC2").Id);

        [Fact]
        public void BookExists_ById_ReturnsExpected()
        {
            Assert.True(_sut.BookExists(1));
            Assert.False(_sut.BookExists(99));
        }

        [Fact]
        public void BookExists_ByIsbn_ReturnsExpected()
        {
            Assert.True(_sut.BookExists("ABC1"));
            Assert.False(_sut.BookExists("NOPE"));
        }

        [Fact]
        public void GetBookRating_ReturnsAverage() => Assert.Equal(4.5m, _sut.GetBookRating(1));

        [Fact]
        public void GetBookRating_NoReviews_ReturnsZero() => Assert.Equal(0m, _sut.GetBookRating(2));

        [Fact]
        public void IsDublicateIsbn_DetectsDuplicates()
        {
            Assert.True(_sut.IsDublicateIsbn(2, " abc1 "));
            Assert.False(_sut.IsDublicateIsbn(1, "ABC1"));
        }

        [Fact]
        public void CreateBook_PersistsWithRelations()
        {
            var book = new Book { Id = 3, Isbn = "ABC3", Title = "New" };

            Assert.True(_sut.CreateBook(new List<int> { 1, 2 }, new List<int> { 1 }, book));
            Assert.Equal(2, _db.BookAuthors.Count(ba => ba.BookId == 3));
            Assert.Equal(1, _db.BookCategories.Count(bc => bc.BookId == 3));
        }

        [Fact]
        public void UpdateBook_ReplacesRelations()
        {
            _sut.CreateBook(new List<int> { 1 }, new List<int> { 1 }, new Book { Id = 3, Isbn = "ABC3", Title = "New" });
            var book = _sut.GetBook(3);

            Assert.True(_sut.UpdateBook(new List<int> { 2 }, new List<int> { 2 }, book));
            Assert.Equal(2, Assert.Single(_db.BookAuthors.Where(ba => ba.BookId == 3)).AuthorId);
            Assert.Equal(2, Assert.Single(_db.BookCategories.Where(bc => bc.BookId == 3)).CategoryId);
        }

        [Fact]
        public void DeleteBook_Removes()
        {
            Assert.True(_sut.DeleteBook(_sut.GetBook(2)));
            Assert.False(_sut.BookExists(2));
        }

        [Fact]
        public void Save_ReturnsTrue() => Assert.True(_sut.Save());
    }

    public class ReviewerRepositoryTests
    {
        private readonly BookDbContext _db = InMemoryDb.Create();
        private readonly ReviewerRepository _sut;

        public ReviewerRepositoryTests()
        {
            _sut = new ReviewerRepository(_db);
            var r1 = new Reviewer { Id = 1, FirstName = "Z", LastName = "Zulu" };
            var r2 = new Reviewer { Id = 2, FirstName = "A", LastName = "Alpha" };
            var book = new Book { Id = 1, Isbn = "123", Title = "T" };
            _db.Reviewers.AddRange(r1, r2);
            _db.Books.Add(book);
            _db.Reviews.Add(new Review { Id = 1, Headline = "h", ReviewText = "t", Rating = 3, Book = book, Reviewer = r1 });
            _db.SaveChanges();
        }

        [Fact]
        public void GetReviewers_ReturnsOrderedByLastName() =>
            Assert.Equal(new[] { "Alpha", "Zulu" }, _sut.GetReviewers().Select(r => r.LastName));

        [Fact]
        public void GetReviewer_ReturnsMatching() => Assert.Equal("Z", _sut.GetReviewer(1).FirstName);

        [Fact]
        public void GetReviewerOfAReview_ReturnsReviewer() => Assert.Equal(1, _sut.GetReviewerOfAReview(1).Id);

        [Fact]
        public void GetReviewsByReviewer_ReturnsReviews()
        {
            Assert.Single(_sut.GetReviewsByReviewer(1));
            Assert.Empty(_sut.GetReviewsByReviewer(2));
        }

        [Fact]
        public void ReviewerExists_ReturnsExpected()
        {
            Assert.True(_sut.ReviewerExists(1));
            Assert.False(_sut.ReviewerExists(99));
        }

        [Fact]
        public void CreateReviewer_Persists()
        {
            Assert.True(_sut.CreateReviewer(new Reviewer { Id = 3, FirstName = "N", LastName = "M" }));
            Assert.True(_sut.ReviewerExists(3));
        }

        [Fact]
        public void UpdateReviewer_Persists()
        {
            var reviewer = _sut.GetReviewer(2);
            reviewer.FirstName = "Changed";

            Assert.True(_sut.UpdateReviewer(reviewer));
            Assert.Equal("Changed", _sut.GetReviewer(2).FirstName);
        }

        [Fact]
        public void DeleteReviewer_Removes()
        {
            Assert.True(_sut.DeleteReviewer(_sut.GetReviewer(2)));
            Assert.False(_sut.ReviewerExists(2));
        }

        [Fact]
        public void Save_ReturnsTrue() => Assert.True(_sut.Save());
    }

    public class ReviewRepositoryTests
    {
        private readonly BookDbContext _db = InMemoryDb.Create();
        private readonly ReviewRepository _sut;

        public ReviewRepositoryTests()
        {
            _sut = new ReviewRepository(_db);
            var book = new Book { Id = 1, Isbn = "123", Title = "T" };
            var other = new Book { Id = 2, Isbn = "456", Title = "O" };
            var reviewer = new Reviewer { Id = 1, FirstName = "R", LastName = "R" };
            _db.Books.AddRange(book, other);
            _db.Reviewers.Add(reviewer);
            _db.Reviews.AddRange(
                new Review { Id = 1, Headline = "h", ReviewText = "t", Rating = 5, Book = book, Reviewer = reviewer },
                new Review { Id = 2, Headline = "h", ReviewText = "t", Rating = 2, Book = book, Reviewer = reviewer });
            _db.SaveChanges();
        }

        [Fact]
        public void GetReviews_ReturnsOrderedByRating() =>
            Assert.Equal(new[] { 2, 5 }, _sut.GetReviews().Select(r => r.Rating));

        [Fact]
        public void GetReview_ReturnsMatching() => Assert.Equal(5, _sut.GetReview(1).Rating);

        [Fact]
        public void GetReviewsOfABook_ReturnsReviews()
        {
            Assert.Equal(2, _sut.GetReviewsOfABook(1).Count);
            Assert.Empty(_sut.GetReviewsOfABook(2));
        }

        [Fact]
        public void GetBookOfAReview_ReturnsBook() => Assert.Equal(1, _sut.GetBookOfAReview(1).Id);

        [Fact]
        public void ReviewExists_ReturnsExpected()
        {
            Assert.True(_sut.ReviewExists(1));
            Assert.False(_sut.ReviewExists(99));
        }

        [Fact]
        public void CreateReview_Persists()
        {
            var review = new Review { Id = 3, Headline = "h", ReviewText = "t", Rating = 3, Book = _db.Books.Find(2), Reviewer = _db.Reviewers.Find(1) };

            Assert.True(_sut.CreateReview(review));
            Assert.True(_sut.ReviewExists(3));
        }

        [Fact]
        public void UpdateReview_Persists()
        {
            var review = _sut.GetReview(1);
            review.Rating = 1;

            Assert.True(_sut.UpdateReview(review));
            Assert.Equal(1, _sut.GetReview(1).Rating);
        }

        [Fact]
        public void DeleteReview_Removes()
        {
            Assert.True(_sut.DeleteReview(_sut.GetReview(1)));
            Assert.False(_sut.ReviewExists(1));
        }

        [Fact]
        public void DeleteReviews_RemovesAll()
        {
            Assert.True(_sut.DeleteReviews(_sut.GetReviewsOfABook(1).ToList()));
            Assert.Empty(_sut.GetReviews());
        }

        [Fact]
        public void Save_ReturnsTrue() => Assert.True(_sut.Save());
    }
}
