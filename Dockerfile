# ── Stage 1: Build API ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-api
WORKDIR /src

COPY ["CanPany-BE/CanPany.Api/CanPany.Api.csproj", "CanPany.Api/"]
COPY ["CanPany-BE/CanPany.Application/CanPany.Application.csproj", "CanPany.Application/"]
COPY ["CanPany-BE/CanPany.Infrastructure/CanPany.Infrastructure.csproj", "CanPany.Infrastructure/"]
COPY ["CanPany-BE/CanPany.Domain/CanPany.Domain.csproj", "CanPany.Domain/"]
COPY ["CanPany-BE/CanPany.Shared/CanPany.Shared.csproj", "CanPany.Shared/"]

RUN dotnet restore "CanPany.Api/CanPany.Api.csproj"

COPY CanPany-BE/ .
WORKDIR "/src/CanPany.Api"
RUN dotnet publish "CanPany.Api.csproj" -c Release -o /app/api /p:UseAppHost=false /p:ErrorOnDuplicatePublishOutputFiles=false

# ── Stage 2: Build Worker ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-worker
WORKDIR /src

COPY ["CanPany-BE/CanPany.Worker/CanPany.Worker.csproj", "CanPany.Worker/"]
COPY ["CanPany-BE/CanPany.Application/CanPany.Application.csproj", "CanPany.Application/"]
COPY ["CanPany-BE/CanPany.Infrastructure/CanPany.Infrastructure.csproj", "CanPany.Infrastructure/"]
COPY ["CanPany-BE/CanPany.Domain/CanPany.Domain.csproj", "CanPany.Domain/"]
COPY ["CanPany-BE/CanPany.Shared/CanPany.Shared.csproj", "CanPany.Shared/"]

RUN dotnet restore "CanPany.Worker/CanPany.Worker.csproj"

COPY CanPany-BE/ .
WORKDIR "/src/CanPany.Worker"
RUN dotnet publish "CanPany.Worker.csproj" -c Release -o /app/worker /p:UseAppHost=false

# ── Stage 3: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Disable config file watching to avoid inotify limit on Render
ENV DOTNET_hostBuilder__reloadConfigOnChange=false
ENV ASPNETCORE_URLS=http://+:10000

# Copy both published apps
COPY --from=build-api /app/api ./api
COPY --from=build-worker /app/worker ./worker

# Copy and prepare startup script
COPY start.sh .
RUN chmod +x start.sh

EXPOSE 10000

ENTRYPOINT ["/bin/bash", "/app/start.sh"]
