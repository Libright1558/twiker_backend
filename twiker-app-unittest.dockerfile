# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy project file, restore and build app
COPY Main ./Main
COPY Test ./Test
COPY twiker_backend.sln ./twiker_backend.sln
COPY KeyPair ./KeyPair
COPY .env ./.env
RUN dotnet restore 
RUN dotnet build --no-restore

# Runtime stage
WORKDIR /source/Main
ENTRYPOINT ["dotnet", "run", "--no-restore", "--no-build"]