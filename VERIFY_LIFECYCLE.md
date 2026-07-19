# VERIFY_LIFECYCLE.md - Customer & Order Lifecycle Verification Guide

## 🎯 Mission

You are an AI Software Architect, Backend Engineer, and Quality Assurance Agent.

Your mission is to verify that the entire Customer Lifecycle implementation matches the intended business requirements while preserving:

* Clean Architecture
* Domain-Driven Design (DDD)
* CQRS
* SOLID
* Result Pattern
* Repository Pattern
* Unit of Work

Do not assume implementation details.

Every conclusion must be supported by evidence from the source code.

---

# Verification Rules

## Evidence Required

For every conclusion provide:

* File name
* Class
* Method
* Explanation

Never simply state that something exists.

Example:

```
File:
src/Application/Orders/Commands/PlaceOrder/PlaceOrderCommandHandler.cs

Method:
Handle()

Reason:
Creates a StockReservation with a 15-minute expiration.
```

---

## No Assumptions

Never assume.

Never guess.

Search by responsibility rather than class names.

For example:

Instead of assuming there is a class called:

```
PlaceOrderCommandHandler
```

Locate the component responsible for:

* Checkout
* Stock Reservation
* Payment Finalization
* Customer Creation
* Role Promotion

---

## If Something Cannot Be Verified

Never invent answers.

Instead report:

```
Status:
Not Verified
```

and explain why.

---

# Lifecycle Verification

---

# Phase 1

## User Registration

### Expected Behavior

When a new user registers:

* A new ASP.NET Identity user is created.
* The assigned role must be:

```
User
```

NOT

```
Customer
```

### Verification

Verify:

* Registration endpoint
* Registration handler
* Identity service

Ensure:

* Identity user is created.
* Default role equals "User".
* No Customer entity is created.
* No Order entity is created.
* No Customer statistics are initialized.

If any Customer record is created during registration,
report it as a business rule violation.

---

# Phase 2

## Shopping Cart & Stock Reservation

### Expected Behavior

When checkout begins:

Inventory must be reserved for exactly

```
15 minutes
```

before payment is completed.

### Verification

Locate the checkout workflow.

Verify:

* Pessimistic locking is used.

Examples:

```
GetByIdsWithLockAsync(...)
```

or equivalent locking implementation.

Verify:

* StockReservation entity is created.

Verify:

Expiration:

```
DateTimeOffset.UtcNow.AddMinutes(15)
```

Verify:

* Reserved quantity equals requested quantity.
* Reservation belongs to the correct customer.
* Shopping cart is cleared only after reservation succeeds.

If reservation fails:

Verify:

* Cart is NOT cleared.
* Transaction rolls back.
* Inventory remains unchanged.

---

# Phase 3

## Payment Finalization

### Expected Behavior

After successful payment:

The following actions must happen in order.

1.

Shipping information collected.

2.

Shipping provider invoked.

3.

Final Order created.

4.

Reservation consumed.

5.

Inventory permanently updated.

6.

Customer entity created.

7.

Identity role changed:

```
User

↓

Customer
```

### Transaction Verification

Verify all actions occur inside a single transaction.

Any failure must roll back:

* Order
* Customer
* Reservation
* Inventory

No partial data may remain.

---

## Identity Promotion

Verify:

Role promotion happens ONLY AFTER:

* Order successfully saved.
* Transaction committed.

Never before.

---

## Idempotency

Verify payment callbacks.

If payment webhook executes twice:

There must NOT be:

* Duplicate Orders
* Duplicate Customers
* Duplicate Reservations
* Duplicate Inventory updates
* Duplicate Role promotions

Report any duplicate creation risk.

---

# Phase 4

## Customer Dashboard

Only users with role

```
Customer
```

may access:

* Customer Profile
* Dashboard
* Orders
* Purchase History

Verify:

Authorization is enforced server-side.

Examples:

```
[Authorize(Roles = "Customer")]
```

or equivalent policy.

Verify profile data comes from:

Customer aggregate

NOT

Identity tables only.

---

# Repository Verification

Locate implementations responsible for:

* Customer Repository
* Order Repository
* Stock Reservation Repository
* Identity Service

Verify:

Repositories are used correctly.

Business logic must NOT exist inside repositories.

---

# CQRS Verification

Verify:

Controllers only dispatch commands and queries.

Business logic belongs inside handlers.

Handlers must not access HttpContext directly.

---

# Result Pattern Verification

Verify:

Business failures return:

```
Result<T>
```

Controllers must translate:

Validation

↓

400

NotFound

↓

404

Conflict

↓

409

Unexpected Exceptions

↓

500

Business logic must not throw exceptions for expected failures.

---

# Validation Verification

Verify:

FluentValidation validates:

* Empty strings
* Invalid email
* Invalid IDs
* Negative values
* Missing required fields
* Invalid enums

Verify failed validation returns:

```
ValidationProblemDetails
```

---

# Transaction Verification

Whenever inventory changes:

Verify:

* Database transaction exists.
* Locks are acquired.
* Rollback occurs on failure.

No inventory modification should occur outside a transaction.

---

# Integration Test Verification

Locate Integration Tests.

Verify the complete lifecycle exists.

Required flow:

Register

↓

Login

↓

Cart

↓

Reservation

↓

Payment

↓

Order

↓

Customer

↓

Dashboard

If missing:

Generate xUnit Integration Tests covering:

* Happy Path
* Validation failures
* Unauthorized
* Duplicate payment
* Reservation expiration
* Payment failure
* Rollback scenarios

---

# Architectural Verification

Verify compliance with:

* Clean Architecture
* DDD
* SOLID
* CQRS
* Repository Pattern
* Unit of Work
* Dependency Injection

Business logic must never appear inside:

* Controllers
* Repositories
* DbContext

---

# Final Report

Generate a Markdown report.

Structure:

# Customer Lifecycle Verification Report

Execution Date

Overall Status

Verified Phases

Not Verified Items

Business Rule Violations

Architecture Violations

CQRS Violations

Transaction Issues

Authorization Issues

Validation Issues

Idempotency Issues

Integration Test Coverage

Recommendations

Overall Health Score

---

# Autonomous Mode

Do not stop after finding the first issue.

Continue verifying the entire lifecycle.

If an implementation is incorrect:

1. Explain the root cause.
2. Explain the architectural impact.
3. Recommend the best fix.

Only modify the implementation when explicitly instructed by the user.

Never change business rules to make the verification pass.
