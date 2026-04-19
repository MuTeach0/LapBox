# LapBox — API Contracts

> Base URL: `https://api.lapbox.com/api`
> Auth: `Authorization: Bearer <access_token>` (unless marked Public)
> All responses: `Content-Type: application/json`
> All errors: RFC 7807 ProblemDetails format

---

## Conventions

### Success Responses
| Scenario | Status |
|---|---|
| Resource returned | `200 OK` |
| Resource created | `201 Created` + `Location` header |
| No content to return | `204 No Content` |

### Error Responses (always ProblemDetails)
```json
{
  "type": "https://lapbox.com/errors/not-found",
  "title": "Product not found.",
  "status": 404,
  "traceId": "0HN2V...",
  "errors": {}
}
```

For validation errors (`422`):
```json
{
  "type": "https://lapbox.com/errors/validation",
  "title": "One or more validation errors occurred.",
  "status": 422,
  "traceId": "0HN2V...",
  "errors": {
    "price": ["Price must be greater than 0."],
    "categoryId": ["Category does not exist."]
  }
}
```

### Pagination Envelope (all list endpoints)
```json
{
  "items": [...],
  "page": 1,
  "pageSize": 12,
  "totalCount": 84,
  "totalPages": 7
}
```

---

## Module: Auth

---

### POST /auth/register
**Access:** Public

**Request:**
```json
{
  "fullName": "Alice Johnson",
  "email": "alice@mail.com",
  "password": "Secret123",
  "phone": "01012345678"
}
```

**Response 201:**
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "dGhpcyBp...",
  "expiresAt": "2026-04-17T12:15:00Z",
  "customer": {
    "customerId": 1,
    "fullName": "Alice Johnson",
    "email": "alice@mail.com",
    "role": "Customer"
  }
}
```

**Errors:** `409` email exists · `422` validation

---

### POST /auth/login
**Access:** Public

**Request:**
```json
{
  "email": "alice@mail.com",
  "password": "Secret123"
}
```

**Response 200:** Same shape as register response

**Errors:** `401` invalid credentials · `423` account locked (includes `unlockAt` field)

---

### POST /auth/refresh
**Access:** Public

**Request:**
```json
{
  "refreshToken": "dGhpcyBp..."
}
```

**Response 200:**
```json
{
  "accessToken": "eyJhbGci...",
  "refreshToken": "bmV3dG9r...",
  "expiresAt": "2026-04-17T12:30:00Z"
}
```

**Errors:** `401` invalid or expired refresh token

---

### POST /auth/logout
**Access:** Customer / Admin

**Request:** Empty body (token from header is used)

**Response 204**

---

## Module: Catalog

---

### GET /products
**Access:** Public

**Query Parameters:**
```
page         int     default: 1
pageSize     int     default: 12, max: 48
categoryId   int     optional
minPrice     decimal optional
maxPrice     decimal optional
tagIds       string  optional, comma-separated (e.g. "1,3,5")
inStock      bool    optional
sortBy       string  optional: price_asc | price_desc | rating_desc | newest
search       string  optional, partial match on name/description
```

**Response 200:**
```json
{
  "items": [
    {
      "productId": 1,
      "productName": "Dell XPS 15",
      "price": 1500.00,
      "primaryImageUrl": "img/dell-xps.png",
      "averageRating": 4.5,
      "reviewCount": 23,
      "categoryName": "Electronics",
      "inStock": true,
      "tags": ["New", "Popular"]
    }
  ],
  "page": 1,
  "pageSize": 12,
  "totalCount": 45,
  "totalPages": 4
}
```

---

### GET /products/{id}
**Access:** Public

**Response 200:**
```json
{
  "productId": 1,
  "productName": "Dell XPS 15",
  "categoryId": 1,
  "categoryName": "Electronics",
  "supplierId": 1,
  "supplierName": "TechSource Inc",
  "description": "15-inch premium laptop...",
  "price": 1500.00,
  "quantityInStock": 10,
  "inStock": true,
  "averageRating": 4.5,
  "reviewCount": 23,
  "images": [
    { "url": "img/dell1.png", "isPrimary": true },
    { "url": "img/dell2.png", "isPrimary": false }
  ],
  "variants": [
    {
      "variantId": 1,
      "ram": "16GB",
      "storage": "512GB SSD",
      "priceModifier": 0.00,
      "quantityInStock": 5
    },
    {
      "variantId": 2,
      "ram": "32GB",
      "storage": "1TB SSD",
      "priceModifier": 300.00,
      "quantityInStock": 3
    }
  ],
  "tags": ["New", "Popular"]
}
```

**Errors:** `404` not found

---

### POST /admin/products
**Access:** Admin

**Request:**
```json
{
  "productName": "Dell XPS 15",
  "categoryId": 1,
  "supplierId": 1,
  "description": "Premium laptop...",
  "price": 1500.00,
  "quantityInStock": 10,
  "tagIds": [1, 2],
  "images": [
    { "url": "img/dell1.png", "isPrimary": true }
  ],
  "variants": [
    { "ram": "16GB", "storage": "512GB SSD", "priceModifier": 0, "quantityInStock": 5 }
  ]
}
```

**Response 201:**
```json
{ "productId": 1 }
```
`Location: /api/products/1`

**Errors:** `422` validation · `404` categoryId or supplierId not found

---

### PUT /admin/products/{id}
**Access:** Admin

**Request:** Any subset of product fields (partial update)
```json
{
  "price": 1400.00,
  "quantityInStock": 8
}
```

**Response 200:** Full product detail (same as GET /products/{id})

**Errors:** `404` · `422` validation

---

### DELETE /admin/products/{id}
**Access:** Admin

**Response 204**

**Errors:** `404` · `409` product has active orders

---

## Module: Basket

---

### GET /basket
**Access:** Customer

**Response 200:**
```json
{
  "customerId": 1,
  "items": [
    {
      "productId": 1,
      "productName": "Dell XPS 15",
      "primaryImageUrl": "img/dell1.png",
      "unitPrice": 1500.00,
      "quantity": 2,
      "subtotal": 3000.00
    }
  ],
  "totalItems": 2,
  "totalAmount": 3000.00
}
```

---

### POST /basket/items
**Access:** Customer

**Request:**
```json
{
  "productId": 1,
  "quantity": 2
}
```

**Response 200:** Updated basket (same as GET /basket)

**Errors:** `404` product not found · `422` quantity > 99

---

### PUT /basket/items/{productId}
**Access:** Customer

**Request:**
```json
{
  "quantity": 3
}
```
Setting `quantity: 0` removes the item.

**Response 200:** Updated basket

**Errors:** `404` item not in basket · `422` quantity > 99

---

### DELETE /basket/items/{productId}
**Access:** Customer

**Response 204**

---

### DELETE /basket
**Access:** Customer

**Response 204**

---

## Module: Orders

---

### POST /orders
**Access:** Customer

**Request:**
```json
{
  "addressId": 2,
  "shipperId": 1,
  "couponCode": "SAVE20"
}
```

**Response 201:**
```json
{
  "orderId": 15,
  "totalAmount": 1200.00,
  "discountAmount": 300.00,
  "paymentUrl": "https://accept.paymob.com/api/acceptance/iframes/12345?payment_token=..."
}
```

**Errors:**
- `422` out of stock — includes `outOfStockItems: [{ productId, productName, requested, available }]`
- `422` invalid coupon — includes `couponError: "Coupon has expired"`
- `404` addressId not found or does not belong to customer

---

### POST /orders/payment-callback
**Access:** Public (Paymob webhook — HMAC verified internally)

**Request:** Paymob webhook payload

**Response 200** (Paymob requires 200 even on business rejection)

---

### GET /orders
**Access:** Customer

**Query Parameters:**
```
page      int  default: 1
pageSize  int  default: 10
```

**Response 200:**
```json
{
  "items": [
    {
      "orderId": 15,
      "orderDate": "2026-04-17T10:00:00Z",
      "status": "Confirmed",
      "totalAmount": 1200.00,
      "itemCount": 2
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 5,
  "totalPages": 1
}
```

---

### GET /orders/{id}
**Access:** Customer (own orders only) · Admin (all orders)

**Response 200:**
```json
{
  "orderId": 15,
  "orderDate": "2026-04-17T10:00:00Z",
  "status": "Confirmed",
  "totalAmount": 1200.00,
  "discountAmount": 300.00,
  "shippingAddress": {
    "label": "Home",
    "street": "15 Tahrir St",
    "city": "Cairo",
    "country": "Egypt"
  },
  "shipper": { "shipperId": 1, "shipperName": "DHL" },
  "items": [
    {
      "productId": 1,
      "productName": "Dell XPS 15",
      "primaryImageUrl": "img/dell1.png",
      "unitPrice": 1500.00,
      "quantity": 1,
      "discount": 300.00,
      "subtotal": 1200.00
    }
  ],
  "payment": {
    "amount": 1200.00,
    "method": "CreditCard",
    "paymentDate": "2026-04-17T10:01:00Z",
    "status": "Completed"
  }
}
```

**Errors:** `404` order not found or not owned by caller

---

### PUT /admin/orders/{id}/status
**Access:** Admin

**Request:**
```json
{
  "newStatus": "Shipped"
}
```

**Response 200:** Updated order summary

**Errors:** `422` invalid transition · `404`

---

### POST /admin/orders/{id}/cancel
**Access:** Admin

**Response 200:** Updated order summary

**Errors:** `422` order in non-cancellable state (Shipped/Delivered/Cancelled) · `404`

---

## Module: Reviews & Wishlist

---

### GET /products/{id}/reviews
**Access:** Public

**Query Parameters:**
```
page      int  default: 1
pageSize  int  default: 10
```

**Response 200:**
```json
{
  "summary": {
    "averageRating": 4.5,
    "totalReviews": 23,
    "distribution": { "5": 12, "4": 7, "3": 2, "2": 1, "1": 1 }
  },
  "items": [
    {
      "reviewId": 5,
      "rating": 5,
      "title": "Best laptop I've owned",
      "body": "Bought it for work...",
      "customerFirstName": "Alice",
      "createdAt": "2026-03-10T09:00:00Z",
      "isVerified": true
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 23,
  "totalPages": 3
}
```

---

### POST /products/{id}/reviews
**Access:** Customer

**Request:**
```json
{
  "rating": 5,
  "title": "Best laptop I've owned",
  "body": "Bought it for work and it handles everything..."
}
```

**Response 201:**
```json
{
  "reviewId": 5,
  "rating": 5,
  "isVerified": true
}
```

**Errors:** `403` no verified purchase · `409` already reviewed · `422` validation

---

### GET /wishlist
**Access:** Customer

**Response 200:**
```json
{
  "items": [
    {
      "productId": 1,
      "productName": "Dell XPS 15",
      "price": 1500.00,
      "primaryImageUrl": "img/dell1.png",
      "inStock": true
    }
  ],
  "totalItems": 1
}
```

---

### POST /wishlist/{productId}
**Access:** Customer

**Response 200:**
```json
{ "totalItems": 2 }
```

Idempotent — adding same product again returns `200` without error.

**Errors:** `404` product not found

---

### DELETE /wishlist/{productId}
**Access:** Customer

**Response 204**

---

## Module: Admin

---

### GET /admin/dashboard
**Access:** Admin

**Response 200:**
```json
{
  "revenue": {
    "allTime": 125000.00,
    "thisMonth": 18500.00,
    "today": 3200.00
  },
  "orders": {
    "pending": 12,
    "confirmed": 5,
    "shipped": 8,
    "delivered": 145,
    "cancelled": 7
  },
  "topProducts": [
    {
      "productId": 1,
      "productName": "Dell XPS 15",
      "totalRevenue": 45000.00,
      "unitsSold": 30
    }
  ]
}
```

---

### GET /admin/orders
**Access:** Admin

**Query Parameters:**
```
page        int     default: 1
pageSize    int     default: 20
status      string  optional
customerId  int     optional
fromDate    date    optional
toDate      date    optional
```

**Response 200:** Pagination envelope with order summaries

---

### GET /admin/customers
**Access:** Admin

**Query Parameters:**
```
page      int     default: 1
pageSize  int     default: 20
search    string  optional (name or email)
```

**Response 200:** Pagination envelope with customer summaries

---

### POST /admin/coupons
**Access:** Admin

**Request:**
```json
{
  "code": "SAVE20",
  "discountType": "Percentage",
  "discountValue": 20.00,
  "minOrderAmount": 500.00,
  "maxUses": 100,
  "expiresAt": "2026-12-31T23:59:59Z"
}
```

**Response 201:**
```json
{ "couponId": 3, "code": "SAVE20" }
```

**Errors:** `409` code already exists · `422` validation

---

### PUT /admin/coupons/{id}
**Access:** Admin

**Request:** Any subset of coupon fields
```json
{ "isActive": false }
```

**Response 200:** Updated coupon

---

## Addresses (Customer Profile)

---

### GET /customers/me/addresses
**Access:** Customer

**Response 200:**
```json
[
  {
    "addressId": 2,
    "label": "Home",
    "street": "15 Tahrir St",
    "city": "Cairo",
    "country": "Egypt",
    "isDefault": true
  }
]
```

---

### POST /customers/me/addresses
**Access:** Customer

**Request:**
```json
{
  "label": "Work",
  "street": "20 Corniche St",
  "city": "Alexandria",
  "country": "Egypt",
  "isDefault": false
}
```

**Response 201:**
```json
{ "addressId": 3 }
```

---

### DELETE /customers/me/addresses/{id}
**Access:** Customer

**Response 204**

**Errors:** `409` address is used in an existing order · `404`
