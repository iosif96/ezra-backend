# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build
dotnet build

# Run the API
dotnet run --project src/Api/Api.csproj

# Run unit tests
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj

# Run integration tests (requires SQL Server)
dotnet test tests/Application.IntegrationTests/Application.IntegrationTests.csproj

# Run a single test by name
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj --filter "FullyQualifiedName~TestMethodName"

# Lint / format
dotnet format
```

## Architecture

This is a .NET 8 ASP.NET Web API using **Vertical Slice Architecture** with CQRS via MediatR.

### Two Projects

- **Api** (`src/Api/`) — ASP.NET entry point. Controllers live in `Endpoints/` and inherit from `ApiControllerBase` which provides a `Mediator` property. Controllers delegate all work to MediatR commands/queries. Routes follow `api/[controller]` convention.
- **Application** (`src/Application/`) — All business logic, domain entities, infrastructure, and shared concerns.

### Vertical Slice Pattern

Each feature in `src/Application/Features/{FeatureName}/` is a self-contained file (or small set of files) containing the command/query record, validator, handler, and response DTOs together. Namespace convention: `Application.Features.{FeatureName}.{Operation}`.

Controllers use `using` aliases to reference feature namespaces (e.g., `using CreateFlight = Application.Features.Flights.CreateFlight;`).

### Key Folders in Application

- `Features/` — Vertical slices organized by domain (Flights, Gates, Merchants, Users, etc.)
- `Domain/Entities/`, `Domain/Enums/`, `Domain/Events/` — Domain model
- `Infrastructure/Persistence/` — EF Core DbContext (`ApplicationDbContext`), migrations, interceptors (auditing, soft delete, domain events)
- `Infrastructure/Services/` — Service implementations (auth, mail, blob storage, Gemini AI, cryptography)
- `Common/Behaviours/` — MediatR pipeline behaviours: Authorization, Validation, Performance, UnhandledException
- `Common/Interfaces/` — Service abstractions
- `Common/Security/` — `[Authorize]` attribute for MediatR commands

### Key Patterns

- **MediatR pipeline**: Requests pass through Authorization → Validation → Performance → UnhandledException behaviours
- **FluentValidation**: Validators auto-registered from assembly; `AbstractValidator<TCommand>` classes co-located with commands
- **ErrorOr**: Used for result types in some features
- **AutoMapper**: Registered from assembly for entity-to-DTO mapping
- **Global usings** in `src/Application/GlobalUsings.cs`: `MediatR`, `AutoMapper`, common exceptions
- **Central package management**: Versions defined in `Directory.Packages.props`

### Database

- SQL Server via EF Core (connection string `DefaultConnection` in appsettings)
- EF Core interceptors for auditable entities, soft delete, and domain event dispatch
- Migrations: `dotnet ef migrations add "Name" --project src/Application --startup-project src/Api --output-dir Infrastructure/Persistence/Migrations`
- Update DB: `dotnet ef database update --project src/Application --startup-project src/Api`

### Services

Auth uses JWT (`ConfigureJWT.cs`), with `ICurrentUserService`, `IAuthService`, `ITokenService`, and API key support (`IApiKeyService`). Azure Blob Storage for file uploads. MailKit for email. ImageSharp for thumbnails.
