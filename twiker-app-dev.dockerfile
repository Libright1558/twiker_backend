# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy project file, restore and build app
COPY Main ./Main
RUN dotnet restore Main/Main-solution.sln
RUN dotnet publish Main/Main-solution.sln -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /source
COPY --from=build /source/out .
COPY KeyPair ./KeyPair
COPY .env ./.env
ENV ASPNETCORE_ENVIRONMENT Development
ENTRYPOINT ["dotnet", "twiker_backend.dll", "--environment=Development"]
