# LapBox — Project Overview

> **Version:** 1.0
> **Last Updated:** 2026-04-19
> **Status:** Planning → Active Development
> **Stack:** .NET 10 · ASP.NET Core 10 · EF Core 10 · SQL Server · Redis · Clean Architecture + CQRS

---

## 1. What Is LapBox?

LapBox is a **single-vendor B2C e-commerce platform** specialised in selling laptops and accessories online. It targets the Egyptian market and is inspired by established stores like Sigma and Albader.

The platform consists of two surfaces:

| Surface | Audience | Purpose |
|---|---|---|
| Customer Storefront | End buyers | Browse, search, purchase, track orders |
| Admin Panel | Store owner / staff | Manage inventory, orders, customers, promotions |

This is **not** a marketplace. There is one seller (the business owner), and all products belong to that single vendor.

---

## 2. Business Goals

1. Give the store owner a professional, reliable online presence to sell laptops directly to consumers.
2. Provide customers with a smooth, trustworthy shopping experience — fast search, clear product specs, safe checkout.
3. Allow the team to scale the platform gradually without rewriting core logic (hence Clean Architecture).
4. Reduce manual overhead for the owner — automated stock tracking, order status updates, and sales reporting.

---

## 3. Stakeholders

| Role | Who | Responsibilities |
|---|---|---|
| Store Admin | Business owner / staff | Add products, manage orders, configure promotions |
| Customer | End buyer | Browse, purchase, review, track |
| Developer Team | 3 junior developers | Build and maintain the platform |
| Payment Gateway | Paymob (initial) | Process card transactions |
| Shipping Providers | DHL / FedEx / UPS | Fulfil deliveries, update tracking |

---

## 4. Scope

### In Scope (v1.0)

- Product catalogue with categories, tags, images, and **structured variants (RAM/Storage)**
- Customer registration, login, and profile management
- Shopping basket (Redis-backed, no SQL table)
- **Secure Checkout with temporary stock reservation (15-minute window)**
- **Detailed Inventory Tracking (Transaction logs for every stock increase/decrease)**
- Order lifecycle management with **full status history tracking**
- Payment integration via Paymob (with retry support)
- Product reviews and ratings (verified purchase only)
- Wishlist
- Admin panel: products, orders, customers, coupons, reports, **and activity audit logs**
- JWT-based authentication with role separation (Admin / Customer)

### Out of Scope (v1.0)

- Multi-vendor marketplace
- Mobile native apps (API-first, frontend team decides client)
- Loyalty / points system
- Live chat support
- Social login (Google, Facebook) — can be added later via Identity infrastructure

---

## 5. Constraints & Assumptions

| Constraint | Detail |
|---|---|
| Team size | 3 junior developers working in parallel |
| Timeline | Module-by-module delivery (see `06_Task_Breakdown.md`) |
| Frontend | Not decided yet — API is designed to be frontend-agnostic (JSON, no server-side rendering) |
| Payment | Start with Paymob; architecture must allow swapping gateway without touching business logic |
| Language | Arabic product names and customer data must be supported (NVARCHAR throughout) |
| Hosting | Not finalised — application must be environment-agnostic (no hardcoded paths) |

---

## 6. Non-Functional Requirements

| Category | Requirement | How We Meet It |
|---|---|---|
| **Reliability** | **Zero Over-selling** | Stock reservation system (15-min window) during checkout prevents race conditions and ensures a product isn't sold twice. |
| **Performance** | Product listing < 1.5s, checkout < 2s | **Surrogate keys (IDs) for fast DB joins**, Redis caching for catalog/basket, DB indexes, and fully async I/O operations. |
| **Scalability** | Support 10,000 concurrent users | Stateless API (JWT), Redis for session/basket to offload SQL Server, and efficient connection pooling. |
| **Security** | No SQL injection, no plaintext passwords | EF Core parameterized queries, Identity password hashing (PBKDF2), and HTTPS enforced via middleware. |
| **Maintainability** | Developer onboarding < 1 day | Clean Architecture with CQRS, SDLC docs, and consistent modular patterns across all layers. |
| **Testability** | Core logic fully unit-testable | Domain and Application layers have zero infrastructure dependencies, making them easy to test with xUnit/NSubstitute. |
| **Observability** | **Full Audit & Traceability** | Structured logging (Serilog), **Inventory transaction logs**, and tracking **CreatedBy / UpdatedBy** for all critical entities. |
---

## 7. Technology Stack

| Layer | Technology | Reason |
|---|---|---|
| Language | C# 13 / .NET 10 | LTS, AOT support, frozen collections, latest perf gains |
| Web Framework | ASP.NET Core 10 — Controllers | Clean, fast, first-class DI; Minimal API optional per module |
| ORM | Entity Framework Core 10 | First-class SQL Server, compiled queries, migrations |
| Database | SQL Server (ShopDB) | Relational, ACID, existing schema |
| Cache / Basket | Redis (StackExchange.Redis 2.x) | Fast in-memory, basket TTL, no SQL overhead |
| CQRS Bus | MediatR 12 | Decouples commands/queries from controllers |
| Validation | FluentValidation 11 | Declarative, testable, integrates with MediatR pipeline |
| Mapping | Mapster 7 | Faster than AutoMapper, source-gen support, simpler config |
| Auth | ASP.NET Core Identity 10 (Infrastructure only) + JWT Bearer | Password security without leaking Identity into Domain |
| Logging | Serilog + Seq (dev) / file sink (prod) | Structured logs, traceId on every request |
| Error format | RFC 7807 ProblemDetails | Standard, parseable by any frontend |
| API Docs | Scalar (built-in .NET 10, replaces Swagger UI) | Modern UI, OpenAPI 3.1 |
| Testing | xUnit 2 + NSubstitute + FluentAssertions | NSubstitute preferred over Moq in .NET 10 projects |
| Rate Limiting | ASP.NET Core built-in RateLimiter (.NET 10) | No extra packages needed |
| Health Checks | Microsoft.Extensions.Diagnostics.HealthChecks | Built-in, checks DB + Redis |

---

## 8. Architecture Summary

The project follows **Clean Architecture** with **CQRS** and a **modular folder structure** inside a monolith.

```
LapBox.sln
├── LapBox.Domain          ← Entities, Value Objects, Domain Events, Interfaces
├── LapBox.Application     ← Use Cases (Commands + Queries), DTOs, Validators
├── LapBox.Infrastructure  ← EF Core, Redis, Paymob, Email, Identity
└── LapBox.API             ← HTTP layer, Controllers/Endpoints, Middleware
```

Each layer is a **separate project** with strict dependency rules:

```
API → Application → Domain
Infrastructure → Application (implements interfaces)
```

**Domain and Application layers must never reference Infrastructure or any external library.**

Full details in `03_Architecture.md`.

---

## 9. Module Delivery Order

Modules are designed so each one can be owned by one developer independently.

```
Phase 1 — Foundation
  └── Auth Module          ← All devs align here first

Phase 2 — Core Commerce    ← 3 devs in parallel
  ├── Catalog Module       (Dev 1)
  ├── Basket Module        (Dev 2)
  └── Orders Module        (Dev 3 — starts after Catalog entities are merged)

Phase 3 — Value Features
  ├── Reviews & Wishlist   (Dev 1 or 2)
  └── Admin Module         (Dev 3 — depends on all above)
```

Full task breakdown in `06_Task_Breakdown.md`.
