# 📚 Simple Web API Book Shop

A clean, layered **ASP.NET Core Web API (.NET 10)** for managing an online book shop catalog (books, authors, categories, countries, reviewers and reviews), backed by **Entity Framework Core** and **SQL Server**, with unit and integration test coverage.

## ✨ Features

- Full REST CRUD for **Books, Authors, Categories, Countries, Reviewers and Reviews**
- Many-to-many relations (Book-Author, Book-Category) and one-to-many (Country-Authors, Book-Reviews)
- **Repository pattern** with interfaces for loose coupling and testability
- **DTOs** to keep API contracts separate from EF entities
- **EF Core migrations** and database seeding
- Interactive **Swagger / OpenAPI** docs (Swashbuckle)
- JSON serialization via Newtonsoft.Json

## 🗂️ Solution Structure

| Project | Description |
|---|---|
| `DemoBookAPI` | The Web API: controllers, DTOs, models, repositories, `BookDbContext`, migrations, seeding |
| `DemoBookAPI.Tests` | xUnit **unit tests** for controllers and repositories (using a test fixture factory) |
| `DemoBookAPI.Integration.Tests` | xUnit **integration tests** running the API end-to-end through a custom `BookApiFactory` |

```
DemoBookAPI/
├── Controllers/   # HTTP endpoints
├── Dtos/          # Data transfer objects
├── Models/        # EF Core entities
├── Services/      # Repositories + DbContext
├── Migrations/    # EF Core migrations
└── Program.cs     # App startup & DI
```

## 🧰 Tech Stack

.NET 10 · ASP.NET Core · Entity Framework Core 10 · SQL Server · Swagger · xUnit

## 🚀 Getting Started

1. **Clone**
   ```bash
   git clone https://github.com/svetlanavrabie/Simple-Web-API-Book-Shop.git
   cd Simple-Web-API-Book-Shop
   ```
2. **Configure** the SQL Server connection string in `DemoBookAPI/appsettings.json`.
3. **Apply migrations**
   ```bash
   dotnet ef database update --project DemoBookAPI
   ```
4. **Run**
   ```bash
   dotnet run --project DemoBookAPI
   ```
5. Open **Swagger UI** at `/swagger` to explore the endpoints.

## 🧪 Running Tests

```bash
dotnet test
```

Runs both the unit tests and the integration tests across the solution.
