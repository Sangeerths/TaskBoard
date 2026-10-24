# TaskBoard

## Local development configuration

Runtime credentials are intentionally kept out of `appsettings.json`.
The project uses .NET User Secrets in Development and environment variables in
deployed environments.

Set the local database connection string and JWT signing key from the
`TaskBoard` directory:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=<host>;Port=5432;Database=<database>;Username=<username>;Password=<password>;SSL Mode=Require"
dotnet user-secrets set "Jwt:SecretKey" "<random secret of at least 32 bytes>"
```

For Azure Container Apps, configure `ConnectionStrings__DefaultConnection` and
`Jwt__SecretKey` as secret-backed environment variables. Set the frontend origin
in `Cors__AllowedOrigins__0`; for local Vite development it defaults to
`http://localhost:5173`.

## Database migrations

Apply EF Core migrations from the repository root as a deployment step with:

```powershell
dotnet ef database update --project TaskBoard\TaskBoard.csproj
```

Run this once per database before deploying application code that depends on a
new schema. Avoid applying migrations independently from every application
replica at startup.
