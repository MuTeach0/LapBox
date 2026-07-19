# LapBox — Requirements (Updated v1.1)

> This document is the single source of truth for what the system must do.
> Every feature here maps to tasks in `06_Task_Breakdown.md` and endpoints in `05_API_Contracts.md`.

---

## 1. User Stories & Acceptance Criteria

### Module: Auth

**US-AUTH-01 — Customer Registration**
> As a new visitor, I want to create an account so I can place orders and track them.

- Customer provides full name, email, and password.
- Email must be unique — duplicate returns `409 Conflict`.
- Password min 8 chars, 1 uppercase, 1 digit.
- System returns JWT (Access + Refresh) on success.
- Welcome email sent asynchronously.

**US-AUTH-02 — Customer Login**
- JWT Access (15 min) / Refresh (7 days).
- 5 failed attempts = 10 min lockout (`423 Locked`).

---

### Module: Catalog

**US-CAT-04 — View Product Detail**
> As a customer, I want to see full specs, all images, and available variants before buying.

- **Acceptance Criteria:**
    - Returns: full product fields, all images, and **Structured Variants**.
    - Variants are returned as a list of attributes (e.g., RAM: 8GB, 16GB | Storage: 256GB, 512GB).
    - System calculates the price based on the selected variant combination.
    - Response is cached in Redis for 5 minutes.

**US-CAT-05 — Admin: Create/Update Product**
- Admin defines product attributes using a structured schema (Attributes & Values).
- **Audit:** Every creation or update is logged with the Admin's ID and timestamp.

---

### Module: Basket
- Items stored in **Redis** (7-day TTL).
- **Note:** Adding an item to the basket DOES NOT reserve stock.

---

### Module: Orders

**US-ORD-01 — Place Order (Checkout & Reservation)**
> As a customer, I want to reserve my items while I finish the payment process.

- **Acceptance Criteria:**
    - When the customer starts Checkout, the system creates a **Temporary Stock Reservation** for 15 minutes.
    - **Logic:** `Available_Stock = Total_Physical_Stock - Active_Reservations`.
    - If `Available_Stock >= Requested_Quantity`, the reservation is successful.
    - On success: Returns OrderId and Paymob Payment URL.
    - On failure: Returns `422 Unprocessable Entity` with a message that the item is currently reserved or out of stock.

**US-ORD-02 — Payment Callback**
- **Confirmed Payment:** Reservation converts to a permanent physical stock deduction. Order status -> `Confirmed`.
- **Failed/Expired Payment:** Reservation is automatically released, making stock available to others. Order status -> `Cancelled`.
- System must handle Paymob webhooks idempotently using Transaction IDs.

**US-ORD-05 — Admin: Update Order Status**
- Valid transitions: Pending → Confirmed → Processing → Shipped → Delivered.
- **Audit:** Every status change is recorded in an `OrderStatusHistory` table with `AdminId`.

---

### Module: Admin & Inventory Management

**US-ADM-03 — Inventory Audit Logs (New)**
> As an admin, I want to track every single movement in my warehouse.

- **Acceptance Criteria:**
    - System logs every stock change (Stock-In, Sale, Return, Adjustment, Reservation).
    - Log entry includes: `ProductId`, `QuantityChange`, `TransactionType`, `AdminId/OrderId`, and `Timestamp`.

---

## 2. System-Level Requirements

### Stock Management Rule (The Reservation Window)
To ensure fairness and prevent over-selling, LapBox implements a **15-minute reservation window**:

1. **Reservation Trigger:** Created when the customer hits "Proceed to Payment".
2. **Background Worker:** A scheduled task runs every 1 minute to identify and delete expired/unpaid reservations.
3. **Concurrency:** Uses SQL Server `UPDLOCK` or `ROWLOCK` during the reservation process to prevent two customers from reserving the same unit at the exact same microsecond.



### Error Response Format
All errors strictly follow **RFC 7807 ProblemDetails**.

```json
{
  "type": "[https://lapbox.com/errors/inventory-reserved](https://lapbox.com/errors/inventory-reserved)",
  "title": "Item Temporarily Unavailable",
  "status": 422,
  "detail": "This item is currently held in another customer's checkout session. Please try again in 15 minutes."
}