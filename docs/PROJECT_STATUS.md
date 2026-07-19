# 🖥️ LapBox - مشروع متجر اللابتوبات

## 📋 ملخص المشروع

**LapBox** هو نظام متجر إلكتروني متكامل للبيع بالتجزئة (B2C) متخصص في بيع اللابتوبات والإلكترونيات. يعتمد على:
- **.NET 10** + **ASP.NET Core Minimal APIs**
- **Clean Architecture** (CQRS Pattern)
- **Entity Framework Core** مع SQL Server
- **JWT Authentication** مع Role-based Authorization
- **MediatR** للـ Command/Query Separation
- **Redis Hybrid Caching**

---

## ✅ الوضع الحالي - APIs الخلصت

### 1️⃣ AuthController (`/api/auth`)
| Method | Endpoint | Role | Status |
|--------|----------|------|--------|
| POST | `/register` | Public | ✅ خلص |
| POST | `/login` | Public | ✅ خلص |
| POST | `/refresh` | Public | ✅ خلص |
| POST | `/logout` | Auth | ✅ خلص |
| GET | `/me` | Auth | ✅ خلص |

### 2️⃣ CustomersController (`/api/v1/customers`)
| Method | Endpoint | Role | Status |
|--------|----------|------|--------|
| GET | `/` | Manager | ✅ خلص |
| GET | `/me` | Auth | ✅ خلص |
| PUT | `/me` | Auth | ✅ خلص |
| POST | `/me/addresses` | Auth | ✅ خلص |
| GET | `/{id}` | Auth | ✅ خلص |
| PUT | `/{id}` | Manager | ✅ خلص |
| DELETE | `/{id}` | Manager | ✅ خلص |

### 3️⃣ CatalogController (`/api/v1/catalog`)
#### Brands
| Method | Endpoint | Role | Status |
|--------|----------|------|--------|
| GET | `/brands` | Public | ✅ خلص |
| GET | `/brands/{id}` | Public | ✅ خلص |
| POST | `/brands` | Manager | ✅ خلص |
| PUT | `/brands/{id}` | Manager | ✅ خلص |
| DELETE | `/brands/{id}` | Manager | ✅ خلص |

#### Categories
| Method | Endpoint | Role | Status |
|--------|----------|------|--------|
| GET | `/categories` | Public | ✅ خلص |
| GET | `/categories/{id}` | Public | ✅ خلص |
| POST | `/categories` | Manager | ✅ خلص |
| PUT | `/categories/{id}` | Manager | ✅ خلص |
| DELETE | `/categories/{id}` | Manager | ✅ خلص |

### 4️⃣ LaptopsController (`/api/v1/laptops`)
| Method | Endpoint | Role | Status |
|--------|----------|------|--------|
| GET | `/` | Public | ✅ خلص |
| GET | `/{id}` | Public | ✅ خلص |
| POST | `/` | Manager | ✅ خلص |
| PUT | `/{id}` | Manager | ✅ خلص |
| PATCH | `/{id}/inventory` | Manager | ✅ خلص |
| PATCH | `/{id}/price` | Manager | ✅ خلص |
| DELETE | `/{id}` | Manager | ✅ خلص |

### 5️⃣ CartsController (`/api/v1/cart`)
| Method | Endpoint | Role | Status |
|--------|----------|------|--------|
| GET | `/` | Auth | ✅ خلص |
| POST | `/items` | Auth | ✅ خلص |
| DELETE | `/items/{laptopId}` | Auth | ✅ خلص |
| DELETE | `/` | Auth | ✅ خلص |

### 6️⃣ OrdersController (`/api/v1/orders`)
| Method | Endpoint | Role | Status |
|--------|----------|------|--------|
| GET | `/my-orders` | Auth | ✅ خلص |
| GET | `/` | Manager | ✅ خلص |
| GET | `/{id}` | Auth | ✅ خلص |
| POST | `/place` | Auth | ⚠️ placeholder |
| PATCH | `/{id}/status` | Manager | ✅ خلص |
| DELETE | `/{id}` | Manager | ✅ خلص |

### 7️⃣ ReviewsController (`/api/v1/reviews`)
| Method | Endpoint | Role | Status |
|--------|----------|------|--------|
| GET | `/laptop/{laptopId}` | Public | ✅ خلص |
| POST | `/` | Auth | ✅ خلص |

### 8️⃣ PromotionsController (`/api/v1/promotions`)
| Method | Endpoint | Role | Status |
|--------|----------|------|--------|
| POST | `/validate` | Public | ✅ خلص |
| POST | `/` | Manager | ✅ خلص |

### 9️⃣ BillingController (`/api/v1/billing`)
| Method | Endpoint | Role | Status |
|--------|----------|------|--------|
| GET | `/invoices/{id}` | Public | ✅ خلص |
| GET | `/invoices/{id}/pdf` | Public | ✅ خلص |
| PATCH | `/invoices/{id}/pay` | Manager | ✅ خلص |

---

## ⚠️ اللي محتاج يتعمل (ناقص)

### 🔴 Critical - لازم يتعمل

#### 1. PlaceOrder Implementation
```csharp
// OrdersController.cs:130-133
// المكان: POST /api/v1/orders/place
// المشكلة: PlaceOrderRequest مش فيه Address fields
// الحل: لازم نضيف Street, City, State, ZipCode, Country للـ Request
```
**الـ Contract الحالي:**
```csharp
public sealed record PlaceOrderRequest(
    Guid CustomerId,
    string? PromotionCode);
```
**المطلوب:**
```csharp
public sealed record PlaceOrderRequest(
    Guid CustomerId,
    string? PromotionCode,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);
```

#### 2. UpdateCategoryRequest Contract
```csharp
// CatalogController.cs:216
// المكان: PUT /api/v1/catalog/categories/{id}
// المشكلة: مش واضح لو في Contract مستقل ولا لازم يتعمل
```

### 🟡 Important - ينفع يتعمل

#### 3. GetInvoicePdf - Null Reference Warning
```csharp
// BillingController.cs:60
// المشكلة: pdf.Value ممكن يكون null
// الحل: تحقق إن pdf.Value مش null قبل ما ترجع File
```

#### 4. ReviewsController - AddReview لا يرجع Review ID
```csharp
// المكان: POST /api/v1/reviews
// المشكلة: Response بيرجع 201 Created بس من غير الـ Review ID
// الحل: رجع GUID أو الـ Review object كامل
```

### 🟢 Nice to Have - تحسن أداء

#### 5. Pagination في Reviews
```csharp
// المكان: GET /api/v1/reviews/laptop/{laptopId}
// التحسين: أضف pagination (page, pageSize)
```

#### 6. Promotion Validation - يرجع Discount الفعلي
```csharp
// المكان: POST /api/v1/promotions/validate
// المشكلة: DiscountAmount دايمًا 0
// الحل: احسب الـ discount بناءً على OrderSubTotal
```

#### 7. OpenAPI Package Vulnerability
```
Warning NU1903: Package 'Microsoft.OpenApi' 2.4.1 has a known high severity vulnerability
الحل: Upgrade لـ version 2.5.0 أو أعلى
```

---

## 🏗️ Architecture Overview

```
📦 LapBox/
├── 📁 src/
│   ├── 📁 LapBox.API/              # Presentation Layer
│   │   ├── Controllers/           # 10 Controllers
│   │   ├── Extensions/            # DI, Middleware
│   │   └── Infrastructure/       # Exception Handling
│   │
│   ├── 📁 LapBox.Application/     # Application Layer
│   │   └── Features/              # CQRS Features
│   │       ├── Auth/
│   │       ├── Billing/
│   │       ├── Carts/
│   │       ├── Catalog/
│   │       ├── Customers/
│   │       ├── Laptops/
│   │       ├── Orders/
│   │       ├── Promotions/
│   │       └── Reviews/
│   │
│   ├── 📁 LapBox.Domain/           # Domain Layer
│   │   ├── Common/                # Base classes, Role enum
│   │   ├── Catalog/              # Brand, Category entities
│   │   ├── Customers/            # Customer entity
│   │   ├── Laptops/              # Laptop entity
│   │   ├── Orders/               # Order, OrderItem entities
│   │   └── Reviews/              # Review entity
│   │
│   ├── 📁 LapBox.Infrastructure/  # Infrastructure Layer
│   │   ├── Data/                 # EF Core, Migrations
│   │   ├── Services/             # Identity, PDF, Storage
│   │   └── BackgroundServices/   # Scheduled tasks
│   │
│   └── 📁 LapBox.Contracts/      # API Contracts
│       ├── Auth/
│       ├── Billing/
│       ├── Carts/
│       ├── Catalog/
│       ├── Customers/
│       ├── Laptops/
│       ├── Orders/
│       ├── Promotions/
│       └── Reviews/
│
├── 📁 tests/                      # Unit Tests
│   ├── LapBox.Domain.Tests/
│   ├── LapBox.Application.Tests/
│   └── LapBox.Infrastructure.Tests/
│
└── 📁 docs/                       # Documentation
```

---

## 🔐 Authorization System

### Roles
```csharp
public enum Role
{
    Customer,   // عميل عادي
    Manager,    // مدير المتجر
    Admin       // مدير النظام
}
```

### Policies
```csharp
// Infrastructure/DependencyInjection.cs:100-104
.AddPolicy("ManagerOnly", policy =>
    policy.RequireRole("Manager", "Admin"))
.AddPolicy("AdminOnly", policy =>
    policy.RequireRole("Admin"));
```

### ملاحظة مهمة
```csharp
// ❌ غلط - Role-based direct (محتاج config إضافي)
[Authorize(Roles = nameof(Role.Manager))]

// ✅ صح - Policy-based (المستخدم في المشروع)
[Authorize(Policy = "AdminOnly")]
```

---

## 📊 Statistics

| Metric | Count |
|--------|-------|
| Total Controllers | 10 |
| Total APIs | ~40 |
| APIs خلصت | ~38 |
| APIs ناقصة | ~2 |
| Build Status | ✅ Success |
| Tests | ✅ 3/3 Passed |

---

## 🚀 Next Steps

1. **[HIGH]** إكمال `PlaceOrder` - إضافة Address fields للـ Request
2. **[MEDIUM]** إصلاح `GetInvoicePdf` null warning
3. **[MEDIUM]** Return Review ID from AddReview
4. **[LOW]** Upgrade OpenAPI package
5. **[LOW]** Add pagination to Reviews

---

## 📞 APIs Documentation

الـ API Documentation متاحة عبر:
- **Swagger UI**: `/swagger`
- **Scalar API Reference**: `/scalar`
- **OpenAPI JSON**: `/openapi/v1.json`

---

**Generated:** 2026-07-10
**Last Updated By:** Mavis AI Assistant
