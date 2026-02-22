# FractPal

Use the provided scripts:
- 'run.ps1' for Windows
- 'run.sh' for Linux and other Unix-like OSs

Setting the environment variables and running the docker commands manually
is still the recommended approach for Windows users. 

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
```
