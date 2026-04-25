# ── Stage 1: Build API ────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-api
WORKDIR /src

COPY ["CanPany.Api/CanPany.Api.csproj", "CanPany.Api/"]
COPY ["CanPany.Application/CanPany.Application.csproj", "CanPany.Application/"]
COPY ["CanPany.Infrastructure/CanPany.Infrastructure.csproj", "CanPany.Infrastructure/"]
COPY ["CanPany.Domain/CanPany.Domain.csproj", "CanPany.Domain/"]
COPY ["CanPany.Shared/CanPany.Shared.csproj", "CanPany.Shared/"]

RUN dotnet restore "CanPany.Api/CanPany.Api.csproj"

COPY . .
WORKDIR "/src/CanPany.Api"
RUN dotnet publish "CanPany.Api.csproj" -c Release -o /app/api /p:UseAppHost=false

# ── Stage 2: Build Worker ─────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-worker
WORKDIR /src

COPY ["CanPany.Worker/CanPany.Worker.csproj", "CanPany.Worker/"]
COPY ["CanPany.Application/CanPany.Application.csproj", "CanPany.Application/"]
COPY ["CanPany.Infrastructure/CanPany.Infrastructure.csproj", "CanPany.Infrastructure/"]
COPY ["CanPany.Domain/CanPany.Domain.csproj", "CanPany.Domain/"]
COPY ["CanPany.Shared/CanPany.Shared.csproj", "CanPany.Shared/"]

RUN dotnet restore "CanPany.Worker/CanPany.Worker.csproj"

COPY . .
WORKDIR "/src/CanPany.Worker"
RUN dotnet publish "CanPany.Worker.csproj" -c Release -o /app/worker /p:UseAppHost=false

# ── Stage 3: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV DOTNET_hostBuilder__reloadConfigOnChange=false
ENV ASPNETCORE_URLS=http://+:10000

COPY --from=build-api /app/api ./api
COPY --from=build-worker /app/worker ./worker
COPY start.sh .
RUN chmod +x start.sh

EXPOSE 10000

ENTRYPOINT ["/bin/bash", "/app/start.sh"]
