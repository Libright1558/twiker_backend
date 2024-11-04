# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy project file, restore and build app
COPY Main ./Main
COPY Test ./Test
COPY KeyPair ./KeyPair
COPY .env ./.env
COPY twiker_backend.sln ./twiker_backend.sln
RUN dotnet restore
RUN dotnet build --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /source
COPY --from=build /source/* .
WORKDIR /source/Main
ENTRYPOINT ["dotnet", "twiker_backend.dll"]
