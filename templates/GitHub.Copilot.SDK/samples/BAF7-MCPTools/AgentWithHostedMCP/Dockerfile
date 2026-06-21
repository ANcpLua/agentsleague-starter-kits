# Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine@sha256:fac7cce841f78faa4bca416fb4c636d1a129c09abd9b50e9b45664b95fd008a0 AS build
WORKDIR /src

# Copy files from the current directory on the host to the working directory in the container
COPY . .

RUN dotnet restore
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release --no-build -o /app -f net10.0

# Run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine@sha256:f03685b2735e0d3d25d6c60672e74b21bb6334f1402f71bae2d2cf02307163cd AS final
WORKDIR /app

# Copy everything needed to run the app from the "build" stage.
COPY --from=build /app .

EXPOSE 8088
ENTRYPOINT ["dotnet", "AgentWithHostedMCP.dll"]
