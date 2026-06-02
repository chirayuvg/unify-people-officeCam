# Unify People OfficeCam

Unify People OfficeCam is a .NET solution for people operations, attendance, visitor management, camera-assisted workflows, notifications, and reporting.

## Projects

- `Unify.People` - ASP.NET Core people management app for attendance, visitors, leave, regularization, dashboards, and reports.
- `Unify.CCC` - camera/control-center related services.
- `Unify.STR.Attendnace` - attendance-focused services and APIs.
- `UnifyNow.Forest` - forest/WhatsApp notification and messaging workflows.
- `Toll.Web.UI` - supporting web UI project.
- `AppHost` - local orchestration host.

## Requirements

- .NET 9 SDK
- PostgreSQL and/or SQL Server, depending on the project being run
- Node.js for WhatsApp/Node integration workflows
- Docker, optional for containerized runs

## Configuration

Sensitive values are intentionally not committed. Replace placeholders through environment variables, .NET user secrets, deployment secrets, or private local config.

Common required values include:

- Database connection strings
- RabbitMQ credentials
- JWT signing secret
- Firebase service account credentials
- Meta/WhatsApp access token and phone number ID
- Elasticsearch credentials and fingerprint
- AI/face service URLs

Do not commit real credentials, local WhatsApp session folders, QR images, uploaded media, logs, or service account JSON files.

## Run

Restore and build from the solution root:

```powershell
dotnet restore Unify.People.sln
dotnet build Unify.People.sln
```

Run a specific web project:

```powershell
dotnet run --project Unify.People/Unify.People.csproj
```

## Repository

GitHub: `chirayuvg/unify-people-officeCam`
