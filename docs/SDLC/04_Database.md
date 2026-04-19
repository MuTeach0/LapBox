# LapBox — Database Design

> DBMS: SQL Server · Schema: `shop` · Database: `ShopDB`
> ORM: Entity Framework Core 10

---

## 1. Design Decisions

| Decision | Choice | Reason |
|---|---|---|
| DBMS | SQL Server | Relational, ACID, existing team knowledge |
| Schema | All tables under `shop` schema | Namespace isolation, easier permission management |
| IDs | `INT IDENTITY` | Simple, fast clustered index, no GUID fragmentation |
| Strings | `NVARCHAR` throughout | Arabic product names and customer data |
| Soft Delete | `IsDeleted BIT` on Products | Orders reference products — hard delete breaks history |
| Basket | Redis only — no SQL table | Temporary data, sub-millisecond access, auto-expiry |
| Order Status | Lookup table `OrderStatuses` | Controlled vocabulary, self-documenting |
| Money | `DECIMAL(10,2)` | Exact representation, no floating point errors |
| Dates | `DATETIME2` | Higher precision, timezone-safe |

---

## 2. Complete Schema

### shop.Categories
```sql
CREATE TABLE shop.Categories
(
    CategoryId   INT IDENTITY(1,1) NOT NULL,
    CategoryName NVARCHAR(100)     NOT NULL,
    TaxRate      DECIMAL(5,2)      NOT NULL,

    CONSTRAINT PK_Categories PRIMARY KEY CLUSTERED (CategoryId)
);
```

---

### shop.Suppliers
```sql
CREATE TABLE shop.Suppliers
(
    SupplierId   INT IDENTITY(1,1) NOT NULL,
    SupplierName NVARCHAR(150)     NOT NULL,
    Phone        NVARCHAR(20)      NOT NULL,

    CONSTRAINT PK_Suppliers PRIMARY KEY CLUSTERED (SupplierId)
);
```

---

### shop.Shippers
```sql
CREATE TABLE shop.Shippers
(
    ShipperId   INT IDENTITY(1,1) NOT NULL,
    ShipperName NVARCHAR(150)     NOT NULL,

    CONSTRAINT PK_Shippers PRIMARY KEY CLUSTERED (ShipperId)
);
```

---

### shop.Tags
```sql
CREATE TABLE shop.Tags
(
    TagId   INT IDENTITY(1,1) NOT NULL,
    TagName NVARCHAR(50)      NOT NULL,

    CONSTRAINT PK_Tags PRIMARY KEY CLUSTERED (TagId)
);
```

---

### shop.OrderStatuses  ← NEW (replaces free-text Status column)
```sql
CREATE TABLE shop.OrderStatuses
(
    StatusCode NVARCHAR(30)  NOT NULL,
    Label      NVARCHAR(100) NOT NULL,
    SortOrder  INT           NOT NULL,

    CONSTRAINT PK_OrderStatuses PRIMARY KEY CLUSTERED (StatusCode)
);

INSERT INTO shop.OrderStatuses (StatusCode, Label, SortOrder) VALUES
('Pending',    'Pending Confirmation', 1),
('Confirmed',  'Order Confirmed',      2),
('Processing', 'Being Prepared',       3),
('Shipped',    'Out for Delivery',     4),
('Delivered',  'Delivered',            5),
('Cancelled',  'Cancelled',            6),
('Refunded',   'Refunded',             7);
```

---

### shop.Customers
```sql
CREATE TABLE shop.Customers
(
    CustomerId INT IDENTITY(1,1) NOT NULL,
    FullName   NVARCHAR(150)     NOT NULL,
    Email      NVARCHAR(255)     NOT NULL,

    CONSTRAINT PK_Customers  PRIMARY KEY CLUSTERED (CustomerId),
    CONSTRAINT UQ_Customers_Email UNIQUE (Email)
);
-- Address removed — moved to CustomerAddresses table
```

---

### shop.CustomerAddresses  ← NEW
```sql
CREATE TABLE shop.CustomerAddresses
(
    AddressId  INT IDENTITY(1,1) NOT NULL,
    CustomerId INT               NOT NULL,
    Label      NVARCHAR(50)      NOT NULL,   -- 'Home', 'Work', etc.
    Street     NVARCHAR(300)     NOT NULL,
    City       NVARCHAR(100)     NOT NULL,
    Country    NVARCHAR(100)     NOT NULL,
    IsDefault  BIT               NOT NULL DEFAULT 0,

    CONSTRAINT PK_CustomerAddresses PRIMARY KEY CLUSTERED (AddressId),
    CONSTRAINT FK_CustomerAddresses_Customers
        FOREIGN KEY (CustomerId)
        REFERENCES shop.Customers(CustomerId)
        ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX IX_CustomerAddresses_CustomerId
    ON shop.CustomerAddresses(CustomerId);
```

---

### shop.CustomerPhones
```sql
CREATE TABLE shop.CustomerPhones
(
    CustomerId  INT          NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    PhoneType   NVARCHAR(20) NOT NULL,

    CONSTRAINT PK_CustomerPhones PRIMARY KEY CLUSTERED (CustomerId, PhoneNumber),
    CONSTRAINT FK_CustomerPhones_Customers
        FOREIGN KEY (CustomerId)
        REFERENCES shop.Customers(CustomerId)
        ON DELETE CASCADE
);
```

---

### shop.Products
```sql
CREATE TABLE shop.Products
(
    ProductId       INT IDENTITY(1,1) NOT NULL,
    ProductName     NVARCHAR(200)     NOT NULL,
    CategoryId      INT               NOT NULL,
    SupplierId      INT               NOT NULL,
    [Description]   NVARCHAR(500)     NULL,
    Price           DECIMAL(10,2)     NOT NULL,
    QuantityInStock INT               NOT NULL,
    IsDeleted       BIT               NOT NULL DEFAULT 0,   -- ← NEW: soft delete
    CreatedAt       DATETIME2         NOT NULL DEFAULT SYSDATETIME(),
    UpdatedAt       DATETIME2         NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Products PRIMARY KEY CLUSTERED (ProductId),
    CONSTRAINT FK_Products_Categories
        FOREIGN KEY (CategoryId) REFERENCES shop.Categories(CategoryId),
    CONSTRAINT FK_Products_Suppliers
        FOREIGN KEY (SupplierId) REFERENCES shop.Suppliers(SupplierId),
    CONSTRAINT CHK_Products_Price
        CHECK (Price > 0),
    CONSTRAINT CHK_Products_Stock
        CHECK (QuantityInStock >= 0)
);

CREATE NONCLUSTERED INDEX IX_Products_CategoryId
    ON shop.Products(CategoryId)
    WHERE IsDeleted = 0;

CREATE NONCLUSTERED INDEX IX_Products_SupplierId    -- ← NEW
    ON shop.Products(SupplierId);

CREATE NONCLUSTERED INDEX IX_Products_IsDeleted     -- ← NEW: filter index
    ON shop.Products(IsDeleted)
    WHERE IsDeleted = 0;
```

---

### shop.ProductVariants  ← NEW
```sql
CREATE TABLE shop.ProductVariants
(
    VariantId       INT IDENTITY(1,1) NOT NULL,
    ProductId       INT               NOT NULL,
    RAM             NVARCHAR(20)      NOT NULL,   -- '8GB', '16GB', '32GB'
    Storage         NVARCHAR(20)      NOT NULL,   -- '256GB SSD', '512GB SSD'
    PriceModifier   DECIMAL(10,2)     NOT NULL DEFAULT 0,
    QuantityInStock INT               NOT NULL DEFAULT 0,

    CONSTRAINT PK_ProductVariants PRIMARY KEY CLUSTERED (VariantId),
    CONSTRAINT FK_ProductVariants_Products
        FOREIGN KEY (ProductId)
        REFERENCES shop.Products(ProductId)
        ON DELETE CASCADE,
    CONSTRAINT CHK_Variants_Stock
        CHECK (QuantityInStock >= 0)
);

CREATE NONCLUSTERED INDEX IX_ProductVariants_ProductId
    ON shop.ProductVariants(ProductId);
```

---

### shop.ProductImages
```sql
CREATE TABLE shop.ProductImages
(
    ProductId INT            NOT NULL,
    ImageUrl  NVARCHAR(400)  NOT NULL,
    IsPrimary BIT            NOT NULL DEFAULT 0,   -- ← NEW: flag for main image

    CONSTRAINT PK_ProductImages PRIMARY KEY CLUSTERED (ProductId, ImageUrl),
    CONSTRAINT FK_ProductImages_Products
        FOREIGN KEY (ProductId)
        REFERENCES shop.Products(ProductId)
        ON DELETE CASCADE
);
```

---

### shop.ProductTags
```sql
CREATE TABLE shop.ProductTags
(
    ProductId INT NOT NULL,
    TagId     INT NOT NULL,

    CONSTRAINT PK_ProductTags PRIMARY KEY CLUSTERED (ProductId, TagId),
    CONSTRAINT FK_ProductTags_Products
        FOREIGN KEY (ProductId) REFERENCES shop.Products(ProductId) ON DELETE CASCADE,
    CONSTRAINT FK_ProductTags_Tags
        FOREIGN KEY (TagId) REFERENCES shop.Tags(TagId)
);

CREATE NONCLUSTERED INDEX IX_ProductTags_TagId
    ON shop.ProductTags(TagId);
```

---

### shop.Orders
```sql
CREATE TABLE shop.Orders
(
    OrderId         INT IDENTITY(1,1) NOT NULL,
    CustomerId      INT               NOT NULL,
    ShipperId       INT               NOT NULL,
    AddressId       INT               NOT NULL,          -- ← FK to CustomerAddresses
    OrderDate       DATETIME2         NOT NULL DEFAULT SYSDATETIME(),
    StatusCode      NVARCHAR(30)      NOT NULL DEFAULT 'Pending',  -- ← FK to OrderStatuses
    TotalAmount     DECIMAL(12,2)     NOT NULL,
    DiscountAmount  DECIMAL(12,2)     NOT NULL DEFAULT 0,          -- ← NEW
    CouponId        INT               NULL,                         -- ← NEW

    CONSTRAINT PK_Orders PRIMARY KEY CLUSTERED (OrderId),
    CONSTRAINT FK_Orders_Customers
        FOREIGN KEY (CustomerId) REFERENCES shop.Customers(CustomerId),
    CONSTRAINT FK_Orders_Shippers
        FOREIGN KEY (ShipperId) REFERENCES shop.Shippers(ShipperId),
    CONSTRAINT FK_Orders_Addresses
        FOREIGN KEY (AddressId) REFERENCES shop.CustomerAddresses(AddressId),
    CONSTRAINT FK_Orders_OrderStatuses
        FOREIGN KEY (StatusCode) REFERENCES shop.OrderStatuses(StatusCode),
    CONSTRAINT FK_Orders_Coupons
        FOREIGN KEY (CouponId) REFERENCES shop.Coupons(CouponId)
);

CREATE NONCLUSTERED INDEX IX_Orders_CustomerId
    ON shop.Orders(CustomerId);

CREATE NONCLUSTERED INDEX IX_Orders_OrderDate       -- ← NEW
    ON shop.Orders(OrderDate DESC);

CREATE NONCLUSTERED INDEX IX_Orders_StatusCode      -- ← NEW
    ON shop.Orders(StatusCode);
```

---

### shop.OrderItems
```sql
CREATE TABLE shop.OrderItems
(
    OrderId   INT           NOT NULL,
    ProductId INT           NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Quantity  INT           NOT NULL,
    Discount  DECIMAL(5,2)  NOT NULL DEFAULT 0,

    CONSTRAINT PK_OrderItems PRIMARY KEY CLUSTERED (OrderId, ProductId),
    CONSTRAINT FK_OrderItems_Orders
        FOREIGN KEY (OrderId)
        REFERENCES shop.Orders(OrderId)
        ON DELETE CASCADE,
    CONSTRAINT FK_OrderItems_Products
        FOREIGN KEY (ProductId)
        REFERENCES shop.Products(ProductId)
);

CREATE NONCLUSTERED INDEX IX_OrderItems_ProductId
    ON shop.OrderItems(ProductId);

CREATE NONCLUSTERED INDEX IX_OrderItems_OrderId     -- ← NEW
    ON shop.OrderItems(OrderId);
```

---

### shop.Payments
```sql
CREATE TABLE shop.Payments
(
    OrderId       INT            NOT NULL,
    Amount        DECIMAL(12,2)  NOT NULL,
    Method        NVARCHAR(30)   NOT NULL,
    PaymentDate   DATETIME2      NOT NULL DEFAULT SYSDATETIME(),
    TransactionId NVARCHAR(100)  NOT NULL,
    [Status]      NVARCHAR(20)   NOT NULL DEFAULT 'Pending',  -- ← NEW: Pending/Completed/Failed/Refunded

    CONSTRAINT PK_Payments PRIMARY KEY CLUSTERED (OrderId),
    CONSTRAINT FK_Payments_Orders
        FOREIGN KEY (OrderId) REFERENCES shop.Orders(OrderId),
    CONSTRAINT UQ_Payments_TransactionId
        UNIQUE (TransactionId)  -- ← NEW: idempotency
);
```

---

### shop.ShippingEligibilities
```sql
CREATE TABLE shop.ShippingEligibilities
(
    ProductId  INT          NOT NULL,
    ShipperId  INT          NOT NULL,
    RegionCode NVARCHAR(10) NOT NULL,

    CONSTRAINT PK_ShippingEligibilities PRIMARY KEY CLUSTERED (ProductId, ShipperId, RegionCode),
    CONSTRAINT FK_ShippingEligibilities_Products
        FOREIGN KEY (ProductId) REFERENCES shop.Products(ProductId),
    CONSTRAINT FK_ShippingEligibilities_Shippers
        FOREIGN KEY (ShipperId) REFERENCES shop.Shippers(ShipperId)
);

CREATE NONCLUSTERED INDEX IX_ShippingEligibilities_ShipperId
    ON shop.ShippingEligibilities(ShipperId);
```

---

### shop.Reviews  ← NEW
```sql
CREATE TABLE shop.Reviews
(
    ReviewId   INT IDENTITY(1,1) NOT NULL,
    ProductId  INT               NOT NULL,
    CustomerId INT               NOT NULL,
    Rating     TINYINT           NOT NULL,
    Title      NVARCHAR(200)     NULL,
    Body       NVARCHAR(2000)    NULL,
    CreatedAt  DATETIME2         NOT NULL DEFAULT SYSDATETIME(),
    IsVerified BIT               NOT NULL DEFAULT 0,

    CONSTRAINT PK_Reviews PRIMARY KEY CLUSTERED (ReviewId),
    CONSTRAINT FK_Reviews_Products
        FOREIGN KEY (ProductId) REFERENCES shop.Products(ProductId),
    CONSTRAINT FK_Reviews_Customers
        FOREIGN KEY (CustomerId) REFERENCES shop.Customers(CustomerId),
    CONSTRAINT UQ_Reviews_OnePerCustomerProduct
        UNIQUE (ProductId, CustomerId),
    CONSTRAINT CHK_Reviews_Rating
        CHECK (Rating BETWEEN 1 AND 5)
);

CREATE NONCLUSTERED INDEX IX_Reviews_ProductId
    ON shop.Reviews(ProductId);

CREATE NONCLUSTERED INDEX IX_Reviews_CustomerId
    ON shop.Reviews(CustomerId);
```

---

### shop.Wishlists  ← NEW
```sql
CREATE TABLE shop.Wishlists
(
    CustomerId INT       NOT NULL,
    ProductId  INT       NOT NULL,
    AddedAt    DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

    CONSTRAINT PK_Wishlists PRIMARY KEY CLUSTERED (CustomerId, ProductId),
    CONSTRAINT FK_Wishlists_Customers
        FOREIGN KEY (CustomerId) REFERENCES shop.Customers(CustomerId) ON DELETE CASCADE,
    CONSTRAINT FK_Wishlists_Products
        FOREIGN KEY (ProductId) REFERENCES shop.Products(ProductId)
);

CREATE NONCLUSTERED INDEX IX_Wishlists_CustomerId
    ON shop.Wishlists(CustomerId);
```

---

### shop.Coupons  ← NEW
```sql
CREATE TABLE shop.Coupons
(
    CouponId       INT IDENTITY(1,1) NOT NULL,
    Code           NVARCHAR(50)      NOT NULL,
    DiscountType   NVARCHAR(20)      NOT NULL,    -- 'Percentage' or 'Fixed'
    DiscountValue  DECIMAL(10,2)     NOT NULL,
    MinOrderAmount DECIMAL(12,2)     NULL,
    MaxUses        INT               NULL,        -- NULL = unlimited
    UsedCount      INT               NOT NULL DEFAULT 0,
    ExpiresAt      DATETIME2         NULL,        -- NULL = no expiry
    IsActive       BIT               NOT NULL DEFAULT 1,

    CONSTRAINT PK_Coupons PRIMARY KEY CLUSTERED (CouponId),
    CONSTRAINT UQ_Coupons_Code UNIQUE (Code),
    CONSTRAINT CHK_Coupons_Type
        CHECK (DiscountType IN ('Percentage', 'Fixed')),
    CONSTRAINT CHK_Coupons_Value
        CHECK (DiscountValue > 0)
);

CREATE NONCLUSTERED INDEX IX_Coupons_Code
    ON shop.Coupons(Code);
```

---

### shop.OrdersArchive  (existing — kept as-is)
```sql
CREATE TABLE shop.OrdersArchive
(
    OrderId         INT           NOT NULL,
    CustomerId      INT           NOT NULL,
    ShipperId       INT           NOT NULL,
    OrderDate       DATETIME2     NOT NULL,
    ShippingAddress NVARCHAR(300) NOT NULL,
    StatusCode      NVARCHAR(30)  NOT NULL,
    TotalAmount     DECIMAL(12,2) NOT NULL,
    ArchivedAt      DATETIME2     NOT NULL,

    CONSTRAINT PK_OrdersArchive PRIMARY KEY CLUSTERED (OrderId)
);
```

---

## 3. Complete Index Inventory

| Index | Table | Column(s) | Type | Purpose |
|---|---|---|---|---|
| PK_* | All | Primary key | Clustered | Row lookup |
| UQ_Customers_Email | Customers | Email | Unique | Login lookup, duplicate check |
| IX_Products_CategoryId | Products | CategoryId | Non-clustered, filtered (IsDeleted=0) | Category filter |
| IX_Products_SupplierId | Products | SupplierId | Non-clustered | Supplier filter |
| IX_Products_IsDeleted | Products | IsDeleted | Non-clustered, filtered | Active product queries |
| IX_ProductVariants_ProductId | ProductVariants | ProductId | Non-clustered | Load variants for product |
| IX_ProductTags_TagId | ProductTags | TagId | Non-clustered | Tag filter |
| IX_Orders_CustomerId | Orders | CustomerId | Non-clustered | Order history per customer |
| IX_Orders_OrderDate | Orders | OrderDate DESC | Non-clustered | Date-range reports |
| IX_Orders_StatusCode | Orders | StatusCode | Non-clustered | Status filter (admin panel) |
| IX_OrderItems_ProductId | OrderItems | ProductId | Non-clustered | Product sales reporting |
| IX_OrderItems_OrderId | OrderItems | OrderId | Non-clustered | Load items for order |
| IX_Reviews_ProductId | Reviews | ProductId | Non-clustered | Reviews on product page |
| IX_Reviews_CustomerId | Reviews | CustomerId | Non-clustered | Customer's reviews |
| IX_Wishlists_CustomerId | Wishlists | CustomerId | Non-clustered | Customer's wishlist |
| IX_Coupons_Code | Coupons | Code | Non-clustered | Coupon lookup at checkout |
| IX_CustomerAddresses_CustomerId | CustomerAddresses | CustomerId | Non-clustered | Load customer addresses |
| IX_ShippingEligibilities_ShipperId | ShippingEligibilities | ShipperId | Non-clustered | Shipper lookup |
| UQ_Payments_TransactionId | Payments | TransactionId | Unique | Payment idempotency |
| UQ_Reviews_OnePerCustomerProduct | Reviews | ProductId, CustomerId | Unique | Prevent duplicate reviews |

---

## 4. Entity Relationship Summary

```
Categories (1) ──────── (N) Products (N) ──── (N) Tags
                               │  (N) ──── (N) ProductImages
Suppliers  (1) ──────── (N) Products
                               │ (1) ──── (N) ProductVariants
                               │ (N) ──── (N) Shippers [via ShippingEligibilities]
                               │ (1) ──── (N) Reviews
                               │ (N) ──── (N) Wishlists

Customers (1) ──── (N) CustomerAddresses
Customers (1) ──── (N) CustomerPhones
Customers (1) ──── (N) Orders
Customers (1) ──── (N) Reviews
Customers (1) ──── (N) Wishlists

Orders (1) ──── (N) OrderItems ──── (1) Products
Orders (1) ──── (1) Payments
Orders (N) ──── (1) Coupons
Orders (N) ──── (1) CustomerAddresses
Orders (N) ──── (1) OrderStatuses [lookup]

BASKET: Redis only — no SQL table
```

---

## 5. EF Core Configuration Notes

Each entity has its own `IEntityTypeConfiguration<T>` file under `Infrastructure/Persistence/Configurations/`.

Example for Product:
```csharp
// Infrastructure/Persistence/Configurations/ProductConfiguration.cs
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", "shop");
        builder.HasKey(p => p.ProductId);

        builder.Property(p => p.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Price).HasColumnType("decimal(10,2)");
        builder.Property(p => p.QuantityInStock).IsRequired();
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);

        // Global query filter for soft delete
        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId);

        builder.HasMany(p => p.Images)
               .WithOne(i => i.Product)
               .HasForeignKey(i => i.ProductId);

        builder.HasMany(p => p.Variants)
               .WithOne(v => v.Product)
               .HasForeignKey(v => v.ProductId);
    }
}
```

The `HasQueryFilter(p => !p.IsDeleted)` means every query on Products automatically excludes soft-deleted records without any developer needing to remember to add `.Where(p => !p.IsDeleted)`.

---

## 6. Migration Strategy

- One migration per feature branch — never multiple unrelated changes in one migration
- Migration naming convention: `YYYYMMDD_DescriptiveName` (e.g., `20260418_AddReviewsTable`)
- Seed data (admin user, order statuses, initial categories) in a separate `Seeds/` folder run after migrations
- Never edit an already-applied migration — always add a new one
