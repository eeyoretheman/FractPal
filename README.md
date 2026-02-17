# FractPal

Running migrations:
```
cd backend
dotnet ef migrations add "<MIGRATION NAME>" \
    --project ./FractPal.Data \
    --startup-project ./FractPal.API
```

Updating the database

```
docker compose up mssql -d
set -a                  # Ensuring env vars are set properly
source .env.development # Might not work on Windows, will look into it
set +a                  #
cd backend
dotnet ef database update \
    --project ./FractPal.Data \
    --startup-project ./FractPal.API

# On Windows do this:
$Env:SA_PASSWORD="Str0ng!Passw0rd2026"
$Env:DATABASE_CONNECTION_STRING="Server=localhost,1433;Database=FractPalDb;User Id=sa;Password=Str0ng!Passw0rd2026;TrustServerCertificate=True;"
$Env:JWT_SECRET_KEY="YourSuperSecretKeyThatIsAtLeast32CharactersLong!"
$Env:JWT_ISSUER="FractPal"
$Env:JWT_AUDIENCE="FractPal"
$Env:JWT_EXPIRY_MINUTES="1440"

```
