FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props MenuMate.slnx ./
COPY src ./src
RUN dotnet restore src/MenuMate.Migrator/MenuMate.Migrator.csproj
RUN dotnet publish src/MenuMate.Migrator/MenuMate.Migrator.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MenuMate.Migrator.dll"]

