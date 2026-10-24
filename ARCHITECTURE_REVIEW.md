# TaskBoard architecture review

## Scope

Review of the ASP.NET Core API, its controller/logic/endpoint-service layers,
EF Core configuration, and the recent task status and board-loading changes.

## Changes made

- Protected board, task, project, comment, and activity controllers with
  `[Authorize]`. Login and registration remain public. Action-level identity
  checks remain as a defensive guard for malformed or missing user claims.
- Made CORS origins configuration-driven. Development defaults to
  `http://localhost:5173`; deployed origins can be configured through
  `Cors__AllowedOrigins__0`. Routing runs before CORS, which runs before
  authentication and authorization.
- Removed duplicate Swagger/OpenAPI service registrations.
- Removed the database connection string and JWT signing key from tracked
  `appsettings.json`. Added a project User Secrets ID and local/Azure
  configuration guidance in `README.md`.
- Kept transient PostgreSQL retries configured and documented schema migration
  as a deployment step rather than a per-replica startup action.

## Remaining actions

### High: rotate exposed credentials

The previous database password and JWT signing key were present in a tracked
configuration file. Removing them from the current file does not remove them
from Git history. Rotate the Supabase database password and JWT signing key,
update local User Secrets and Azure Container Apps secrets, and invalidate
existing tokens signed by the old key. Do not commit replacement values.

### Medium: keep schema and application releases in sync

The missing `TaskItems.Status` column showed that application code can reach a
database before its migration is applied. The migration was applied to the
configured database during this task. Keep migration application as a
deployment step before routing traffic to a release that depends on the new
schema.

### Medium: reduce repeated task status rules

Task status is represented as a string with allowed values repeated across
request validation and business logic. If statuses change, centralize the
allowed values and validation so API validation, persistence, and UI contracts
cannot drift.

## Architecture

The current controller → wrapper → logic → endpoint-service split is small and
understandable. Scoped `AppDbContext` usage is appropriate for a request. The
board read path awaits project, membership, and board queries sequentially; it
does not issue parallel EF operations on one context.
