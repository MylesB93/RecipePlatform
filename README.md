# RecipePlatform

RecipePlatform is a .NET 10 minimal API for creating, retrieving, updating, deleting, searching, and paging recipes. Data is stored in PostgreSQL through Entity Framework Core.

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL 17, running locally
- Redis 7, running locally
- Docker Desktop, if you want to run the integration tests or the supplied containers

## Run locally

1. Create a PostgreSQL database named `recipes` with a user that matches the development connection string in [RecipePlatform/appsettings.json](RecipePlatform/appsettings.json), or set your own connection string:

   ```powershell
   $env:ConnectionStrings__Postgres = "Host=localhost;Port=5432;Database=recipes;Username=postgres;Password=postgres"
   $env:ConnectionStrings__Redis = "localhost:6379"
   ```

2. Restore dependencies and apply the Entity Framework migrations:

   ```powershell
   dotnet restore
   dotnet tool install --global dotnet-ef
   dotnet ef database update --project RecipePlatform
   ```

   If `dotnet-ef` is already installed, omit the installation command.

3. Run the API:

   ```powershell
   dotnet run --project RecipePlatform
   ```

   The development profile listens on `http://localhost:5167` by default. Confirm the service is running at `GET /` or `GET /health`.

## API endpoints

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/health` | Health check |
| `GET` | `/api/recipes` | List recipes; supports `page`, `pageSize`, and `search` query parameters |
| `POST` | `/api/recipes` | Create a recipe |
| `GET` | `/api/recipes/{id}` | Get a recipe by ID |
| `PUT` | `/api/recipes/{id}` | Update a recipe |
| `DELETE` | `/api/recipes/{id}` | Delete a recipe |

Example request:

```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:5167/api/recipes `
  -ContentType "application/json" `
  -Body '{"name":"Chicken Curry","description":"A simple chicken curry recipe."}'
```

## Tests

Run the unit tests:

```powershell
dotnet test RecipePlatform.UnitTests/RecipePlatform.UnitTests.csproj
```

Run the integration tests:

```powershell
dotnet test RecipePlatform.IntegrationTests/RecipePlatform.IntegrationTests.csproj
```

The integration tests use Testcontainers to start PostgreSQL, so Docker Desktop must be running.

## Caching

Recipe reads use Redis with a cache-aside strategy. Individual recipes are cached for 10 minutes and paged/search results for one minute. Creating, updating, or deleting a recipe updates or removes the item cache and invalidates all list/search results through a cache-generation key.

Set `ConnectionStrings__Redis` to override the Redis connection string. When the API runs through Docker Compose, it uses the included `redis` service automatically.

## Logging

The API writes structured JSON logs to standard output with Serilog. Request completion, rejected recipe creation requests, missing recipes, cache activity, and unhandled failures include structured properties suitable for container log collection.

## Containers

The repository includes a `docker-compose.yml` with an API service, PostgreSQL 17, and Redis 7. Redis data is stored in the `redis-data` Docker volume.

```powershell
docker compose up --build
```

This exposes the API on `http://localhost:8080`. The API does not currently apply Entity Framework migrations at startup, so apply the migrations before using recipe endpoints when running outside the test suite.

## Continuous integration

GitHub Actions restores dependencies, builds the solution, runs the test suite, and builds the API container image for pushes and pull requests targeting `main`.
