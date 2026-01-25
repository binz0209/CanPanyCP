# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY ["CanPany.Worker/CanPany.Worker.csproj", "CanPany.Worker/"]
COPY ["CanPany.Application/CanPany.Application.csproj", "CanPany.Application/"]
COPY ["CanPany.Infrastructure/CanPany.Infrastructure.csproj", "CanPany.Infrastructure/"]
COPY ["CanPany.Domain/CanPany.Domain.csproj", "CanPany.Domain/"]
COPY ["CanPany.Shared/CanPany.Shared.csproj", "CanPany.Shared/"]

RUN dotnet restore "CanPany.Worker/CanPany.Worker.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/CanPany.Worker"
RUN dotnet build "CanPany.Worker.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "CanPany.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN groupadd -r worker && useradd -r -g worker worker
USER worker

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD echo "OK" || exit 1

ENTRYPOINT ["dotnet", "CanPany.Worker.dll"]
