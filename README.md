# TaskBoard API

TaskBoard is an ASP.NET Core Web API for project workspaces and Kanban-style
boards. It supports project membership, task assignment and priorities, due
dates, workflow statuses, comments, and activity history. The API uses JWT
bearer authentication and PostgreSQL persistence through Entity Framework Core.

## Features

- Register and sign in with JWT bearer tokens.
- Create projects and manage project members and roles.
- Create, rename, reorder, and remove board columns.
- Create, update, move, filter, and delete tasks.
- Track task status: `Started`, `In Progress`, `On Hold`, or `Completed`.
- Set task priority, due date, and assignee.
- Add comments and view project or task activity.
- Explore the API with Swagger UI at the API root.

## Technology

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL with Npgsql
- JWT bearer authentication
- Docker multi-stage build

## Architecture

HTTP requests flow through controllers, wrapper services, logic services, and
endpoint services. Controllers handle HTTP concerns; logic services enforce
project membership and task rules; endpoint services query and update the
database through the request-scoped `AppDbContext`.

```text
HTTP client → Controllers → WrapperService → LogicService
                                      → EndpointService → EF Core → PostgreSQL
```

## Run locally

Prerequisites: .NET 10 SDK and a PostgreSQL database.

From the repository root, configure local secrets from the `TaskBoard` project
directory:

```powershell
cd TaskBoard
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=<host>;Port=5432;Database=<database>;Username=<username>;Password=<password>;SSL Mode=Require"
dotnet user-secrets set "Jwt:SecretKey" "<random secret of at least 32 bytes>"
cd ..
```

The JWT issuer and audience default to `TaskBoard` and `TaskBoardClient`. They
can be overridden through `Jwt:Issuer` and `Jwt:Audience` configuration.

Apply database migrations, then run the API:

```powershell
dotnet ef database update --project TaskBoard\TaskBoard.csproj
dotnet run --project TaskBoard\TaskBoard.csproj
```

In Development, Swagger UI is available at the API root. The HTTP and HTTPS
development URLs are configured in `TaskBoard/Properties/launchSettings.json`.

## Configuration

Credentials are not stored in `appsettings.json`. Development uses .NET User
Secrets; deployed environments should use secret-backed environment variables.

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string |
| `Jwt:SecretKey` | JWT signing key; use a cryptographically random key of at least 32 bytes |
| `Jwt:Issuer` | Expected JWT issuer; defaults to `TaskBoard` |
| `Jwt:Audience` | Expected JWT audience; defaults to `TaskBoardClient` |
| `Cors:AllowedOrigins` | Exact frontend origins permitted to call the API |

For Azure Container Apps, use secret-backed environment variables such as
`ConnectionStrings__DefaultConnection` and `Jwt__SecretKey`. Configure the
frontend origin with `Cors__AllowedOrigins__0`; additional origins can use
`Cors__AllowedOrigins__1`, and so on. Local development defaults to
`http://localhost:5173`.

## API overview

All routes except registration and login require a valid bearer token.

| Area | Routes |
| --- | --- |
| Authentication | `POST /api/Auth/register`, `POST /api/Auth/login`, `GET /api/Auth/me` |
| Projects | `GET /api/Projects`, `POST /api/Projects`, `GET/PUT/DELETE /api/Projects/{id}` |
| Project members | `GET/POST /api/Projects/{id}/members`, `PUT/DELETE /api/Projects/{id}/members/{userId}` |
| Board | `GET /api/Board/{id}/board`, `POST /api/Board/{id}/columns` |
| Columns | `PUT /api/columns/{id}`, `PUT /api/Board/{id}/columns/reorder`, `DELETE /api/columns/{id}` |
| Tasks | `GET /api/Tasks/projects/{id}/tasks`, `POST /api/Tasks/columns/{columnId}/tasks`, `GET/PUT/DELETE /api/Tasks/tasks/{id}`, `PUT /api/Tasks/tasks/{id}/move` |
| Comments | `GET/POST /api/Comments/tasks/{id}/comments`, `PUT/DELETE /api/Comments/comments/{id}` |
| Activity | `GET /api/Activity/tasks/{id}/activity`, `GET /api/Activity/projects/{id}/activity` |

Use Swagger UI for request schemas, query filters, and response details.

## Database migrations

Apply migrations once per database as a release step before deploying code that
depends on the updated schema:

```powershell
dotnet ef database update --project TaskBoard\TaskBoard.csproj
```

Avoid applying migrations independently from every application replica during
startup.

## Docker

Build the container from the repository root:

```powershell
docker build -t taskboard-api .
```

The image listens on port `8080`. Supply the required database and JWT values
through your container platform's secret configuration, and set the allowed
frontend origin there as well.
