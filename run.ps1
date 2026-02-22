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
    if ($_ -match "^\s*([^#=][^=]*?)\s*=\s*`"?(.*?)`"?\s*$") {
        $name = $matches[1].Trim()
        $value = $matches[2].Trim()
        [System.Environment]::SetEnvironmentVariable($name, $value)
    }
}

Write-Host "Waiting for MSSQL to be healthy..."
$timeout = 60
$elapsed = 0
do {
    Start-Sleep -Seconds 2
    $elapsed += 2
    $health = docker inspect --format "{{.State.Health.Status}}" (docker compose ps -q mssql) 2>$null
    Write-Host "  MSSQL status: $health ($elapsed`s elapsed)"
    if ($elapsed -ge $timeout) {
        Write-Error "Timed out waiting for MSSQL to become healthy."
        exit 1
    }
} while ($health -ne "healthy")

Write-Host "Running EF Core database update..."
dotnet ef database update `
    --project ./backend/FractPal.Data `
    --startup-project ./backend/FractPal.API

Write-Host "Starting full Docker environment..."
docker compose up --build

Write-Host "Done."