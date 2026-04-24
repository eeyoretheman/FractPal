# FractPal 🌀

**FractPal** is a social media platform built around fractals — a place where users can share, discover, and appreciate fractal art. Think of it as a community gallery for the mathematically beautiful.

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | C# / .NET (ASP.NET Core) |
| Frontend | TypeScript / React |
| Database | Microsoft SQL Server (MSSQL) |
| Auth | JWT |
| Infrastructure | Docker, Docker Compose, nginx |

## Getting Started

### Prerequisites

- [Docker](https://www.docker.com/) and Docker Compose installed
- .NET SDK (for running migrations locally)

### Running the App

Use the provided convenience scripts to build and start all services:

**Linux / macOS**
```bash
./run.sh
```

**Windows**
```powershell
./run.ps1
```

> **Note for Windows users:** Setting environment variables and running Docker commands manually is still the recommended approach. The `.ps1` script may not handle all edge cases.

### Environment Variables

Copy `.env.example` to `.env.development` and fill in your values:

```bash
cp .env.example .env.development
```

Key variables:

| Variable | Description |
|---|---|
| `SA_PASSWORD` | SQL Server SA password |
| `DATABASE_CONNECTION_STRING` | Full MSSQL connection string |
| `JWT_SECRET_KEY` | Secret key for signing JWTs (min. 32 chars) |
| `JWT_ISSUER` | JWT issuer (e.g. `FractPal`) |
| `JWT_AUDIENCE` | JWT audience (e.g. `FractPal`) |
| `JWT_EXPIRY_MINUTES` | Token lifetime in minutes |

## Database Migrations

### Adding a new migration

```bash
cd backend
dotnet ef migrations add "<MIGRATION NAME>" \
    --project ./FractPal.Data \
    --startup-project ./FractPal.API
```

### Applying migrations to the database

```bash
# Start the database container
docker compose up mssql -d

# Load environment variables
set -a
source .env.development
set +a

# Run the migration
cd backend
dotnet ef database update \
    --project ./FractPal.Data \
    --startup-project ./FractPal.API
```

> **Windows note:** The `source` command may not work in PowerShell. Set the environment variables manually or use a tool like `dotenv`.

## Project Structure

```
FractPal/
├── backend/          # ASP.NET Core API
├── frontend/         # React + TypeScript app
│   └── fractpal/
├── docs/             # Documentation
├── compose.yml       # Docker Compose config
├── nginx.conf        # nginx reverse proxy config
├── Dockerfile.backend
├── Dockerfile.frontend
├── run.sh            # Linux/macOS startup script
└── run.ps1           # Windows startup script
```
