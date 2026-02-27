# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

POP Forums is an ASP.NET Core forum and Q&A application targeting .NET 10. It uses SignalR for real-time updates, TypeScript for front-end components, and SQL Server as the primary data store.

## Solution Structure

| Project | Purpose |
|---|---|
| `PopForums` | Core business logic, service interfaces, repository interfaces, models |
| `PopForums.Sql` | SQL Server data access implementations, caching layer, migrations scripts |
| `PopForums.Mvc` | ASP.NET MVC area (`/Forums`), controllers, views, TypeScript client, CSS |
| `PopForums.Web` | Host app template — references above projects, contains `Program.cs` |
| `PopForums.AzureKit` | Azure-specific implementations: Redis cache, Azure Search, Blob Storage, queues |
| `PopForums.AzureKit.Functions` | Azure Functions implementations for background jobs |
| `PopForums.ElasticKit` | ElasticSearch search implementation |
| `PopForums.Test` | xUnit tests using NSubstitute, covers services and some MVC code |

## Build Commands

### .NET
```bash
# Build entire solution
dotnet build PopForums.sln

# Run all tests
dotnet test src/PopForums.Test/PopForums.Test.csproj

# Run a single test class
dotnet test src/PopForums.Test/PopForums.Test.csproj --filter "FullyQualifiedName~PostMasterServiceTests"

# Run a single test method
dotnet test src/PopForums.Test/PopForums.Test.csproj --filter "FullyQualifiedName~PostMasterServiceTests.SomeMethodName"
```

### Front-end asset setup (run once from `src/PopForums.Mvc/`)
```bash
npm install
npx gulp copies   # copy node_modules assets (Bootstrap, SignalR, TinyMCE, Vue, etc.) to wwwroot/lib
npx gulp css      # minify CSS
```

TypeScript compilation is handled automatically by `Microsoft.TypeScript.MSBuild` as part of the .NET build — no manual `tsc` or `gulp ts` needed. The Mvc project's static assets (JS, CSS, fonts) are embedded into the NuGet package and served to the host app via `StaticWebAssetBasePath=/PopForums`. The app itself is run from `PopForums.Web`, not `PopForums.Mvc`.

## Running Locally

The `PopForums.Web` project is the host application. Key setup steps:

1. Set the connection string in `appsettings.json` under `PopForums:Database:ConnectionString` (default looks for a local SQL Server DB named `popforums21`)
2. First run: navigate to `/Forums/Setup` to initialize the database and admin account (don't run the SQL script manually before this)
3. Background jobs: by default `Program.cs` uses `AddPopForumsAzureFunctionsAndQueues()`. For local development without Azure, switch to `AddPopForumsBackgroundJobs()` (in-process)

### Docker services for local dev
```bash
# SQL Server (ARM: azure-sql-edge; x86: mssql/server:2022-latest)
docker run --cap-add SYS_PTRACE -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=P@ssw0rd' -p 1433:1433 --name sqledge -d mcr.microsoft.com/azure-sql-edge

# Azurite (storage + queues)
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite

# Redis (distributed cache / SignalR backplane)
docker run --name some-redis -p 6379:6379 -d redis

# ElasticSearch
docker run --name es-9 -p 9200:9200 -e discovery.type=single-node -it docker.elastic.co/elasticsearch/elasticsearch:9.3.0
```

## Architecture Patterns

### Layered dependency direction
`PopForums.Mvc` → `PopForums` (interfaces) ← `PopForums.Sql` (implementations)

- `PopForums` defines all repository interfaces (`IForumRepository`, `IPostRepository`, etc.) in `Repositories/` and service classes in `Services/`
- `PopForums.Sql` implements those repository interfaces with SQL Server + Dapper-style access
- Services depend on repository interfaces; they never touch SQL directly
- `PopForums.AzureKit` and `PopForums.ElasticKit` swap in alternative implementations via DI extension methods

### DI registration pattern
Each library exposes extension methods on `IServiceCollection`:
- `services.AddPopForumsSql()` — registers SQL repos and in-memory cache
- `services.AddPopForumsMvc()` — registers MVC services, auth, and base forum services
- `services.AddPopForumsRedisCache()` — overrides cache with Redis two-level cache
- `services.AddPopForumsElasticSearch()` / `services.AddPopForumsAzureSearch()` — override search
- `services.AddPopForumsAzureFunctionsAndQueues()` — routes background work to Azure queues
- `services.AddPopForumsBackgroundJobs()` — runs background jobs in-process (local dev)

### MVC Area
All forum routes are under the `Forums` area. Controllers live in `src/PopForums.Mvc/Areas/Forums/Controllers/`. The area is mapped via `app.AddPopForumsEndpoints()`.

### Background jobs
Background tasks (email, search indexing, award calculation, session cleanup, etc.) are implemented as `BackgroundService` derivatives. In production, these run as Azure Functions via `PopForums.AzureKit.Functions`. In-process mode is available for single-node or local use.

### Front-end
- No SPA framework for the main forum UI — raw TypeScript components in `Client/Components/`
- Components extend `ElementBase.ts` and use a simple state engine in `State/`
- SignalR connects on page load; components react to hub messages for real-time updates
- Vue.js + Vue Router are used **only** for the admin interface
- Localization on the client side uses a JSON payload from the server; see `FormattedTime.ts` for an example

### Testing
- Tests are in `PopForums.Test`, mirroring the folder structure of the projects under test
- Mocking via NSubstitute; test framework is xUnit
- Tests focus on service layer; controller and repository coverage is minimal

## Database

- Initial schema: `src/PopForums.Sql/PopForums.sql`
- Migration scripts follow the pattern `PopForumsXXtoYY.sql` in `src/PopForums.Sql/`
- From v21.x to v22.x, run `PopForums21to22.sql`
- Statistics (post counts, etc.) are precomputed at write time rather than aggregated at query time
