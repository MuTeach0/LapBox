# LapBox — Task Breakdown (Trello-Ready)

> Each task below maps to one Trello card.
> Format: **Task Title** → Description → Acceptance Criteria → Who

---

## How to Use This Document

Copy each task as a Trello card into the board with these lists:
```
Backlog | In Progress | In Review | Done
```

Labels: `Auth` `Catalog` `Basket` `Orders` `Reviews` `Admin` `Infra` `DB`

Every card should have:
- Description from this doc
- Checklist from the Acceptance Criteria
- Assignee (Dev 1 / Dev 2 / Dev 3)
- Label

---

## Phase 1 — Foundation (All Devs Together)

**Priority: these tasks must all be Done before Phase 2 starts.**

---

### [INFRA-01] Solution setup & project structure
**Label:** `Infra`
**Assign:** All 3 devs, Dev 1 leads

Create the solution with all 4 projects and configure project references.

Acceptance Criteria:
- [ ] `LapBox.sln` created with projects: Domain, Application, Infrastructure, API
- [ ] Project references follow dependency rule (Domain ← Application ← Infrastructure ← API)
- [ ] Domain and Application have zero NuGet packages that reference EF Core or Identity
- [ ] `.gitignore` configured, initial commit pushed to shared repo
- [ ] `appsettings.json` has sections: `ConnectionStrings`, `Jwt`, `Redis`, `Paymob`
- [ ] `appsettings.Development.json` with local values (not committed — in `.gitignore`)

---

### [INFRA-02] Database setup & initial migration
**Label:** `Infra` `DB`
**Assign:** Dev 1

Create `LapBoxDbContext`, all entity configurations, and run first migration.

Acceptance Criteria:
- [ ] All tables from `04_Database.md` exist in the DB after migration
- [ ] `HasQueryFilter` for soft delete is applied on Products
- [ ] All indexes from `04_Database.md` Section 3 are created
- [ ] Admin seed creates one Admin user (credentials in a secret, not hardcoded)
- [ ] `OrderStatuses` seed data is inserted
- [ ] Migration runs cleanly from `dotnet ef database update`

---

### [INFRA-03] Result pattern & ProblemDetails middleware
**Label:** `Infra`
**Assign:** Dev 2

Implement `Result<T>`, `Error`, and the global exception middleware.

Acceptance Criteria:
- [ ] `Result<T>` class implemented in Domain/Common as described in `03_Architecture.md`
- [ ] `Error` record with static factory methods implemented
- [ ] `GlobalExceptionMiddleware` catches `ValidationException` → 422 ProblemDetails
- [ ] `GlobalExceptionMiddleware` catches any other exception → 500 ProblemDetails (no stack trace in response)
- [ ] `ToProblemDetails()` extension method on `Result<T>` works in controllers
- [ ] Middleware is registered in `Program.cs` before all other middleware

---

### [INFRA-04] MediatR + FluentValidation pipeline
**Label:** `Infra`
**Assign:** Dev 3

Wire up MediatR with `ValidationBehavior` and `LoggingBehavior`.

Acceptance Criteria:
- [ ] MediatR registered in Application DI
- [ ] `ValidationBehavior<TRequest, TResponse>` auto-discovers all `IValidator<T>` in assembly
- [ ] `LoggingBehavior<TRequest, TResponse>` logs request name and duration at Info level
- [ ] FluentValidation validators auto-registered from Application assembly
- [ ] Sending a command with invalid data returns 422 without reaching the handler

---

### [INFRA-05] Redis connection & basket repository
**Label:** `Infra`
**Assign:** Dev 2

Set up Redis and implement `IBasketRepository`.

Acceptance Criteria:
- [ ] `StackExchange.Redis` configured from `appsettings.json`
- [ ] `IBasketRepository` interface defined in Application with: `GetAsync`, `SetAsync`, `DeleteAsync`, `DeleteItemAsync`
- [ ] `BasketRepository` implementation stores basket as Redis Hash keyed `basket:{customerId}`
- [ ] TTL is 7 days, refreshed on every write
- [ ] Redis connection failure does not crash the app (logs error, returns empty basket gracefully)

---

### [AUTH-01] Auth Module — Register & Login
**Label:** `Auth`
**Assign:** Dev 1

Implement customer registration and login.

Acceptance Criteria:
- [ ] `RegisterCommand` and handler created in `Application/Auth/Commands/Register/`
- [ ] `LoginCommand` and handler created in `Application/Auth/Commands/Login/`
- [ ] `IAuthService` interface defined in Application with `RegisterAsync` and `LoginAsync`
- [ ] `AuthService` implements `IAuthService` using `UserManager<AppUser>` in Infrastructure
- [ ] `JwtTokenGenerator` creates token with `sub`, `email`, `role`, `jti`, `iat`, `exp` claims
- [ ] Access token expiry: 15 minutes. Refresh token expiry: 7 days
- [ ] Duplicate email returns `Result.Failure(Error.Conflict(...))`
- [ ] Password rules enforced via FluentValidation (min 8 chars, 1 uppercase, 1 digit)
- [ ] `POST /api/auth/register` returns 201 with `AuthResponse`
- [ ] `POST /api/auth/login` returns 200 with `AuthResponse`
- [ ] `POST /api/auth/login` with wrong credentials returns 401

---

### [AUTH-02] Auth Module — Refresh Token & Logout
**Label:** `Auth`
**Assign:** Dev 3

Implement token refresh and logout.

Acceptance Criteria:
- [ ] `RefreshTokenCommand` and handler implemented
- [ ] Old refresh token is deleted after use (rotation)
- [ ] Invalid/expired refresh token returns 401
- [ ] `POST /api/auth/logout` invalidates refresh token and returns 204
- [ ] `POST /api/auth/refresh` returns new access + refresh token pair

---

### [AUTH-03] JWT middleware & role authorization
**Label:** `Auth`
**Assign:** Dev 2

Configure JWT validation and role-based authorization policies.

Acceptance Criteria:
- [ ] JWT Bearer authentication configured in `Program.cs`
- [ ] Two policies defined: `RequireCustomer` and `RequireAdmin`
- [ ] `[Authorize(Policy = "RequireAdmin")]` on admin controllers returns 403 for Customer tokens
- [ ] Unauthenticated requests to protected endpoints return 401
- [ ] Token with tampered signature returns 401

---

## Phase 2 — Core Commerce (Parallel)

### Dev 1 Track: Catalog Module

---

### [CAT-01] Get Products (list with filters)
**Label:** `Catalog`
**Assign:** Dev 1

Acceptance Criteria:
- [ ] `GetProductsQuery` with all filter/sort/search/pagination params
- [ ] Handler queries DB via `IProductRepository`, applies all filters
- [ ] Soft-deleted products excluded (handled by EF query filter)
- [ ] Results cached in Redis for 3 minutes (uses `ICacheable` + `CachingBehavior`)
- [ ] `GET /api/products` returns 200 with paginated envelope
- [ ] Empty results return 200 with empty array, not 404
- [ ] Invalid `sortBy` value returns 422

---

### [CAT-02] Get Product by ID
**Label:** `Catalog`
**Assign:** Dev 1

Acceptance Criteria:
- [ ] `GetProductByIdQuery` includes images, variants, tags, category, supplier
- [ ] Average rating and review count computed (can be a DB query for now)
- [ ] Response cached in Redis for 5 minutes keyed `product:{id}`
- [ ] `GET /api/products/{id}` returns 200 with full product detail
- [ ] Non-existent ID returns 404

---

### [CAT-03] Create & Update Product (Admin)
**Label:** `Catalog`
**Assign:** Dev 1

Acceptance Criteria:
- [ ] `CreateProductCommand` with validator — all required field rules
- [ ] Handler creates Product + Images + Variants + ProductTags in one transaction
- [ ] `UpdateProductCommand` supports partial update
- [ ] Updating stock below 0 returns 422
- [ ] On update, Redis cache for this product is invalidated
- [ ] `POST /api/admin/products` returns 201 + Location header
- [ ] `PUT /api/admin/products/{id}` returns 200 with updated product
- [ ] Both require Admin role — 403 for Customer

---

### [CAT-04] Delete Product (soft delete)
**Label:** `Catalog`
**Assign:** Dev 1

Acceptance Criteria:
- [ ] `DeleteProductCommand` sets `IsDeleted = true`
- [ ] Product with active orders (not Delivered/Cancelled) returns 409
- [ ] Deleted product disappears from all customer queries (query filter handles it)
- [ ] `DELETE /api/admin/products/{id}` returns 204
- [ ] Admin role required

---

### Dev 2 Track: Basket Module

---

### [BAS-01] Get Basket
**Label:** `Basket`
**Assign:** Dev 2

Acceptance Criteria:
- [ ] `GetBasketQuery` loads basket from Redis
- [ ] Product names and prices fetched fresh from DB (not stored in Redis)
- [ ] Calculates `subtotal` per item and `totalAmount`
- [ ] Empty basket returns 200 with empty items array
- [ ] `GET /api/basket` returns correct shape from `05_API_Contracts.md`

---

### [BAS-02] Add / Update / Remove Basket Items
**Label:** `Basket`
**Assign:** Dev 2

Acceptance Criteria:
- [ ] `AddToBasketCommand`: adds item or increments quantity if already exists
- [ ] `UpdateBasketItemCommand`: updates quantity; quantity 0 = delete item
- [ ] `RemoveBasketItemCommand`: removes specific item
- [ ] `ClearBasketCommand`: deletes entire Redis key
- [ ] Adding non-existent productId returns 404 (checks DB)
- [ ] Quantity > 99 returns 422
- [ ] TTL is refreshed on every write
- [ ] All endpoints return correct HTTP status per `05_API_Contracts.md`

---

### Dev 3 Track: Orders Module

**Note: Wait for CAT-01/CAT-02 entities to be merged before starting ORD-01.**

---

### [ORD-01] Place Order (Checkout)
**Label:** `Orders`
**Assign:** Dev 3

This is the most critical command in the system — read the architecture doc before starting.

Acceptance Criteria:
- [ ] `PlaceOrderCommand` with: addressId, shipperId, optional couponCode
- [ ] Validate address belongs to authenticated customer
- [ ] Validate coupon if provided: exists, active, not expired, usage < maxUses, order total ≥ minOrderAmount
- [ ] Stock check + decrement inside `BEGIN TRANSACTION` with `UPDLOCK` row hints (see `03_Architecture.md` Section 7)
- [ ] If any item is out of stock, entire transaction is rolled back, 422 returned with details
- [ ] On success: Order created, OrderItems inserted, coupon `UsedCount` incremented, basket cleared from Redis
- [ ] Payment URL initiated via `IPaymentService.CreatePaymentAsync`
- [ ] Returns 201 with orderId and paymentUrl
- [ ] Unit test covers: success path, out-of-stock path, invalid coupon path

---

### [ORD-02] Payment Webhook
**Label:** `Orders`
**Assign:** Dev 3

Acceptance Criteria:
- [ ] `POST /api/orders/payment-callback` is public (no JWT required)
- [ ] HMAC signature verified before processing — invalid signature returns 400 immediately
- [ ] Verified payment: Order status → `Confirmed`, Payment record created with `Status: Completed`
- [ ] Failed payment: Order status → `Cancelled`, stock quantities restored
- [ ] Idempotent: processing same `TransactionId` twice has no effect (check `UQ_Payments_TransactionId`)
- [ ] Always returns 200 (Paymob requirement)

---

### [ORD-03] Order History & Detail
**Label:** `Orders`
**Assign:** Dev 3

Acceptance Criteria:
- [ ] `GetOrderHistoryQuery` returns paginated list for authenticated customer only
- [ ] `GetOrderByIdQuery` returns full detail (items, address, payment)
- [ ] Customer cannot access another customer's order — returns 404 (not 403, to avoid info leakage)
- [ ] Admin can access any order
- [ ] `GET /api/orders` and `GET /api/orders/{id}` match shapes in `05_API_Contracts.md`

---

### [ORD-04] Admin Order Management
**Label:** `Orders`
**Assign:** Dev 3

Acceptance Criteria:
- [ ] `UpdateOrderStatusCommand` validates allowed transitions
- [ ] Invalid transitions (e.g. Delivered → Pending) return 422 with current and requested status
- [ ] `CancelOrderCommand` restores stock quantities and flags payment for refund
- [ ] Only Pending/Confirmed orders can be cancelled — others return 422
- [ ] `GET /api/admin/orders` supports all query params from `05_API_Contracts.md`

---

## Phase 3 — Value Features

---

### [REV-01] Submit & View Reviews
**Label:** `Reviews`
**Assign:** Dev 1 or Dev 2

Acceptance Criteria:
- [ ] `SubmitReviewCommand` checks for verified purchase (completed OrderItem exists)
- [ ] Duplicate review (same customer + product) returns 409
- [ ] Rating 1–5 enforced in validator
- [ ] `IsVerified` flag set correctly
- [ ] `GET /api/products/{id}/reviews` returns summary + paginated reviews
- [ ] `POST /api/products/{id}/reviews` returns 201
- [ ] Reviews endpoint is public (read) / Customer-only (write)

---

### [WIS-01] Wishlist
**Label:** `Reviews`
**Assign:** Dev 1 or Dev 2

Acceptance Criteria:
- [ ] `AddToWishlistCommand` is idempotent — no error if already in wishlist
- [ ] `GetWishlistQuery` excludes soft-deleted products silently
- [ ] `RemoveFromWishlistCommand` returns 204
- [ ] All endpoints match shapes in `05_API_Contracts.md`

---

### [ADM-01] Admin Dashboard
**Label:** `Admin`
**Assign:** Dev 3

Acceptance Criteria:
- [ ] `GetDashboardQuery` returns revenue (all time, this month, today), order counts by status, top 5 products
- [ ] Response cached in Redis for 10 minutes
- [ ] `GET /api/admin/dashboard` returns correct shape from `05_API_Contracts.md`
- [ ] Admin role required

---

### [ADM-02] Coupon Management
**Label:** `Admin`
**Assign:** Dev 3

Acceptance Criteria:
- [ ] `CreateCouponCommand` with all fields, duplicate code returns 409
- [ ] `UpdateCouponCommand` supports partial update including deactivation
- [ ] `POST /api/admin/coupons` returns 201
- [ ] `PUT /api/admin/coupons/{id}` returns 200
- [ ] All validators per `02_Requirements.md`

---

### [ADM-03] Customer & Address Management
**Label:** `Admin`
**Assign:** Dev 1

Acceptance Criteria:
- [ ] `GET /api/customers/me/addresses` returns customer's addresses
- [ ] `POST /api/customers/me/addresses` creates new address
- [ ] `DELETE /api/customers/me/addresses/{id}` returns 409 if address used in an order
- [ ] `GET /api/admin/customers` returns paginated list with search

---

## Definition of Done (for every task)

A Trello card moves to **Done** only when ALL of the following are true:

- [ ] All acceptance criteria in the checklist are checked
- [ ] Unit tests written and passing for all business logic paths (happy path + at least 2 error paths)
- [ ] No compiler warnings
- [ ] PR reviewed by at least one other team member
- [ ] Scalar/Scalar shows the endpoint correctly
- [ ] Postman collection updated with the new endpoint
- [ ] No hardcoded strings — use constants or config
- [ ] Migration (if DB change) runs cleanly from scratch

---

## Team Assignment Summary

| Dev | Phase 1 | Phase 2 | Phase 3 |
|---|---|---|---|
| Dev 1 | INFRA-01 (lead), AUTH-01, INFRA-02 | Catalog (CAT-01 to CAT-04) | REV-01 or WIS-01, ADM-03 |
| Dev 2 | INFRA-03, AUTH-03, INFRA-05 | Basket (BAS-01, BAS-02) | WIS-01 or REV-01 |
| Dev 3 | INFRA-04, AUTH-02 | Orders (ORD-01 to ORD-04) | ADM-01, ADM-02 |

---

## Acceptance Criteria for the Whole Project (Release Checklist)

Before calling v1.0 done, verify all of the following:

**Functional:**
- [ ] Customer can register, login, browse products, add to basket, checkout, and track order end-to-end
- [ ] Admin can create a product and it appears in storefront immediately
- [ ] Coupon applies correct discount at checkout
- [ ] Two customers checking out the last item simultaneously — only one succeeds, other gets 422
- [ ] Review submission rejected when no verified purchase

**Technical:**
- [ ] All endpoints return ProblemDetails on error (no raw exception text ever)
- [ ] All list endpoints are paginated
- [ ] JWT tokens expire correctly and refresh works
- [ ] Redis basket expires after 7 days of inactivity
- [ ] Soft-deleted products never appear in customer API
- [ ] Payment webhook is idempotent

**Quality:**
- [ ] Unit test coverage ≥ 70% on Application layer
- [ ] No broken migrations (fresh migration from scratch succeeds)
- [ ] No N+1 queries (check EF Core logs in dev)
- [ ] API documented in Scalar and Postman collection exported
- [ ] Serilog writing structured logs to console (and file in production)
