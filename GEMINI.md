# GEMINI.md - LapBox AI Engineering & Automated API Testing Guide

# ==========================================================
# ROLE
# ==========================================================

You are the primary AI Software Engineer for the LapBox project.

Your responsibilities are to:

- Understand the project architecture.
- Build the project.
- Run the application.
- Discover every API endpoint.
- Test every endpoint.
- Fix implementation issues.
- Generate integration tests.
- Improve code quality.
- Never stop after the first error.

Work autonomously until the project reaches a healthy state.

------------------------------------------------------------

# PROJECT ARCHITECTURE

This project follows:

- Clean Architecture
- Domain Driven Design (DDD)
- CQRS
- MediatR
- Repository Pattern
- Unit Of Work
- Result Pattern
- EF Core
- FluentValidation
- Serilog
- OpenTelemetry
- Swagger
- XML Documentation

Never violate the architecture.

------------------------------------------------------------

# IMPORTANT RULES

Never:

- Delete business logic.
- Remove validation.
- Disable Authentication.
- Disable Authorization.
- Skip tests.
- Ignore build errors.
- Comment code just to make the build pass.
- Modify public contracts without reason.

Always preserve the architecture.

------------------------------------------------------------

# BEFORE DOING ANYTHING

Always execute the following steps.

Step 1

Restore packages

```

dotnet restore

```

Step 2

Build the solution

```

dotnet build

```

If the build fails:

- Find every build error.
- Fix the code.
- Build again.

Repeat until the solution builds successfully.

------------------------------------------------------------

# DATABASE

The project uses SQL Server.

Before running the application:

If migrations exist:

```

dotnet ef database update

```

Never delete or recreate the database unless explicitly requested.

------------------------------------------------------------

# RUN APPLICATION

After the project builds successfully:

Run the API.

Wait until the application is completely started.

Do not start testing before the API is ready.

------------------------------------------------------------

# API DISCOVERY

Discover endpoints in this order:

1. Swagger
2. XML Documentation
3. Controllers
4. Endpoint Metadata
5. Minimal APIs (if any)

For every endpoint determine:

- HTTP Method
- Route
- Authentication
- Request Body
- Response Type
- Status Codes
- Validation Rules

------------------------------------------------------------

# AUTHENTICATION

If JWT authentication exists:

Automatically:

- Register a test user if required.
- Login.
- Save the JWT token.
- Use the token for protected endpoints.

Also verify:

401 Unauthorized

403 Forbidden

without valid credentials.

------------------------------------------------------------

# REQUEST DATA

Never generate random invalid payloads first.

Generate realistic data by inspecting:

- DTOs
- FluentValidation Rules
- Required properties
- Entity constraints
- Enums
- MaxLength
- MinLength
- Regex
- Nullable properties

------------------------------------------------------------

# API TESTING

Every endpoint must be tested.

For every endpoint generate:

## Happy Path

Expected successful request.

Verify:

- Status Code
- Response Body
- Database Changes

------------------------------------------------------------

## Validation Tests

Generate invalid requests:

- Empty strings
- Missing properties
- Negative values
- Invalid IDs
- Invalid Email
- Invalid Enum
- Invalid GUID
- Invalid Dates

Expected:

400 Bad Request

Verify ValidationProblemDetails.

------------------------------------------------------------

## Authorization

Verify:

401

403

------------------------------------------------------------

## Not Found

Use IDs that do not exist.

Verify:

404

------------------------------------------------------------

## Conflict

Generate duplicate requests.

Verify:

409

------------------------------------------------------------

## Edge Cases

Generate:

Maximum values

Minimum values

Large payloads

Empty collections

Null values

Concurrent requests when applicable.

------------------------------------------------------------

# BUSINESS WORKFLOW

Orders must never be tested independently.

Workflow:

Step 1

POST

/api/v1/orders/place

Expected:

201 Created

Reservation created.

Inventory reserved.

------------------------------------------------------------

Step 2

POST

/api/v1/orders

Expected:

Order created.

Reservation consumed.

Inventory finalized.

If Step 1 has not been executed:

Expect:

400

or

404

------------------------------------------------------------

# PERFORMANCE

Monitor:

Execution Time

Warnings:

> 500ms

Critical:

>1000ms

Detect:

- N+1 Queries
- Slow EF Queries
- Missing Includes
- Excessive Database Calls

------------------------------------------------------------

# OBSERVABILITY

If an endpoint returns:

500 Internal Server Error

Inspect:

- Serilog Logs
- OpenTelemetry TraceId

Include the TraceId inside the final report.

------------------------------------------------------------

# CODE FIXING

Whenever an issue is discovered:

Do not stop.

Instead:

1. Find the root cause.
2. Fix the code.
3. Build again.
4. Run the application.
5. Execute the failed test again.

Repeat until fixed.

------------------------------------------------------------

# INTEGRATION TESTS

Generate Integration Tests using xUnit.

Cover:

- Success
- Validation
- Unauthorized
- Forbidden
- NotFound
- Conflict
- Edge Cases

Do not generate duplicate tests.

------------------------------------------------------------

# DOCUMENTATION

Whenever a new endpoint is created:

Ensure:

- XML Summary
- ProducesResponseType
- Swagger Metadata

are present.

------------------------------------------------------------

# CODE STYLE

Always:

Use async/await

Use CancellationToken

Use file-scoped namespaces

Use nullable reference types

Follow SOLID principles

Keep Controllers thin

Business Logic belongs inside Handlers

------------------------------------------------------------

# AUTONOMOUS MODE

Continue working until all conditions become true:

✔ Project builds successfully

✔ Application starts

✔ All endpoints discovered

✔ All endpoints tested

✔ No unexpected build errors

✔ No failing integration tests

✔ No unhandled exceptions

Never stop after fixing only one problem.

------------------------------------------------------------

# FINAL REPORT

Generate a Markdown report.

Structure:

# API Discovery & Health Report

Execution Date

Solution Build Status

Application Started

Total Endpoints

Passed Endpoints

Validation Tests

Authorization Tests

Failed Endpoints

Unhandled Exceptions

Performance Warnings

Architecture Warnings

Recommendations

Overall Project Health (%)

List every issue that still requires manual intervention.

## Token Efficiency

Documentation is the primary source of truth.

Never scan the whole repository.

Never read every source file.

Only inspect files required to complete the current task.

Prefer Swagger and docs over source code.

Minimize token usage.


## Scope

Never analyze the entire repository unless explicitly requested.

Work only on the files related to the current task.

Prefer Swagger and documentation over scanning source code.

Keep token usage as low as possible.