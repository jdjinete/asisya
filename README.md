# ASISYA - Enterprise Catalog & Commerce Solution (.NET 8 + Clean Architecture)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16.0-336791.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED.svg)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/Tests-15%20Passed-brightgreen.svg)]()

Production-grade implementation of the **Finanzauto - ASISYA Developer I** technical assessment. Built following **Clean Architecture**, **CQRS (Command Query Responsibility Segregation)** with MediatR, and **Spec-Driven Development (SDD)**.

---

## 1. Architectural Blueprint & Technical Decisions

```
backend/
├── Asisya.sln
├── src/
│   ├── Asisya.Domain/           # Enterprise entities, zero third-party dependencies
│   │   └── Entities/            # Category, Product, Supplier, Customer, Employee, Shipper, Order, OrderDetail
│   ├── Asisya.Application/      # Application business logic (CQRS, MediatR, FluentValidation)
│   │   ├── Common/              # IApplicationDbContext, PaginatedList<T>
│   │   └── Features/            # Category & Product Commands and Queries
│   ├── Asisya.Infrastructure/   # External persistence (EF Core, Npgsql PostgreSQL, Migrations)
│   │   └── Persistence/         # AsisyaDbContext, Configurations, B-Tree Indexes
│   └── Asisya.WebApi/           # HTTP entry point, Controllers, JWT Auth, Swagger OpenAPI, RFC 7807 Middleware
└── tests/
    └── Asisya.Application.Tests/ # xUnit test suite (15 unit tests passing)
```

### Key Architectural Justifications

1. **Clean Architecture Separation:**
   - **Domain Independence:** `Asisya.Domain` contains pure domain entities with zero external library references.
   - **Application Inversion:** `Asisya.Application` orchestrates use cases via `IApplicationDbContext` and MediatR handlers without referencing EF Core drivers or ASP.NET Core controllers.
   - **Infrastructure Decoupling:** PostgreSQL persistence and Fluent API mappings reside strictly in `Asisya.Infrastructure`.
2. **PostgreSQL vs SQL Server:**
   - Zero commercial licensing overhead for cloud container deployments.
   - Minimal container footprint (~50MB RAM idle vs ~1.5GB for SQL Server).
   - High performance indexing (B-Tree, Trigram GIN) for rapid textual queries.
3. **Bulk Ingestion Strategy (Batching vs Message Queues):**
   - For standard HTTP operations (up to 50,000–100,000 items), an in-process transactional batch insert (chunks of 1,000 items) provides **deterministic synchronous validation** with immediate feedback to the HTTP client (success vs failure counts) in ~1 second.
   - Crucial performance pattern: Invoking `ChangeTracker.Clear()` after each chunk avoids EF Core's O(N²) change-tracking memory explosion.
   - Resilient retry policy: Wrapped inside `IApplicationDbContext.ExecuteInTransactionAsync()` leveraging Npgsql's execution strategy.
4. **Referential Integrity Protection:**
   - `Products.CategoryId` -> `Categories.CategoryId` enforces `ON DELETE RESTRICT` preventing catalog corruption.
   - `OrderDetails.ProductId` -> `Products.ProductId` enforces `ON DELETE RESTRICT` to preserve historical transaction auditability.

---

## 2. Quickstart with Docker Compose

### Prerequisites
- Docker (v20+)
- Docker Compose (v2+)

### Running the Entire Stack

In the repository root, start both PostgreSQL and the .NET 8 Web API:

```bash
docker compose up --build -d
```

- **Frontend Web Portal (React SPA):** [http://localhost:3001](http://localhost:3001)
- **Web API & Swagger UI:** [http://localhost:5000](http://localhost:5000)
- **OpenAPI JSON Spec:** [http://localhost:5000/swagger/v1/swagger.json](http://localhost:5000/swagger/v1/swagger.json)
- **PostgreSQL Database:** `localhost:5432` (`asisya_db` / `asisya_user` / `asisya_password`)

*Note: Database migrations run automatically on startup.*

---

## 3. Frontend Architecture (React 18 + Vite + TypeScript)

The frontend satisfies all specifications using standard React ecosystem patterns:
- **Routing & Guarding (`AppRoutingModule` simulation):** Implemented in `src/router/AppRoutes.tsx` using `react-router-dom` v6 with an `AuthGuard` component that intercepts unauthenticated route access and redirects to `/login`.
- **Reactive Forms (`Reactive Forms` simulation):** Implemented in `src/pages/ProductFormPage.tsx` using `react-hook-form`, enforcing real-time field validation (required fields, price > 0, stock >= 0) and inline error messages.
- **Security & Interceptors:** `src/api/apiClient.ts` configures an Axios request interceptor that automatically attaches the JWT Bearer token from `localStorage` to all HTTP requests, and a response interceptor that catches `401 Unauthorized` responses to clear sessions and redirect to `/login`.
- **High-Performance Catalog:** Server-side pagination, instant debounced search, category filtering (`SERVIDORES`, `CLOUD`), and product detail inspection with category photo rendering.

---

## 3. API Endpoints & Operational Verification

### 3.1 Authentication (JWT)
Obtain a signed JWT Bearer token:

```bash
curl -X POST http://localhost:5000/Auth/Login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@asisya.com","password":"Admin123!"}'
```

Response:
```json
{
  "token": "<JWT_TOKEN>",
  "tokenType": "Bearer",
  "expiresInSeconds": 3600,
  "email": "admin@asisya.com",
  "role": "Admin"
}
```

Export the token in your shell:
```bash
TOKEN=$(curl -s -X POST http://localhost:5000/Auth/Login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@asisya.com","password":"Admin123!"}' | grep -o '"token":"[^"]*' | cut -d'"' -f4)
```

---

### 3.2 Category Creation (`POST /Category`)
Creates or resolves categories, enforcing business rules and uppercase normalization for core categories (`SERVIDORES`, `CLOUD`):

```bash
curl -X POST http://localhost:5000/Category \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "categoryName": "SERVIDORES",
    "description": "High performance datacenter servers"
  }'
```

---

### 3.3 Mass Product Ingestion (`POST /Product`)

#### A. High-Speed Synthetic Generation (e.g. 5,000 to 100,000 items):
```bash
curl -X POST http://localhost:5000/Product \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "generateRandomCount": 5000,
    "batchSize": 1000
  }'
```

Response (~1 second execution time):
```json
{
  "totalProcessed": 5000,
  "successfulImports": 5000,
  "failedImports": 0,
  "elapsedMilliseconds": 1069,
  "errors": []
}
```

#### B. Explicit Product List Upload:
```bash
curl -X POST http://localhost:5000/Product \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "products": [
      {
        "productName": "Dell PowerEdge R750xs",
        "categoryId": 1,
        "unitPrice": 2499.99,
        "unitsInStock": 25,
        "quantityPerUnit": "1U Rackmount Chassis",
        "discontinued": false
      },
      {
        "productName": "AWS EC2 c6i.2xlarge Dedicated",
        "categoryId": 2,
        "unitPrice": 0.34,
        "unitsInStock": 100,
        "quantityPerUnit": "8 vCPU / 16GB RAM hourly",
        "discontinued": false
      }
    ],
    "batchSize": 500
  }'
```

---

### 3.4 Query Catalog with Filters & Pagination (`GET /Products`)

Retrieve paginated catalog items with optional search and category filters:

```bash
# Retrieve page 1 with 10 items
curl "http://localhost:5000/Products?pageIndex=1&pageSize=10"

# Filter by category and search term with price ordering
curl "http://localhost:5000/Products?searchTerm=PowerEdge&categoryId=1&minPrice=100&sortBy=price&sortOrder=desc"
```

Response envelope:
```json
{
  "items": [
    {
      "productId": 63,
      "productName": "Dell PowerEdge - Gen10 #000063",
      "categoryId": 1,
      "categoryName": "SERVIDORES",
      "unitPrice": 3364.35,
      "unitsInStock": 10,
      "discontinued": false,
      "quantityPerUnit": "Rack 1U chassis"
    }
  ],
  "pageIndex": 1,
  "pageSize": 10,
  "totalItems": 484,
  "totalPages": 49,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

### 3.5 Product Detail with Category Photo (`GET /Products/{id}`)

Inspect a specific product with its full inventory metrics, vendor, and embedded category picture:

```bash
curl "http://localhost:5000/Products/63"
```

Response:
```json
{
  "productId": 63,
  "productName": "Dell PowerEdge - Gen10 #000063",
  "quantityPerUnit": "Rack 1U chassis",
  "unitPrice": 3364.35,
  "unitsInStock": 10,
  "unitsOnOrder": 11,
  "reorderLevel": 10,
  "discontinued": false,
  "category": {
    "categoryId": 1,
    "categoryName": "SERVIDORES",
    "description": "High performance datacenter servers",
    "picture": null,
    "pictureBase64": null
  },
  "supplierId": null,
  "supplierName": null
}
```

---

### 3.6 Standard Error Handling (RFC 7807 ProblemDetails)
When requesting a non-existent item or sending invalid inputs, the API responds with RFC 7807 formatted ProblemDetails:

```bash
curl -i "http://localhost:5000/Products/99999"
```

```http
HTTP/1.1 404 Not Found
Content-Type: application/problem+json

{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Product Not Found",
  "status": 404,
  "detail": "Product with ID 99999 was not found in the ASISYA catalog."
}
```

---

## 4. Running Automated Tests

Run the full xUnit test suite (covering unit tests for bulk batching, change tracker eviction, search filters, and JWT authentication):

```bash
dotnet test backend/Asisya.sln
```

Test Results:
```text
Passed!  - Failed: 0, Passed: 15, Skipped: 0, Total: 15, Duration: 538 ms
```

---

## 5. Teardown

To stop and remove running containers and volumes:

```bash
docker compose down -v
```
