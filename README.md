# TaskBoard

TaskBoard is a project management app with a React frontend and an ASP.NET Core
Web API. Organize work into projects and Kanban-style boards, assign tasks,
track workflow status, and collaborate with comments and activity history.

## Live app

[Open TaskBoard](https://task-board-gilt-nine.vercel.app/)

## Features

- Register and sign in with JWT bearer tokens.
- Create projects and manage project members and roles.
- Create, rename, reorder, and remove board columns.
- Create, update, move, filter, and delete tasks.
- Track task status: `Started`, `In Progress`, `On Hold`, or `Completed`.
- Set task priority, due date, and assignee.
- Add comments and view project or task activity.
- Explore the API with Swagger UI at the API root.
- Use a responsive React single-page app for sign-in, projects, boards, task
  discussions, and project members.

## Technology

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL with Npgsql
- JWT bearer authentication
- Docker multi-stage build
- React 19, TypeScript, and Vite 8 frontend

## Architecture

The frontend uses page and component modules, an authentication context, and a
shared API client that attaches JWT bearer tokens to protected requests. HTTP
requests flow through backend controllers, wrapper services, logic services,
and endpoint services. Controllers handle HTTP concerns; logic services enforce
project membership and task rules; endpoint services query and update the
database through the request-scoped `AppDbContext`.

```text
React pages/components → AuthContext/API client → ASP.NET Core API
                                                → Controllers
                                                → WrapperService
                                                → LogicService
                                                → EndpointService → EF Core → PostgreSQL
```

### Frontend logic

- `src/main.tsx` mounts the app inside React Strict Mode, React Router, and the
  authentication provider.
- `src/App.tsx` defines public landing, login, and registration routes. The
  `/projects` and `/projects/:id` routes are protected by `ProtectedRoute`.
- `src/context/AuthContext.tsx` restores the signed-in user through
  `/api/Auth/me`, manages login/logout, and clears the session after an API
  `401` response. The bearer token is kept in `sessionStorage` for the current
  browser tab.
- `src/services/api.ts` is the shared HTTP client. It reads
  `VITE_API_BASE_URL`, attaches the bearer token, validates API-relative paths,
  parses JSON responses, and surfaces API errors. HTTP is allowed only for
  localhost; remote API URLs must use HTTPS.
- `src/pages/ProjectsPage.tsx` handles project listing and project create/edit
  flows. `src/pages/ProjectDetails.tsx` loads the project, board, and members,
  then coordinates column and task actions.
- Reusable components handle task forms, project members, task discussions, and
  column activity. `src/utils/taskStatus.ts` normalizes legacy status aliases
  and exposes the canonical `Started`, `In Progress`, `On Hold`, and `Completed`
  options used by the board and task form.

The frontend source is under `frontend/src`; configuration and npm scripts are
in `frontend/package.json`.

## Hosting architecture

TaskBoard uses separate hosting for the application and its database:

```text
Static frontend host ──serves──▶ User's browser
                                  │ HTTPS API requests (JWT bearer token)
                                  ▼
                         Azure Container Apps
                         ASP.NET Core API :8080
                                  │ PostgreSQL over TLS
                                  ▼
                         Supabase PostgreSQL
```

Supabase hosts the PostgreSQL database and stores application data. It does not
run the ASP.NET Core API. Azure Container Apps runs the API container; the API
is the only application component that should connect to the database. The
browser communicates with the API and must never receive the database
connection string.

### Azure API configuration

Deploy the Docker image built from the repository root to Azure Container Apps
with ingress targeting container port `8080`. Configure the following settings
in the Container App, using secret references for credentials:

- `ConnectionStrings__DefaultConnection`: the Supabase PostgreSQL connection
  string. Keep TLS enabled and use a Supabase connection endpoint compatible
  with the Container Apps network path.
- `Jwt__SecretKey`: a strong signing secret shared by the API instances.
- `Jwt__Issuer` and `Jwt__Audience`: must match the token issuer and audience
  expected by this API.
- `Cors__AllowedOrigins__0`: the exact HTTPS origin serving the frontend. Add
  further origins with `Cors__AllowedOrigins__1`, etc., only when needed.

Keep production values in Azure secrets and environment configuration, not in
`appsettings.json`, source code, or frontend variables. Only the public API URL
belongs in the frontend's `VITE_API_BASE_URL` build setting.

### Database release order

Apply EF Core migrations to Supabase once before deploying an API version that
requires the new schema. Then deploy the API image and confirm the Container App
is healthy before publishing the matching frontend build. The migration command
is in [Database migrations](#database-migrations); do not run it independently
from every API replica.

## Run locally

Prerequisites: .NET 10 SDK, Node.js with npm, and a PostgreSQL database.

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

In a second terminal, configure and start the frontend:

```powershell
cd frontend
Copy-Item .env.example .env.local
npm install
npm run dev
```

The frontend runs at `http://localhost:5173` and reads its API URL from
`VITE_API_BASE_URL`. `.env.local` is ignored by Git; change the example value if
your local API uses a different URL. For a production frontend, set
`VITE_API_BASE_URL` to the deployed HTTPS API URL at build time.

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

The image builds and runs the backend API, which listens on port `8080`. Build
the frontend separately from `frontend/` with `npm run build` and deploy its
static `dist/` output to a static web host. Supply database and JWT values
through the API container platform's secret configuration, and set the allowed
frontend origin there as well.
