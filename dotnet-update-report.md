# .NET Update Report

**Project:** DemoBookAPI (ASP.NET Core Web API)
**Upgrade:** .NET Core 3.1 -> .NET 10 (LTS), branch `upgrade-dotnet-10`

## Changes
- Target framework: `netcoreapp3.1` -> `net10.0`.
- `Startup.cs` removed. All configuration is now in `Program.cs` (minimal hosting, `WebApplication.CreateBuilder`). DB seeding runs in a scope before the app starts.
- Swagger UI added with Swashbuckle.AspNetCore 10.3.0 (`/swagger`).
- Solution migrated from `DemoBookAPI.sln` to `DemoBookAPI.slnx` (old `.sln` removed).

## Packages
| Package | Before | After |
|---|---|---|
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 3.1.2 | 10.0.12 |
| Microsoft.EntityFrameworkCore | 3.1.1 | 10.0.12 |
| Microsoft.EntityFrameworkCore.SqlServer | 3.1.1 | 10.0.12 |
| Microsoft.EntityFrameworkCore.Tools | 3.1.1 | 10.0.12 |
| Microsoft.VisualStudio.Web.CodeGeneration.Design | 3.1.1 | 10.0.2 |
| Microsoft.Extensions.Logging.Debug | 3.1.0 | removed (in shared framework) |
| NuGet.Packaging / NuGet.Protocol | transitive 6.12.1 (vulnerable) | 7.9.0 |
| Swashbuckle.AspNetCore | - | 10.3.0 (new) |

## Issues fixed
- NU1510 (redundant package) and NU1901 (vulnerable transitive NuGet packages) resolved.

## Result
- `dotnet build DemoBookAPI.slnx`: 0 errors, 0 warnings.
- No test projects. Runtime needs SQL Server (`connectionStrings:bookDbConnectionString`); not exercised here.
