# LapBox — Architecture (Updated v1.1)

> This document explains every architectural decision in the project.
> If you are a new developer, read this before touching any code.

---

## 1. The Big Picture

LapBox is a **monolith with modular internals**. It is NOT microservices — we have one deployable unit, one database, one Redis instance. But internally, code is organised into independent modules so three developers can work in parallel without stepping on each other.



**Dependency rule (enforced by project references):**
- Domain depends on nothing.
- Application depends on Domain only.
- Infrastructure depends on Application (to implement its interfaces).
- API depends on Application (to send commands/queries) and Infrastructure (for DI registration).

---

## 2. Updated Project Structure
LapBox.sln
│
├── src/
│   ├── LapBox.Domain/
│   │   ├── Entities/
│   │   │   ├── Catalog/
│   │   │   │   ├── Product.cs
│   │   │   │   ├── ProductVariant.cs (Structured Specs)
│   │   │   │   ├── StockReservation.cs (New: For 15-min window)
│   │   │   │   └── InventoryTransaction.cs (New: For Audit)
│   │   │   ├── Orders/
│   │   │   │   ├── Order.cs
│   │   │   │   └── OrderStatusHistory.cs (New: For Status Audit)
│   │   │   └── Common/
│   │   │       └── BaseAuditableEntity.cs (New: Shadow properties)
│   │
│   ├── LapBox.Application/
│   │   ├── Catalog/
│   │   │   ├── Commands/ (ReserveStock, CreateProduct...)
│   │   │   └── Queries/ (GetProductDetail...)
│   │   └── Common/
│   │       ├── Interfaces/ (ICurrentUserService, IDateTime...)
│   │       └── Behaviors/ (Validation, Logging, Performance)
│   │
│   ├── LapBox.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── Interceptors/ (New: AuditableEntityInterceptor)
│   │   │   └── Repositories/ (Using .AsNoTracking() for Queries)
│   │   └── BackgroundJobs/ (New: ReservationExpiryWorker)
│   │
│   └── LapBox.API/
│       └── Controllers/

---

## 3. CQRS & Result Pattern
Every operation goes through MediatR as either a **Command** (write) or a **Query** (read). We never throw exceptions for business errors; every handler returns `Result<T>`.

- **Success:** Returns `200 OK` or `201 Created`.
- **Business Failure:** Returns `422 Unprocessable Entity` or `409 Conflict`.
- **Infrastructure Failure:** Caught by `GlobalExceptionMiddleware` -> `500 Internal Server Error`.

---

## 4. Performance & Caching Strategy
To meet the requirement of **Response Time < 1.5s**:

1.  **Read Operations:** Repositories must use `.AsNoTracking()` in all Query handlers to avoid EF change tracking overhead.
2.  **Output Caching:** Product details and Catalog lists are cached in Redis via `CachingBehavior` for 5 minutes.
3.  **Invalidation:** Commands (Update/Delete) trigger a cache invalidation for the respective keys.

---

## 5. Stock Management Architecture (The Reservation System)

Unlike standard baskets, LapBox uses a **Soft-Lock Reservation** to prevent race conditions during payment:

1.  **The Lock:** When `PlaceOrderCommand` starts, the system creates a record in `StockReservations` and updates the logical `AvailableStock`.
2.  **Atomic Transaction:** This happens inside a SQL Transaction using `UPDLOCK` on the product row to ensure zero over-selling.
3.  **Expiry Worker:** A background service (`PeriodicTimer` or `Hangfire`) runs every 60 seconds to release reservations older than 15 minutes that haven't been paid.



---

## 6. Audit & Traceability
We don't just delete or update data; we track it:

-   **Automatic Auditing:** An EF Core Interceptor automatically populates `CreatedAt`, `CreatedBy`, `LastModifiedAt`, and `LastModifiedBy` by resolving `ICurrentUserService`.
-   **Inventory Audit:** Every quantity change creates an entry in `InventoryTransactions`. This provides a full history of why stock moved (Sale, Return, Admin adjustment).
-   **Status History:** Every order status change is logged in `OrderStatusHistory` to track the lifecycle and admin performance.

---

## 7. Authentication Strategy (Hybrid)
We use **ASP.NET Core Identity** for user management but keep it isolated in the Infrastructure layer.

-   **JWT Payload:** Includes `sub` (UserId), `email`, and `role` (Admin/Customer).
-   **Token Rotation:** Refresh tokens are rotated on every use. Old tokens are invalidated immediately to prevent replay attacks.
-   **Security:** Password hashing is handled by Identity; tokens are stored securely in Redis/SQL.

---

## 8. Module Boundaries & Rules

1.  **No Direct DbContext:** Use Repository interfaces. This keeps Application code testable and decoupled from EF Core.
2.  **Cross-Module Communication:** If the Order module needs Product data, it uses the `IProductRepository`, not a direct SQL join.
3.  **No Logic in Controllers:** Controllers are 10-line methods that just call `_mediator.Send()`.
4.  **Zero Raw SQL:** Use EF Core for standard CRUD. If complex reporting is needed, use **Dapper** inside the Infrastructure layer only.

---

## 9. Coding Conventions

| Convention | Rule |
| :--- | :--- |
| **Async** | All I/O must be async. Always pass `CancellationToken`. |
| **Records** | Use `record` for DTOs, Commands, and Queries (Immutability). |
| **Validation** | Use `FluentValidation`. Never validate manually inside handlers. |
| **Errors** | All errors must return RFC 7807 `ProblemDetails`. |
| **Logging** | Log important events (Orders, Stock changes) with `LogInformation`. |