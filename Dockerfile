FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS runtime
COPY /deploy .
WORKDIR .
EXPOSE 8085
ENTRYPOINT ["dotnet", "Server.dll", "PostgresConnection=Host=book-store-db;Username=postgresadmin;Password=admin123;Database=postgresdb"]
