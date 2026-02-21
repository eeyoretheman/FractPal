#!/usr/bin/env sh

set -e

echo "Copying environment files..."

cp -f .env.example .env
cp -f .env.example .env.development

echo "Updating .env.development..."
sed -i.bak 's/mssql,1433/localhost,1433/g' .env.development
rm -f .env.development.bak

echo "Starting MSSQL container..."
docker compose up -d mssql

echo "Loading environment variables from .env.development..."
set -a
. ./.env.development
set +a

echo "Running EF Core database update..."
dotnet ef database update \
    --project ./backend/FractPal.Data \
    --startup-project ./backend/FractPal.API

echo "Starting full Docker environment..."
docker compose up --build

echo "Done."
