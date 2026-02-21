Write-Host "Copying environment files..."

Copy-Item -Path ".env.example" -Destination ".env" -Force
Copy-Item -Path ".env.example" -Destination ".env.development" -Force

Write-Host "Updating .env.development..."

(Get-Content ".env.development") `
    -replace "mssql,1433", "localhost,1433" `
    | Set-Content ".env.development"

Write-Host "Starting MSSQL container..."
docker compose up -d mssql

Write-Host "Loading environment variables from .env.development..."

Get-Content ".env.development" | ForEach-Object {
    if ($_ -match "^\s*([^#][^=]*)=(.*)$") {
        $name = $matches[1]
        $value = $matches[2]
        [System.Environment]::SetEnvironmentVariable($name, $value)
    }
}

Write-Host "Running EF Core database update..."
dotnet ef database update `
    --project ./backend/FractPal.Data `
    --startup-project ./backend/FractPal.API

Write-Host "Starting full Docker environment..."
docker compose up --build

Write-Host "Done."
