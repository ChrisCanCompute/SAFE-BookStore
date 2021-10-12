FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
COPY /cleanerDeploy .
WORKDIR .
ENTRYPOINT ["dotnet", "Cleaner.dll", "PostgresConnection=Host=book-store-db;Username=postgresadmin;Password=admin123;Database=postgresdb"]
