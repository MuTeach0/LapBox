# LapBox — Architecture

> This document explains every architectural decision in the project.
> If you are a new developer, read this before touching any code.

---

## 1. The Big Picture

LapBox is a **monolith with modular internals**. It is NOT microservices — we have one deployable unit, one database, one Redis instance. But internally, code is organised into independent modules so three developers can work in parallel without stepping on each other.

```
┌─────────────────────────────────────────────────────┐
│                      LapBox.API                      │
│  (HTTP in, ProblemDetails out, JWT middleware)       │
└────────────────────────┬────────────────────────────┘
                         │ MediatR
┌────────────────────────▼────────────────────────────┐
│                  LapBox.Application                  │
│  Commands · Queries · Validators · DTOs · Interfaces │
└──────────┬──────────────────────────────────────────┘
           │ depends on
┌──────────▼──────────┐   ┌───────────────────────────┐
│   LapBox.Domain     │   │   LapBox.Infrastructure   │
│  Entities · Enums   │◄──│  EF Core · Redis · Paymob │
│  Value Objects      │   │  Identity · Email         │
└─────────────────────┘   └───────────────────────────┘
```

**Dependency rule (enforced by project references, not convention):**
- Domain depends on nothing
- Application depends on Domain only
- Infrastructure depends on Application (to implement its interfaces)
- API depends on Application (to send commands/queries) and Infrastructure (for DI registration)

---

## 2. Project Structure

```
LapBox.sln
│
├── src/
│   ├── LapBox.Domain/
│   │   ├── Entities/
│   │   │   ├── Catalog/
│   │   │   │   ├── Product.cs
│   │   │   │   ├── Category.cs
│   │   │   │   ├── ProductVariant.cs
│   │   │   │   ├── ProductImage.cs
│   │   │   │   └── ProductTag.cs
│   │   │   ├── Orders/
│   │   │   │   ├── Order.cs
│   │   │   │   ├── OrderItem.cs
│   │   │   │   └── OrderStatus.cs (enum)
│   │   │   ├── Auth/
│   │   │   │   └── AppUser.cs
│   │   │   ├── Customers/
│   │   │   │   ├── Customer.cs
│   │   │   │   ├── CustomerAddress.cs
│   │   │   │   └── CustomerPhone.cs
│   │   │   ├── Payments/
│   │   │   │   └── Payment.cs
│   │   │   ├── Reviews/
│   │   │   │   └── Review.cs
│   │   │   ├── Promotions/
│   │   │   │   └── Coupon.cs
│   │   │   └── Shared/
│   │   │       └── Supplier.cs
│   │   │       └── Shipper.cs
│   │   ├── ValueObjects/
│   │   │   └── Money.cs
│   │   └── Common/
│   │       ├── Result.cs          ← Result Pattern
│   │       └── Error.cs
│   │
│   ├── LapBox.Application/
│   │   ├── Catalog/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateProduct/
│   │   │   │   │   ├── CreateProductCommand.cs
│   │   │   │   │   ├── CreateProductCommandHandler.cs
│   │   │   │   │   └── CreateProductCommandValidator.cs
│   │   │   │   └── UpdateProduct/
│   │   │   │       ├── UpdateProductCommand.cs
│   │   │   │       ├── UpdateProductCommandHandler.cs
│   │   │   │       └── UpdateProductCommandValidator.cs
│   │   │   └── Queries/
│   │   │       ├── GetProducts/
│   │   │       │   ├── GetProductsQuery.cs
│   │   │       │   └── GetProductsQueryHandler.cs
│   │   │       └── GetProductById/
│   │   │           ├── GetProductByIdQuery.cs
│   │   │           └── GetProductByIdQueryHandler.cs
│   │   ├── Orders/
│   │   │   ├── Commands/
│   │   │   │   └── PlaceOrder/
│   │   │   │       ├── PlaceOrderCommand.cs
│   │   │   │       ├── PlaceOrderCommandHandler.cs
│   │   │   │       └── PlaceOrderCommandValidator.cs
│   │   │   └── Queries/
│   │   │       └── GetOrderHistory/
│   │   │           └── ...
│   │   ├── Auth/
│   │   │   └── Commands/
│   │   │       ├── Register/
│   │   │       └── Login/
│   │   ├── Basket/
│   │   │   └── Commands/
│   │   │       └── ...
│   │   ├── Reviews/
│   │   └── Common/
│   │       ├── Behaviors/
│   │       │   ├── ValidationBehavior.cs
│   │       │   ├── LoggingBehavior.cs
│   │       │   └── CachingBehavior.cs
│   │       └── Interfaces/
│   │           ├── IProductRepository.cs
│   │           ├── IOrderRepository.cs
│   │           ├── IBasketRepository.cs
│   │           ├── IAuthService.cs
│   │           ├── IPaymentService.cs
│   │           └── IEmailService.cs
│   │
│   ├── LapBox.Infrastructure/
│   │   ├── Persistence/
│   │   │   ├── LapBoxDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── ProductConfiguration.cs
│   │   │   │   ├── OrderConfiguration.cs
│   │   │   │   └── ... (one file per entity)
│   │   │   ├── Repositories/
│   │   │   │   ├── Catalog/
│   │   │   │   │   └── ProductRepository.cs
│   │   │   │   ├── Orders/
│   │   │   │   │   └── OrderRepository.cs
│   │   │   │   └── ...
│   │   │   ├── Migrations/
│   │   │   └── Seeds/
│   │   │       └── AdminSeeder.cs
│   │   ├── Cache/
│   │   │   └── BasketRepository.cs    ← Redis implementation
│   │   ├── Auth/
│   │   │   ├── AuthService.cs         ← UserManager lives here only
│   │   │   ├── JwtTokenGenerator.cs
│   │   │   └── Models/
│   │   │       └── AppUser.cs         ← IdentityUser subclass
│   │   ├── Payment/
│   │   │   └── PaymobPaymentService.cs
│   │   ├── Email/
│   │   │   └── EmailService.cs
│   │   └── DependencyInjection.cs     ← All Infrastructure registrations
│   │
│   └── LapBox.API/
│       ├── Modules/
│       │   ├── Catalog/
│       │   │   └── ProductsController.cs
│       │   ├── Orders/
│       │   │   └── OrdersController.cs
│       │   ├── Auth/
│       │   │   └── AuthController.cs
│       │   ├── Basket/
│       │   │   └── BasketController.cs
│       │   └── Admin/
│       │       └── AdminController.cs
│       ├── Middleware/
│       │   └── GlobalExceptionMiddleware.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       └── Program.cs
│
└── tests/
    ├── LapBox.Domain.Tests/
    ├── LapBox.Application.Tests/
    └── LapBox.Infrastructure.Tests/
```

---

## 3. CQRS Pattern

Every operation goes through MediatR as either a Command (write) or a Query (read).

**Command — changes state:**
```csharp
// Application/Catalog/Commands/CreateProduct/CreateProductCommand.cs
public record CreateProductCommand(
    string ProductName,
    int CategoryId,
    int SupplierId,
    decimal Price,
    int QuantityInStock,
    string? Description
) : IRequest<Result<int>>;  // returns new ProductId

// Handler
public class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Result<int>>
{
    private readonly IProductRepository _repo;

    public CreateProductCommandHandler(IProductRepository repo)
        => _repo = repo;

    public async Task<Result<int>> Handle(
        CreateProductCommand request,
        CancellationToken ct)
    {
        // 1. Validate business rules (not validation — that's FluentValidation)
        // 2. Build entity
        // 3. Persist
        // 4. Return Result
    }
}
```

**Query — reads state, never mutates:**
```csharp
// Application/Catalog/Queries/GetProducts/GetProductsQuery.cs
public record GetProductsQuery(
    int Page,
    int PageSize,
    int? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? SortBy,
    bool? InStock
) : IRequest<Result<PagedResult<ProductListItemDto>>>;
```

**Controller is thin — just HTTP translation:**
```csharp
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetProductsQuery query)
    {
        var result = await _sender.Send(query);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }
}
```

---

## 4. Result Pattern

We never throw exceptions for business errors. Every handler returns `Result<T>`.

```csharp
// Domain/Common/Result.cs
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error? Error { get; }

    private Result(T value)
    {
        IsSuccess = true;
        Value = value;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);
}

// Domain/Common/Error.cs
public record Error(string Code, string Message, int StatusCode)
{
    public static readonly Error NotFound =
        new("NOT_FOUND", "Resource not found.", 404);

    public static readonly Error Unauthorized =
        new("UNAUTHORIZED", "Authentication required.", 401);

    public static Error Conflict(string message) =>
        new("CONFLICT", message, 409);

    public static Error Validation(string message) =>
        new("VALIDATION", message, 422);

    public static Error Custom(string code, string message, int status) =>
        new(code, message, status);
}
```

**Extension method to convert Result to ProblemDetails response:**
```csharp
// API/Extensions/ResultExtensions.cs
public static IActionResult ToProblemDetails<T>(this Result<T> result)
{
    return new ObjectResult(new ProblemDetails
    {
        Title = result.Error!.Message,
        Status = result.Error.StatusCode,
        Extensions = { ["code"] = result.Error.Code }
    })
    { StatusCode = result.Error.StatusCode };
}
```

---

## 5. MediatR Pipeline Behaviors

Behaviors run automatically for every command and query, in this order:

```
Request → LoggingBehavior → ValidationBehavior → CachingBehavior → Handler → Response
```

**ValidationBehavior** — runs FluentValidation, short-circuits with `422` if invalid:
```csharp
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>  // MediatR 12 interface
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var failures = _validators
            .SelectMany(v => v.Validate(request).Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

**CachingBehavior** — reads from Redis before hitting DB for queries that implement `ICacheable`:
```csharp
public interface ICacheable
{
    string CacheKey { get; }
    TimeSpan CacheDuration { get; }
}
```

---

## 6. Repository Pattern

We use Repository, not direct `DbContext` in Application handlers. This keeps the Application layer free of EF Core.

**Interface in Application layer (no EF reference):**
```csharp
// Application/Common/Interfaces/IProductRepository.cs
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct);
    Task<PagedResult<Product>> GetAllAsync(ProductFilter filter, CancellationToken ct);
    Task<int> CreateAsync(Product product, CancellationToken ct);
    Task UpdateAsync(Product product, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}
```

**Implementation in Infrastructure layer (uses EF Core):**
```csharp
// Infrastructure/Persistence/Repositories/Catalog/ProductRepository.cs
public class ProductRepository : IProductRepository
{
    private readonly LapBoxDbContext _context;

    public ProductRepository(LapBoxDbContext context) => _context = context;

    public async Task<Product?> GetByIdAsync(int id, CancellationToken ct)
        => await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.ProductId == id && !p.IsDeleted, ct);
}
```

We do NOT create `IAppDbContext` in the Application layer. The Repository is the abstraction — it is the contract. The `DbContext` is an implementation detail that lives only in Infrastructure.

---

## 7. Basket Architecture (Redis Only)

The basket has NO SQL table. It lives entirely in Redis.

```
Redis Key:   basket:{customerId}
Redis Type:  Hash
Field:       {productId}
Value:       { quantity: 2 }
TTL:         7 days (refreshed on every write)
```

**Basket model (in-memory only, never persisted to SQL):**
```csharp
public class BasketItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class CustomerBasket
{
    public int CustomerId { get; set; }
    public List<BasketItem> Items { get; set; } = new();
}
```

**Why no SQL table?**
- Baskets are temporary — no business value in persisting them long-term
- Redis gives sub-millisecond reads
- Automatic TTL expiry = no cleanup jobs needed
- No joins needed — basket is always loaded in full

**Stock Race Condition Handling:**

The basket does NOT hold stock. The only moment stock is touched is at checkout:

```
Checkout Request
    → Begin SQL Transaction
    → For each basket item:
        → SELECT QuantityInStock WITH (UPDLOCK)  ← row lock prevents race
        → If quantity < requested: ROLLBACK → return 422
        → UPDATE Products SET QuantityInStock = QuantityInStock - @qty
    → Insert Order + OrderItems
    → COMMIT
    → Clear basket from Redis
    → Initiate payment
```

With `UPDLOCK`, if two customers checkout simultaneously for the last item, one gets the lock and succeeds, the other waits, then sees 0 stock and gets a `422`.

---

## 8. Authentication Architecture (Hybrid)

Identity is used only inside `Infrastructure/Auth/`. Nothing in Domain or Application knows about `UserManager`, `IdentityUser`, or anything from `Microsoft.AspNetCore.Identity`.

**Flow:**
```
POST /api/auth/register
  → RegisterCommand (Application)
  → IAuthService.RegisterAsync (interface in Application)
  → AuthService : IAuthService (Implementation in Infrastructure)
      → UserManager<AppUser>.CreateAsync(...)
      → JwtTokenGenerator.Generate(userId, email, role)
  → Returns AuthResponse { AccessToken, RefreshToken, ExpiresAt }
```

**JWT Payload:**
```json
{
  "sub": "customer-guid",
  "email": "alice@mail.com",
  "role": "Customer",
  "jti": "unique-token-id",
  "iat": 1713300000,
  "exp": 1713300900
}
```

**Refresh Token Strategy:**
- Refresh tokens are stored in SQL (`AspNetUserTokens` via Identity)
- Access token: 15 minutes
- Refresh token: 7 days, rotated on use (old token deleted, new one issued)
- Refresh tokens are hashed before storage (never plaintext in DB)

---

## 9. Error Handling Strategy

**Global Exception Middleware** catches any unhandled exception and formats it as ProblemDetails:

```csharp
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 422;
            await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(
                ex.Errors.GroupBy(e => e.PropertyName)
                         .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "An unexpected error occurred.",
                Status = 500
            });
        }
    }
}
```

Business errors use `Result<T>` and never reach this middleware. This middleware is the last resort for infrastructure failures.

---

## 10. Payment Integration

The payment gateway is abstracted behind an interface so it can be swapped without touching business logic:

```csharp
// Application/Common/Interfaces/IPaymentService.cs
public interface IPaymentService
{
    Task<Result<string>> CreatePaymentAsync(int orderId, decimal amount, CancellationToken ct);
    bool VerifyWebhookSignature(string payload, string hmacHeader);
}

// Infrastructure/Payment/PaymobPaymentService.cs
public class PaymobPaymentService : IPaymentService
{
    // Paymob-specific implementation here
}
```

To switch to Stripe: create `StripePaymentService : IPaymentService` and change one line in `DependencyInjection.cs`.

---

## 11. Module Boundaries

Each module owns its controllers, commands, queries, and repository interfaces. Cross-module communication goes through the domain model, not direct repository calls.

**Allowed:**
```csharp
// Orders handler checking product stock
var product = await _productRepository.GetByIdAsync(item.ProductId, ct);
```

**Not allowed:**
```csharp
// Orders handler directly querying products table via DbContext
var product = await _context.Products.FindAsync(item.ProductId);
// ← This bypasses the abstraction and couples modules to EF
```

---

## 12. Coding Conventions

| Convention | Rule |
|---|---|
| Async | All I/O methods are async, always pass `CancellationToken` |
| Naming | Commands: `VerbNounCommand`, Queries: `GetNounQuery`, Handlers: same + `Handler` |
| Records | Use `record` for Commands, Queries, and DTOs (immutable by default) |
| Validation | All validation in FluentValidation 11 validators, never in handlers or controllers |
| No magic strings | Use `AppRoles.Admin` not `"Admin"`, use `OrderStatus.Pending` not `"Pending"` |
| No raw SQL | All DB access through EF Core (Repository). Raw SQL only for reporting queries via Dapper if needed |
| No service locator | No `IServiceProvider` in business code — inject what you need |
