# LapBox — Product Requirements Document (PRD)

> **Version:** 1.0
> **Status:** Approved for Development
> **Stack:** .NET 10 · SQL Server · Redis · Clean Architecture + CQRS
> **Team:** 3 Junior Developers

---

## 1. Product Vision

**LapBox** is a single-vendor e-commerce platform purpose-built for selling laptops online in the Egyptian market.

**Core promise to the customer:** Find the right laptop, buy it confidently, and track it to your door — in under 5 minutes.

**Core promise to the business owner:** A fully managed storefront that handles inventory, orders, and payments automatically — no manual work after setup.

---

## 2. Problem Statement

| Who | Problem |
|---|---|
| Customer | Existing laptop stores (physical and online) lack clear specs, real reviews, and transparent stock info. Customers waste time only to find items are out of stock. |
| Store Owner | Managing a spreadsheet for inventory and WhatsApp for orders is not scalable. There is no visibility into which products drive revenue. |

---

## 3. Target Users

### 3.1 Customer Persona
**Name:** Ahmed, 26, Software Engineer in Cairo

- Needs a powerful laptop for development and gaming
- Researches heavily before buying — reads specs, compares variants, checks reviews
- Wants to know if an item is actually in stock before getting excited
- Prefers paying by card online
- Wants to track his order without calling anyone

**Key Jobs To Be Done:**
- Find a laptop that fits his exact specs (RAM / Storage)
- Trust that the reviews are real (verified purchase badge)
- Complete checkout in one flow without being redirected 5 times
- Know exactly when his order ships

### 3.2 Admin Persona
**Name:** Karim, Store Manager

- Needs to add new products quickly with images, variants, and specs
- Wants to see which products are running low on stock
- Needs to move orders through statuses (Confirmed → Shipped → Delivered)
- Wants to run promotions with coupon codes during back-to-school season
- Wants a daily dashboard: how much revenue today, how many pending orders

---

## 4. User Journey Maps

### 4.1 Customer: First Purchase

```
Lands on site
    → Browses product listing (filtered by budget + RAM)
    → Opens product detail page (reads specs, variants, reviews)
    → Adds preferred variant to basket
    → Views basket (confirms items + total)
    → Proceeds to checkout
        → Selects saved address (or adds new one)
        → Selects shipper
        → Applies coupon (optional)
        → Pays via Paymob
    → Receives order confirmation
    → Tracks order status from profile
    → Leaves a review after delivery
```

### 4.2 Admin: Adding a New Product

```
Logs into admin panel
    → Navigates to Products → Add New
    → Fills in: name, category, supplier, description, price, stock
    → Adds images (primary + secondary)
    → Adds variants (16GB/512GB, 32GB/1TB...)
    → Tags the product (New, Popular...)
    → Saves → Product appears in storefront immediately
```

### 4.3 Admin: Processing an Order

```
Order notification arrives (new Confirmed order)
    → Admin opens order detail
    → Prepares shipment
    → Updates status to Processing → Shipped (enters tracking note)
    → Customer sees updated status in their profile
    → Admin marks Delivered when shipper confirms
```

---

## 5. Feature Requirements

### 5.1 Feature Priority Matrix

| Feature | Priority | Phase | Notes |
|---|---|---|---|
| Customer auth (register / login) | P0 | 1 | Blocker for everything |
| Product catalogue (list + detail) | P0 | 2 | Core of the product |
| Basket (Redis) | P0 | 2 | Required for checkout |
| Checkout + payment (Paymob) | P0 | 2 | Revenue critical |
| Order tracking | P0 | 2 | Trust critical |
| Admin: product management | P0 | 2 | Owner can't operate without this |
| Admin: order management | P0 | 2 | Owner can't ship without this |
| Product variants (RAM/Storage) | P1 | 2 | Laptops always have variants |
| Product reviews | P1 | 3 | Converts browsers to buyers |
| Wishlist | P1 | 3 | Retention feature |
| Coupon / promo codes | P1 | 3 | Seasonal promotions |
| Admin dashboard + reports | P1 | 3 | Business visibility |
| Customer addresses (multiple) | P1 | 3 | UX improvement |
| Search | P1 | 2 | Core discovery |
| Social login (Google/Facebook) | P2 | Post v1 | Nice to have |
| Order history PDF export | P2 | Post v1 | — |
| SMS notifications | P2 | Post v1 | — |
| Loyalty points | P3 | Post v1 | — |

---

## 6. Functional Specifications

### 6.1 Auth Module

**Registration**
- Fields: Full Name, Email, Password, Phone
- Validation: Email unique, password ≥ 8 chars + 1 uppercase + 1 digit
- On success: returns JWT access token (15 min) + refresh token (7 days)
- Side effect: welcome email sent asynchronously

**Login**
- Fields: Email, Password
- Security: 5 failed attempts → 10-minute lockout
- Returns same token pair as registration

**Token Refresh**
- Accepts refresh token → returns new access + refresh token pair
- Old refresh token is immediately invalidated (rotation)

**Roles**
- `Customer` — default on registration
- `Admin` — seeded, not publicly registerable
- All admin endpoints return `403` when called with Customer token

---

### 6.2 Catalog Module

**Product Listing**
- Pagination: default 12, max 48 per page
- Filters: categoryId, minPrice, maxPrice, tagIds, inStock
- Sort: price_asc, price_desc, rating_desc, newest
- Search: partial match on ProductName and Description (case-insensitive)
- Cached in Redis: 3 minutes
- Each item shows: name, price, primary image, average rating, inStock flag

**Product Detail**
- Shows: all fields, all images, all variants with stock per variant, average rating, review count, tags
- Cached in Redis: 5 minutes
- Cache invalidated when admin updates the product

**Product Variants**
- Each variant has: RAM, Storage, PriceModifier (added to base price), QuantityInStock
- Variant stock is separate from base product stock
- Customer selects variant before adding to basket

**Soft Delete**
- Deleted products disappear from all customer-facing endpoints
- EF Core global query filter handles this transparently
- Admin sees deleted products with a `[Deleted]` badge

---

### 6.3 Basket Module

**Storage:** Redis Hash — `basket:{customerId}` — no SQL table

**Behaviour:**
- Adding same product again increments quantity
- Stock is NOT checked or reserved at basket stage
- Prices are fetched fresh from DB on every basket view (not stored in Redis)
- Basket TTL: 7 days, refreshed on every write
- Clearing basket: Redis key deleted

**Constraints:**
- Max 99 units per line item
- Anonymous baskets: not supported in v1

---

### 6.4 Orders Module

**Checkout Flow:**
1. Customer submits: addressId, shipperId, optional couponCode
2. System validates address belongs to customer
3. System validates coupon (if provided): active, not expired, usage limit not reached, order total ≥ minimum
4. System opens SQL transaction with UPDLOCK on stock rows
5. Stock check: for each item, verify QuantityInStock ≥ requested quantity
6. If any item fails: transaction rolled back, `422` returned with list of failed items
7. If all pass: stock decremented, Order + OrderItems inserted, coupon UsedCount incremented
8. Transaction committed
9. Basket cleared from Redis
10. Payment URL generated via Paymob and returned in response

**Payment Webhook:**
- Paymob POSTs to `/api/orders/payment-callback`
- HMAC signature verified before any processing
- On success: Order status → Confirmed, Payment record created
- On failure: Order status → Cancelled, stock quantities restored
- Idempotent by TransactionId (unique constraint on Payments table)

**Order Status Lifecycle:**

```
Pending → Confirmed → Processing → Shipped → Delivered
                                          ↘
Pending → Cancelled (admin action or failed payment)
Delivered → Refunded (manual process in v1)
```

No backward transitions allowed. Terminal states: Delivered, Cancelled, Refunded.

---

### 6.5 Reviews Module

**Eligibility:** Customer must have at least one completed order containing the product (`IsVerified = true`)

**Constraints:**
- One review per customer per product (unique constraint in DB)
- Rating: integer 1–5
- Title: optional, max 200 chars
- Body: optional, max 2000 chars

**Display:**
- Summary: average rating, total count, star distribution (1★ to 5★ counts)
- List: sorted by newest first, paginated (10 per page)
- Each review shows: rating, title, body, customer first name only (privacy), date, verified badge

---

### 6.6 Admin Module

**Dashboard metrics:**
- Revenue: all time, this month, today
- Order counts by status
- Top 5 products by revenue
- Cached 10 minutes (acceptable staleness for a dashboard)

**Coupon management:**
- Types: Percentage (e.g. 20% off) or Fixed (e.g. 50 EGP off)
- Controls: code, value, min order amount, max uses, expiry date, active toggle
- Deactivating a coupon takes effect immediately

---

## 7. Non-Functional Requirements

### 7.1 Performance Targets

| Endpoint | Target Response Time | Strategy |
|---|---|---|
| GET /products (listing) | < 1.5s | Redis cache 3 min + DB indexes |
| GET /products/{id} | < 500ms | Redis cache 5 min |
| POST /orders (checkout) | < 2s | Optimised SQL transaction, async payment call |
| GET /basket | < 300ms | Redis only, no DB hit for basket data |
| GET /admin/dashboard | < 1s | Redis cache 10 min |
| POST /auth/login | < 500ms | Single DB lookup by indexed Email |

### 7.2 Security Requirements

| Requirement | Implementation |
|---|---|
| No plaintext passwords | ASP.NET Core Identity (bcrypt hashing) |
| No SQL injection | EF Core parameterised queries throughout |
| JWT tamper protection | RS256 or HS256 signed tokens, validated on every request |
| Payment webhook integrity | HMAC signature verification before processing |
| No sensitive data in logs | Serilog destructuring policy — mask email, never log passwords or tokens |
| HTTPS only | Enforced via ASP.NET Core HTTPS redirection middleware |
| Rate limiting | Login endpoint: 10 requests/minute per IP (via ASP.NET Core built-in RateLimiter (.NET 10)) |

### 7.3 Reliability

- API is stateless — any horizontal scaling works (JWT, Redis for basket/cache)
- No in-memory state between requests
- Checkout is atomic — partial orders are impossible (SQL transaction)
- Payment webhook is idempotent — re-delivery has no effect

---

## 8. API Design Principles

1. **RESTful resource naming** — nouns, not verbs: `/products`, `/orders`, not `/getProducts`
2. **Standard HTTP verbs** — GET (read), POST (create), PUT (update), DELETE (remove)
3. **ProblemDetails for all errors** — RFC 7807, always includes `traceId`
4. **Pagination on all lists** — no endpoint returns an unbounded collection
5. **Consistent response envelopes** — lists always wrapped in `{ items, page, pageSize, totalCount, totalPages }`
6. **Versioning** — not needed in v1, but URL prefix `/api/` reserved for future `/api/v2/`
7. **No breaking changes without a new version**

---

## 9. Data & Privacy

| Data | Retention | Notes |
|---|---|---|
| Customer PII (name, email, phone) | Indefinite while account exists | Deletable on customer request (GDPR-style) |
| Order history | Indefinite | Required for business records |
| Basket data | 7 days TTL in Redis | Auto-expires |
| Payment TransactionIds | Indefinite | Required for dispute resolution |
| Logs | 30 days | Structured logs, PII should be masked |

---

## 10. Error Handling Contract

Every error response must follow this shape. No raw exception messages ever reach the client.

```json
{
  "type": "https://lapbox.com/errors/{error-type}",
  "title": "Human-readable error message.",
  "status": 422,
  "traceId": "0HN2V...",
  "errors": {
    "fieldName": ["Specific validation message."]
  }
}
```

| HTTP Status | When to Use |
|---|---|
| 400 Bad Request | Malformed request (invalid JSON, missing required header) |
| 401 Unauthorized | No token or invalid/expired token |
| 403 Forbidden | Valid token but insufficient role |
| 404 Not Found | Resource does not exist or caller is not allowed to know it exists |
| 409 Conflict | Duplicate (email, coupon code) or constraint violation (deleting product with active orders) |
| 422 Unprocessable | Business rule violation (out of stock, invalid transition, bad coupon) |
| 423 Locked | Account locked after too many failed login attempts |
| 500 Internal Server Error | Unexpected server error — no detail exposed to client |

---

## 11. Observability

- **Logging:** Serilog with structured properties. Request/response logged at Info. Errors at Error with full stack trace.
- **Correlation:** Every request gets a `traceId` (from ASP.NET Core `Activity`). Included in all ProblemDetails responses.
- **No sensitive data in logs:** Email addresses masked to `a***@mail.com`. Passwords and tokens never logged.
- **Health check endpoint:** `GET /health` returns 200 if DB and Redis are reachable — used by deployment pipeline.

---

## 12. Acceptance Criteria for v1.0 Release

All of the following must be true before go-live:

### Functional
- [ ] Customer registers, logs in, browses products, adds to basket, checks out, pays, tracks order — full flow works end-to-end
- [ ] Out-of-stock item at checkout returns clear error, order is not created
- [ ] Two simultaneous checkouts for the last item — only one succeeds
- [ ] Admin creates a product — appears in storefront within 5 minutes (cache TTL)
- [ ] Admin updates stock — new stock reflects within 5 minutes
- [ ] Coupon applies correct discount; expired coupon is rejected
- [ ] Payment webhook updates order status correctly
- [ ] Review submission rejected without verified purchase
- [ ] Token refresh works; expired refresh token is rejected

### Technical
- [ ] All error responses are ProblemDetails (no raw exceptions)
- [ ] All list endpoints are paginated
- [ ] Soft-deleted products never appear in customer API
- [ ] Payment webhook is idempotent (tested by sending same payload twice)
- [ ] No N+1 queries (verified via EF Core logging in dev)
- [ ] All DB migrations run cleanly from a blank database

### Quality
- [ ] Unit test coverage ≥ 70% on Application layer (all handlers)
- [ ] API documented in Scalar and Postman collection exported to repo
- [ ] No compiler warnings
- [ ] `GET /health` returns 200 in staging

---

## 13. Out of Scope (v1.0)

The following are explicitly excluded from this version to keep scope manageable:

- Multi-vendor marketplace
- Mobile native apps (iOS / Android)
- Social login (Google, Apple, Facebook)
- Loyalty / points system
- Live chat or chatbot
- Product comparison feature
- SMS notifications
- Abandoned basket emails
- Returns and refund automation (manual process in v1)
- Multi-currency support
- Multi-language (Arabic / English toggle)
