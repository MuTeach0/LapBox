# LapBox — Requirements

> This document is the single source of truth for what the system must do.
> Every feature here maps to tasks in `06_Task_Breakdown.md` and endpoints in `05_API_Contracts.md`.

---

## 1. User Stories & Acceptance Criteria

### Module: Auth

---

**US-AUTH-01 — Customer Registration**
> As a new visitor, I want to create an account so I can place orders and track them.

Acceptance Criteria:
- Customer provides full name, email, and password
- Email must be unique in the system — duplicate returns `409 Conflict` with ProblemDetails
- Password must be at least 8 characters, contain one uppercase letter and one digit
- On success, system returns a JWT access token and a refresh token
- Password is never stored in plaintext (Identity handles hashing)
- Welcome email is sent after successful registration (async, does not block response)

---

**US-AUTH-02 — Customer Login**
> As a registered customer, I want to log in with my email and password.

Acceptance Criteria:
- System returns JWT access token (15-minute expiry) and refresh token (7-day expiry) on success
- Invalid credentials return `401 Unauthorized` — no hint about which field is wrong
- After 5 consecutive failed attempts, account is locked for 10 minutes
- Locked account returns `423 Locked` with a message and unlock time

---

**US-AUTH-03 — Token Refresh**
> As a logged-in customer, I want my session to stay alive without re-entering my password.

Acceptance Criteria:
- Customer sends a valid refresh token and receives a new access token + new refresh token
- Old refresh token is invalidated immediately after use (rotation)
- Expired or invalid refresh token returns `401 Unauthorized`

---

**US-AUTH-04 — Admin Login**
> As the store admin, I want to log in with elevated privileges.

Acceptance Criteria:
- Same login endpoint as customers
- JWT payload contains `role: Admin`
- Admin endpoints return `403 Forbidden` when called with a Customer token
- There is no public registration for Admin — Admin accounts are seeded or created via a protected CLI command

---

### Module: Catalog

---

**US-CAT-01 — Browse Products**
> As a customer, I want to see a list of available laptops with basic info and price.

Acceptance Criteria:
- Returns paginated list (default page size: 12, max: 48)
- Each item shows: ProductId, ProductName, Price, primary image URL, average rating, category name
- Out-of-stock products appear in results but are marked `inStock: false`
- Response time < 1.5 seconds (cache first, then DB)

---

**US-CAT-02 — Filter & Sort Products**
> As a customer, I want to narrow down products by category, price range, and specs.

Acceptance Criteria:
- Filter by: categoryId, minPrice, maxPrice, tagIds (comma-separated), inStock (bool)
- Sort by: price_asc, price_desc, rating_desc, newest
- Filters and sort can be combined freely
- Invalid filter values return `400 Bad Request` with field-level errors

---

**US-CAT-03 — Search Products**
> As a customer, I want to search by product name or keyword.

Acceptance Criteria:
- Search is case-insensitive and supports partial matches
- Searches ProductName and Description fields
- Returns same shape as US-CAT-01 list items
- Empty results return `200 OK` with empty array (not 404)

---

**US-CAT-04 — View Product Detail**
> As a customer, I want to see full specs, all images, and available variants before buying.

Acceptance Criteria:
- Returns: all product fields, all images, all variants (RAM/Storage/price modifier/stock), average rating, review count, tags
- If product does not exist, returns `404 Not Found`
- Response is cached in Redis for 5 minutes (invalidated on admin update)

---

**US-CAT-05 — Admin: Create Product**
> As an admin, I want to add a new laptop to the catalogue.

Acceptance Criteria:
- Required fields: ProductName, CategoryId, SupplierId, Price, QuantityInStock
- Optional: Description, images (URLs), tags (IDs), variants
- CategoryId and SupplierId must exist — invalid references return `422` with field detail
- Returns `201 Created` with the new product's full detail
- Admin role required — `403` otherwise

---

**US-CAT-06 — Admin: Update Product**
> As an admin, I want to edit product details, price, or stock.

Acceptance Criteria:
- Partial update supported (only provided fields are changed)
- Updating stock below zero is rejected with `422`
- On success, Redis cache for this product is invalidated
- Returns updated product detail

---

**US-CAT-07 — Admin: Delete Product**
> As an admin, I want to remove a product that is no longer sold.

Acceptance Criteria:
- Product with active orders (status not Delivered/Cancelled) cannot be deleted — returns `409 Conflict`
- Soft delete preferred: `IsDeleted = true`, product disappears from customer-facing endpoints
- Returns `204 No Content` on success

---

### Module: Basket

---

**US-BAS-01 — Add Item to Basket**
> As a customer, I want to add a product to my basket.

Acceptance Criteria:
- Basket is identified by CustomerId (from JWT) — no anonymous baskets in v1
- Adding same product again increments quantity (does not duplicate)
- Stock is NOT reserved — stock check happens only at checkout
- Basket stored in Redis with 7-day TTL (refreshed on every write)
- Adding a non-existent product returns `404`

---

**US-BAS-02 — View Basket**
> As a customer, I want to see everything in my basket with current prices.

Acceptance Criteria:
- Returns all items with: productId, productName, primaryImage, unitPrice, quantity, subtotal
- Prices are fetched fresh from DB on each view (not stored in Redis) to reflect price changes
- Also returns basket total and item count

---

**US-BAS-03 — Update Item Quantity**
> As a customer, I want to change the quantity of an item in my basket.

Acceptance Criteria:
- Setting quantity to 0 removes the item
- Quantity cannot exceed 99 per line item — `422` if exceeded
- Returns updated basket

---

**US-BAS-04 — Remove Item / Clear Basket**
> As a customer, I want to remove a specific item or empty my entire basket.

Acceptance Criteria:
- Remove single item: item deleted from Redis hash
- Clear all: Redis key deleted
- Both return `204 No Content`

---

### Module: Orders

---

**US-ORD-01 — Place Order (Checkout)**
> As a customer, I want to convert my basket into a confirmed order and pay.

Acceptance Criteria:
- Customer selects a shipping address (from their saved addresses) and a shipper
- Optional: apply coupon code
- System performs atomic stock check + decrement in a SQL transaction
- If any item is out of stock at this moment, order is rejected with `422` listing the specific products
- If coupon is invalid/expired, order is rejected with `422` and reason
- On success: order record created, basket cleared from Redis, payment initiated
- Returns `201 Created` with OrderId and payment URL (Paymob checkout link)

---

**US-ORD-02 — Payment Callback**
> As the system, I need to handle Paymob's webhook to confirm or fail a payment.

Acceptance Criteria:
- Paymob sends a POST callback with HMAC signature
- System verifies HMAC before processing — invalid signature returns `400` immediately
- On confirmed payment: order status moves to `Confirmed`, Payment record created
- On failed payment: order status moves to `Cancelled`, stock is restored
- Idempotent: processing the same transaction ID twice has no effect

---

**US-ORD-03 — View Order History**
> As a customer, I want to see all my past and current orders.

Acceptance Criteria:
- Returns paginated list sorted by OrderDate descending
- Each item: orderId, orderDate, status, totalAmount, item count
- Only the authenticated customer's own orders are returned

---

**US-ORD-04 — View Order Detail**
> As a customer, I want to see full detail of a specific order.

Acceptance Criteria:
- Returns: order info, all order items (with product name + image), shipping address, payment method, status history
- Returns `404` if orderId does not belong to the authenticated customer
- Admin can view any order

---

**US-ORD-05 — Admin: Update Order Status**
> As an admin, I want to move an order through its lifecycle.

Acceptance Criteria:
- Valid transitions only: Pending → Confirmed → Processing → Shipped → Delivered
- Cancelled and Refunded are terminal states — no further transitions allowed
- Invalid transition returns `422` with current and requested status
- Status change is logged with timestamp

---

**US-ORD-06 — Admin: Cancel Order**
> As an admin, I want to cancel an order and restore stock.

Acceptance Criteria:
- Only Pending or Confirmed orders can be cancelled
- Stock is restored for all items in the cancelled order
- If a payment was made, it is marked for refund (manual process in v1 — just flagged)
- Returns `200 OK` with updated order

---

### Module: Reviews & Wishlist

---

**US-REV-01 — Submit Review**
> As a customer, I want to rate and review a product I purchased.

Acceptance Criteria:
- Customer must have a completed order containing the product (IsVerified check)
- Rating is 1–5 (integer) — out of range returns `422`
- One review per customer per product — duplicate returns `409 Conflict`
- Title is optional (max 200 chars), Body is optional (max 2000 chars)
- Returns `201 Created`

---

**US-REV-02 — View Product Reviews**
> As a customer, I want to read other buyers' reviews on a product page.

Acceptance Criteria:
- Returns paginated list (page size 10) sorted by newest first
- Each review: rating, title, body, customer first name, date, isVerified badge
- Summary at top: averageRating, totalReviews, distribution (count per star)

---

**US-WIS-01 — Add to Wishlist**
> As a customer, I want to save products I'm interested in buying later.

Acceptance Criteria:
- Adding same product again returns `200 OK` (idempotent, no error)
- Returns updated wishlist item count

---

**US-WIS-02 — View Wishlist**
> As a customer, I want to see all my saved products.

Acceptance Criteria:
- Returns list with: productId, name, price, primaryImage, inStock status
- If a product was deleted since it was wishlisted, it is excluded silently

---

### Module: Admin

---

**US-ADM-01 — Sales Dashboard**
> As an admin, I want to see today's revenue, order count, and top products at a glance.

Acceptance Criteria:
- Returns: total revenue (all time, this month, today), order counts by status, top 5 products by revenue
- Data is cached for 10 minutes (acceptable staleness for a dashboard)

---

**US-ADM-02 — Manage Coupons**
> As an admin, I want to create discount codes for promotions.

Acceptance Criteria:
- Coupon has: code (unique), type (Percentage or Fixed), value, minOrderAmount, maxUses, expiresAt
- Creating duplicate code returns `409 Conflict`
- Deactivating a coupon makes it immediately unusable (IsActive = false)

---

## 2. System-Level Requirements

### Error Response Format (All Endpoints)

All errors follow RFC 7807 ProblemDetails. Never throw unhandled exceptions to the client.

```json
{
  "type": "https://lapbox.com/errors/validation",
  "title": "One or more validation errors occurred.",
  "status": 422,
  "traceId": "0HN2V...",
  "errors": {
    "price": ["Price must be greater than 0"],
    "categoryId": ["Category not found"]
  }
}
```

### Authentication Rules

- All customer endpoints require `Authorization: Bearer <token>` unless marked `[AllowAnonymous]`
- All admin endpoints require `Authorization: Bearer <token>` with `role: Admin`
- Unauthenticated requests return `401 Unauthorized`
- Authenticated customers accessing admin endpoints return `403 Forbidden`

### Pagination Standard

All list endpoints follow this query parameter convention:

```
GET /api/products?page=1&pageSize=12&sortBy=price_asc
```

All list responses follow this envelope:

```json
{
  "items": [...],
  "page": 1,
  "pageSize": 12,
  "totalCount": 84,
  "totalPages": 7
}
```

### Stock Rule

Stock is only decremented at checkout inside an atomic DB transaction. Basket does not reserve stock. The business accepts the rare edge case of two customers reaching checkout simultaneously for the last item — the second one gets a clear `422` error.

---

## 3. Data Validation Summary

| Field | Rule |
|---|---|
| Email | Valid format, max 255 chars, unique |
| Password | Min 8 chars, 1 uppercase, 1 digit |
| ProductName | Required, max 200 chars |
| Price | Decimal > 0, max 10 digits (2 decimal places) |
| Quantity | Integer ≥ 1 |
| Rating | Integer 1–5 |
| CouponCode | Required, max 50 chars, alphanumeric + dash |
| PhoneNumber | Required, max 20 chars |
