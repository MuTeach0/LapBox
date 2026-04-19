# LapBox

> Single-vendor laptop e-commerce platform — built with .NET 10, Clean Architecture, and CQRS.

---

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Environment Variables](#environment-variables)
- [Running the Project](#running-the-project)
- [Running Tests](#running-tests)
- [API Documentation](#api-documentation)
- [Database](#database)
- [Redis / Basket](#redis--basket)
- [Modules](#modules)
- [SDLC Documentation](#sdlc-documentation)
- [Team](#team)

---

## Overview

LapBox is a production-grade B2C e-commerce API for selling laptops online. It is a **single-vendor** store — one seller, many customers.

**Key features:**
- Product catalogue with categories, variants (RAM / Storage), images, and tags
- Redis-backed basket (no SQL table)
- Atomic checkout with stock race-condition protection via SQL `UPDLOCK`
- Payment integration via Paymob with HMAC webhook verification
- JWT authentication with role separation (`Admin` / `Customer`)
- Product reviews (verified purchase only), wishlist, and coupon system
- Admin panel: inventory, orders, coupons, and sales dashboard
- All errors follow RFC 7807 ProblemDetails — no raw exceptions ever reach the client

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Web | ASP.NET Core 10 |
| ORM | Entity Framework Core 10 |
| Database | SQL Server |
| Cache / Basket | Redis (StackExchange.Redis 2.x) |
| CQRS | MediatR 12 |
| Validation | FluentValidation 11 |
| Mapping | Mapster 7 |
| Auth | ASP.NET Core Identity 10 + JWT Bearer |
| Logging | Serilog |
| API Docs | Scalar (OpenAPI 3.1) |
| Testing | xUnit 2 + NSubstitute + FluentAssertions |

---

## Architecture

Clean Architecture with CQRS and a modular folder structure inside a monolith.

```
LapBox.API           ← HTTP layer (controllers, middleware, DI wiring)
LapBox.Application   ← Use cases (commands + queries), DTOs, validators, interfaces
LapBox.Infrastructure← EF Core, Redis, Paymob, Identity, Email
LapBox.Domain        ← Entities, value objects, Result pattern, Error types
```

**Dependency rule:**
```
API → Application → Domain
Infrastructure → Application (implements interfaces defined there)
```

Domain and Application have **zero** references to EF Core, Identity, or any external library.

---

## Project Structure

```
LapBox.sln
├── src/
│   ├── LapBox.Domain/
│   │   ├── Entities/
│   │   │   ├── Catalog/        Product, Category, ProductVariant, ProductImage, ProductTag
│   │   │   ├── Orders/         Order, OrderItem, OrderStatus (enum)
│   │   │   ├── Customers/      Customer, CustomerAddress, CustomerPhone
│   │   │   ├── Auth/           AppUser
│   │   │   ├── Payments/       Payment
│   │   │   ├── Reviews/        Review
│   │   │   ├── Promotions/     Coupon
│   │   │   └── Shared/         Supplier, Shipper
│   │   ├── ValueObjects/       Money
│   │   └── Common/             Result<T>, Error
│   │
│   ├── LapBox.Application/
│   │   ├── Auth/               Commands: Register, Login, Refresh, Logout
│   │   ├── Catalog/            Commands: CreateProduct, UpdateProduct, DeleteProduct
│   │   │                       Queries:  GetProducts, GetProductById
│   │   ├── Basket/             Commands: AddItem, UpdateItem, RemoveItem, ClearBasket
│   │   │                       Queries:  GetBasket
│   │   ├── Orders/             Commands: PlaceOrder, UpdateOrderStatus, CancelOrder
│   │   │                       Queries:  GetOrderHistory, GetOrderById
│   │   ├── Reviews/            Commands: SubmitReview
│   │   │                       Queries:  GetProductReviews
│   │   ├── Wishlist/           Commands: AddToWishlist, RemoveFromWishlist
│   │   │                       Queries:  GetWishlist
│   │   ├── Admin/              Queries:  GetDashboard, GetAllOrders, GetAllCustomers
│   │   │                       Commands: CreateCoupon, UpdateCoupon
│   │   └── Common/
│   │       ├── Behaviors/      ValidationBehavior, LoggingBehavior, CachingBehavior
│   │       └── Interfaces/     IProductRepository, IOrderRepository, IBasketRepository,
│   │                           IAuthService, IPaymentService, IEmailService
│   │
│   ├── LapBox.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── LapBoxDbContext.cs
│   │   │   ├── Configurations/     One IEntityTypeConfiguration<T> per entity
│   │   │   ├── Repositories/       Catalog/, Orders/, Reviews/, ...
│   │   │   ├── Migrations/
│   │   │   └── Seeds/              AdminSeeder, OrderStatusSeeder
│   │   ├── Cache/                  BasketRepository (Redis)
│   │   ├── Auth/                   AuthService, JwtTokenGenerator, AppUser
│   │   ├── Payment/                PaymobPaymentService
│   │   ├── Email/                  EmailService
│   │   └── DependencyInjection.cs
│   │
│   └── LapBox.API/
│       ├── Modules/
│       │   ├── Auth/               AuthController
│       │   ├── Catalog/            ProductsController
│       │   ├── Basket/             BasketController
│       │   ├── Orders/             OrdersController
│       │   ├── Reviews/            ReviewsController
│       │   ├── Wishlist/           WishlistController
│       │   └── Admin/              AdminController
│       ├── Middleware/             GlobalExceptionMiddleware
│       ├── Extensions/             ServiceCollectionExtensions
│       └── Program.cs
│
└── tests/
    ├── LapBox.Domain.Tests/
    ├── LapBox.Application.Tests/
    └── LapBox.Infrastructure.Tests/
```

---

## Getting Started

### Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 10.0+ |
| SQL Server | 2019+ (or Docker) |
| Redis | 7.x (or Docker) |
| Git | any |

### Clone the repository

```bash
git clone https://github.com/your-org/lapbox.git
cd lapbox
```

### Restore dependencies

```bash
dotnet restore
```

---

## Environment Variables

Copy the example file and fill in your values:

```bash
cp src/LapBox.API/appsettings.Development.json.example src/LapBox.API/appsettings.Development.json
```

`appsettings.Development.json` — **never commit this file:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ShopDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-characters-long",
    "Issuer": "LapBox",
    "Audience": "LapBox",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Paymob": {
    "ApiKey": "your-paymob-api-key",
    "IntegrationId": "your-integration-id",
    "IframeId": "your-iframe-id",
    "HmacSecret": "your-hmac-secret"
  },
  "Email": {
    "SmtpHost": "smtp.mailtrap.io",
    "SmtpPort": 587,
    "Username": "your-username",
    "Password": "your-password",
    "FromAddress": "noreply@lapbox.com"
  },
  "AdminSeed": {
    "Email": "admin@lapbox.com",
    "Password": "Admin@123456"
  }
}
```

---

## Running the Project

### Option 1 — Local (SQL Server + Redis installed)

```bash
# Apply database migrations
dotnet ef database update --project src/LapBox.Infrastructure --startup-project src/LapBox.API

# Run the API
dotnet run --project src/LapBox.API
```

### Option 2 — Docker Compose (recommended for new devs)

```bash
docker-compose up -d
```

`docker-compose.yml` spins up: SQL Server, Redis, and the API together.

```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong@Passw0rd"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  api:
    build: .
    ports:
      - "5000:8080"
    depends_on:
      - db
      - redis
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=ShopDB;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;
      - Redis__ConnectionString=redis:6379
```

---

## Running Tests

```bash
# All tests
dotnet test

# Specific project
dotnet test tests/LapBox.Application.Tests

# With coverage report
dotnet test --collect:"XPlat Code Coverage"
```

Coverage target: **≥ 70% on Application layer** (all handlers).

---

## API Documentation

Once the API is running, Scalar UI is available at:

```
http://localhost:5000/scalar
```

OpenAPI JSON spec:

```
http://localhost:5000/openapi/v1.json
```

### Base URL

```
http://localhost:5000/api
```

### Authentication

All protected endpoints require a Bearer token:

```
Authorization: Bearer <access_token>
```

Obtain a token from `POST /api/auth/login`.

### Error format (all endpoints)

```json
{
  "type": "https://lapbox.com/errors/validation",
  "title": "One or more validation errors occurred.",
  "status": 422,
  "traceId": "0HN2V...",
  "errors": {
    "price": ["Price must be greater than 0."]
  }
}
```

### Quick endpoint reference

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/auth/register` | Public | Register new customer |
| POST | `/auth/login` | Public | Login, receive JWT |
| POST | `/auth/refresh` | Public | Refresh access token |
| GET | `/products` | Public | List products (paginated, filterable) |
| GET | `/products/{id}` | Public | Product detail with variants |
| GET | `/products/{id}/reviews` | Public | Reviews + rating summary |
| POST | `/basket/items` | Customer | Add item to basket |
| GET | `/basket` | Customer | View current basket |
| POST | `/orders` | Customer | Checkout |
| GET | `/orders` | Customer | Order history |
| GET | `/orders/{id}` | Customer | Order detail |
| POST | `/wishlist/{productId}` | Customer | Add to wishlist |
| POST | `/products/{id}/reviews` | Customer | Submit review |
| GET | `/admin/dashboard` | Admin | Sales dashboard |
| POST | `/admin/products` | Admin | Create product |
| PUT | `/admin/products/{id}` | Admin | Update product |
| PUT | `/admin/orders/{id}/status` | Admin | Update order status |
| POST | `/admin/coupons` | Admin | Create coupon |

Full request/response contracts: see `SDLC/05_API_Contracts.md`.

---

## Database

### Schema

Database: `ShopDB` — all tables under `shop` schema.

| Table | Description |
|---|---|
| `shop.Categories` | Product categories with tax rates |
| `shop.Suppliers` | Product suppliers |
| `shop.Shippers` | Shipping providers (DHL, FedEx, UPS) |
| `shop.Tags` | Product tags (New, Popular, Discount) |
| `shop.OrderStatuses` | Lookup table for order lifecycle states |
| `shop.Customers` | Registered customers |
| `shop.CustomerAddresses` | Multiple shipping addresses per customer |
| `shop.CustomerPhones` | Multiple phone numbers per customer |
| `shop.Products` | Products with soft delete (`IsDeleted`) |
| `shop.ProductVariants` | RAM / Storage variants per product |
| `shop.ProductImages` | Product images (one marked as primary) |
| `shop.ProductTags` | Many-to-many: products ↔ tags |
| `shop.Orders` | Customer orders |
| `shop.OrderItems` | Line items per order |
| `shop.Payments` | One payment per order (idempotent by `TransactionId`) |
| `shop.Coupons` | Discount codes (Percentage or Fixed) |
| `shop.Reviews` | Verified-purchase product reviews |
| `shop.Wishlists` | Customer wishlisted products |
| `shop.ShippingEligibilities` | Which shipper can ship which product to which region |
| `shop.OrdersArchive` | Archived historical orders |

ERD: see `SDLC/LapBox_ERD.dbml` — paste into [dbdiagram.io](https://dbdiagram.io/d).

### Migrations

```bash
# Create new migration
dotnet ef migrations add <MigrationName> \
  --project src/LapBox.Infrastructure \
  --startup-project src/LapBox.API

# Apply migrations
dotnet ef database update \
  --project src/LapBox.Infrastructure \
  --startup-project src/LapBox.API
```

**Convention:** one migration per feature branch, named `YYYYMMDD_DescriptiveName`.

---

## Redis / Basket

The basket is stored **only in Redis** — there is no SQL table for it.

```
Key:   basket:{customerId}
Type:  Hash
Field: {productId}
Value: { "quantity": 2 }
TTL:   7 days (refreshed on every write)
```

Stock is **not reserved** in the basket. Stock check + decrement happens atomically at checkout inside a SQL transaction with `UPDLOCK` to prevent race conditions.

---

## Modules

The project is structured as a **monolith with modular internals**. Each module can be developed independently.

| Module | Owner (Phase) | Depends on |
|---|---|---|
| Auth | Phase 1 — all devs | — |
| Catalog | Phase 2 — Dev 1 | Auth |
| Basket | Phase 2 — Dev 2 | Auth, Catalog (product lookup) |
| Orders | Phase 2 — Dev 3 | Auth, Catalog, Basket |
| Reviews & Wishlist | Phase 3 — Dev 1 or 2 | Auth, Catalog, Orders |
| Admin | Phase 3 — Dev 3 | All modules |

---

## SDLC Documentation

Full documentation lives in the `SDLC/` folder:

| File | Contents |
|---|---|
| `01_Project_Overview.md` | Vision, stakeholders, tech stack, constraints |
| `02_Requirements.md` | User stories + acceptance criteria per feature |
| `03_Architecture.md` | Clean Arch layers, CQRS patterns, Result pattern, Basket strategy |
| `04_Database.md` | Full schema SQL, index inventory, EF Core config notes |
| `05_API_Contracts.md` | Every endpoint with request/response JSON examples |
| `06_Task_Breakdown.md` | Trello-ready tasks with checklists, assignees, Definition of Done |
| `07_PRD.md` | Product vision, personas, user journeys, feature priority matrix |
| `LapBox_ERD.dbml` | ERD code for [dbdiagram.io](https://dbdiagram.io/d) |

---

## Team

| Dev | Phase 1 | Phase 2 | Phase 3 |
|---|---|---|---|
| Dev 1 | INFRA-01 (lead), AUTH-01, INFRA-02 | Catalog module | Reviews, Addresses |
| Dev 2 | INFRA-03, AUTH-03, INFRA-05 | Basket module | Wishlist |
| Dev 3 | INFRA-04, AUTH-02 | Orders module | Admin dashboard, Coupons |

---

## Git Workflow

```
main          ← production-ready only
dev           ← integration branch, all PRs merge here first
feature/*     ← one branch per Trello task (e.g. feature/cat-01-get-products)
```

**Branch naming:** `feature/{task-id}-short-description`
Example: `feature/ord-01-place-order`

**PR rules:**
- At least one reviewer approval before merge to `dev`
- All tests must pass
- No compiler warnings
- Migration (if any) must run cleanly

---

## License

Private — all rights reserved.
