# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["CanPany-BE/CanPany.Api/CanPany.Api.csproj", "CanPany.Api/"]
COPY ["CanPany-BE/CanPany.Application/CanPany.Application.csproj", "CanPany.Application/"]
COPY ["CanPany-BE/CanPany.Infrastructure/CanPany.Infrastructure.csproj", "CanPany.Infrastructure/"]
COPY ["CanPany-BE/CanPany.Domain/CanPany.Domain.csproj", "CanPany.Domain/"]
COPY ["CanPany-BE/CanPany.Shared/CanPany.Shared.csproj", "CanPany.Shared/"]

RUN dotnet restore "CanPany.Api/CanPany.Api.csproj"

# Copy full source and build
COPY CanPany-BE/ .
WORKDIR "/src/CanPany.Api"
RUN dotnet build "CanPany.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "CanPany.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

EXPOSE 10000
ENV ASPNETCORE_URLS=http://+:10000

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "CanPany.Api.dll"]
