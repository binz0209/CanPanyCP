# CanPany Backend

AI-powered Recruitment Platform - Backend API

## Kiến trúc

Dự án sử dụng **Clean Architecture** với các layers:

- **CanPany.Domain**: Domain entities, interfaces, enums
- **CanPany.Application**: Business logic, Commands/Queries (CQRS), DTOs
- **CanPany.Infrastructure**: Data access, external services, security
- **CanPany.Api**: Controllers, middleware, configuration
- **CanPany.Shared**: Common utilities, base classes, results
- **CanPany.Tests**: Unit tests, integration tests

## Công nghệ

- **.NET 8.0**: Framework chính
- **MongoDB**: Database chính
- **Redis**: Caching, sessions, job queue
- **JWT**: Authentication
- **BCrypt**: Password hashing
- **AES-256**: Data encryption
- **Serilog**: Logging
- **Swagger/OpenAPI**: API documentation
- **MediatR**: CQRS pattern
- **FluentValidation**: Input validation
- **AutoMapper**: Object mapping

## Cài đặt

### Yêu cầu

- .NET 8.0 SDK
- MongoDB (local hoặc cloud)
- Redis (optional, cho caching)

### Setup

1. Clone repository
2. Restore packages:
```bash
dotnet restore
```

3. Cấu hình `appsettings.json`:
   - MongoDB connection string
   - JWT secret key
   - Encryption key
   - External service keys (Cloudinary, Google Gemini, SePay)

4. Chạy project:
```bash
dotnet run --project CanPany.Api
```

5. Truy cập Swagger UI: `https://localhost:5001/swagger`

## Cấu trúc thư mục

```
CanPany-BE/
├── CanPany.Domain/
│   ├── Entities/          # Domain entities
│   ├── Enums/            # Enumerations
│   └── Interfaces/       # Repository interfaces
├── CanPany.Application/
│   ├── Commands/         # CQRS Commands
│   ├── Queries/          # CQRS Queries
│   ├── DTOs/            # Data Transfer Objects
│   └── Services/        # Application services
├── CanPany.Infrastructure/
│   ├── Data/            # MongoDB context
│   ├── Repositories/    # Repository implementations
│   └── Security/        # Encryption, hashing
├── CanPany.Api/
│   ├── Controllers/     # API controllers
│   └── Program.cs       # Startup configuration
├── CanPany.Shared/
│   ├── Common/          # Base classes, extensions
│   └── Results/         # Result pattern
└── CanPany.Tests/       # Test projects
```

## Tính năng chính

- ✅ User authentication & authorization
- ✅ Job management
- ✅ CV management
- ✅ Application management
- ✅ Company management
- ✅ AES-256 encryption
- ✅ Password hashing (BCrypt)
- ✅ MongoDB repositories
- ✅ Health check endpoint

## Đang phát triển

- [ ] JWT authentication
- [ ] AI integration (Google Gemini)
- [ ] File upload (Cloudinary)
- [ ] Payment integration (SePay)
- [ ] Real-time messaging (SignalR)
- [ ] Vector search
- [ ] Background jobs
- [ ] I18N support

## Testing

```bash
dotnet test
```

## License

Copyright © 2024 CanPany

