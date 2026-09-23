# ASISYA - Enterprise Catalog & Commerce Solution (.NET 8 + Clean Architecture + React)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18.2-61DAFB.svg)](https://react.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16.0-336791.svg)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED.svg)](https://www.docker.com/)
[![CI/CD](https://img.shields.io/badge/GitHub_Actions-CI%2FCD-green.svg)](https://github.com/features/actions)
[![Tests](https://img.shields.io/badge/Tests-15%20Passed-brightgreen.svg)]()

Production-grade implementation of the **Finanzauto - ASISYA Developer I** technical assessment. Built following **Clean Architecture**, **CQRS (Command Query Responsibility Segregation)** with MediatR, **Spec-Driven Development (SDD)**, and a **Feature-Based Modular React SPA**.

---

## 1. Architectural Blueprint & Technical Decisions

```
asisya/
├── .github/workflows/pipeline.yml    # Multi-stage CI/CD workflow (Build, Test, Lint, Docker)
├── backend/
│   ├── Asisya.sln
│   ├── Dockerfile                   # Multi-stage .NET 8 Alpine build & test
│   ├── src/
│   │   ├── Asisya.Domain/           # Core domain entities (Zero dependencies)
│   │   │   └── Entities/            # Category, Product, Supplier, Customer, Employee, Shipper, Order, OrderDetail
│   │   ├── Asisya.Application/      # CQRS use cases, MediatR handlers, FluentValidation
│   │   │   ├── Common/              # IApplicationDbContext, PaginatedList<T>
│   │   │   └── Features/            # Category & Product Commands and Queries
│   │   ├── Asisya.Infrastructure/   # EF Core DbContext, Npgsql PostgreSQL, Migrations, B-Tree Indexes
│   │   └── Asisya.WebApi/           # REST Controllers, JWT Authentication, Swagger OpenAPI, RFC 7807 Middleware
│   └── tests/
│       └── Asisya.Application.Tests/ # xUnit test suite (15 unit tests passing)
├── frontend/
│   ├── Dockerfile                   # Multi-stage Node.js build with Nginx Alpine runtime
│   ├── nginx.conf                   # Reverse proxy for seamless API communication & SPA routing
│   └── src/                         # Modular React 18 + Vite + TypeScript application
└── docker-compose.yml               # Orchestration for PostgreSQL, .NET Web API, and React Frontend
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

Clone the repository and spin up all three services:

```bash
git clone https://github.com/jdjinete/asisya.git
cd asisya
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

## 4. API Endpoints & Operational Verification

### 4.1 Authentication (JWT)
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

### 4.2 Category Creation (`POST /Category`)
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

### 4.3 Mass Product Ingestion (`POST /Product`)

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

### 4.4 Query Catalog with Filters & Pagination (`GET /Products`)

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

### 4.5 Product Detail with Category Photo (`GET /Products/{id}`)

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

### 4.6 Standard Error Handling (RFC 7807 ProblemDetails)
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

## 5. Running Automated Tests

Run the full xUnit test suite (covering unit tests for bulk batching, change tracker eviction, search filters, and JWT authentication):

```bash
dotnet test backend/Asisya.sln
```

Test Results:
```text
Passed!  - Failed: 0, Passed: 15, Skipped: 0, Total: 15, Duration: 538 ms
```

---

## 6. Continuous Integration & Pipeline (GitHub Actions)

The repository includes a production-ready CI/CD pipeline defined in `.github/workflows/pipeline.yml`:
1. **Backend CI:** Restores, builds, and executes all 15 xUnit unit tests on .NET 8.
2. **Frontend CI:** Installs dependencies, runs ESLint code quality checks, and compiles the production Vite web bundle.
3. **Docker Validation:** Validates that both multi-stage Dockerfiles (`backend/Dockerfile` and `frontend/Dockerfile`) compile without errors prior to merge.

---

## 7. Technical Assumptions & Architectural Decisions (Supuestos)

During the design and implementation, the following technical assumptions were made to resolve ambiguities and maximize enterprise quality:

1. **Angular Terminology in a React Environment:**
   - *Requirement Mention:* The specification prompt referred to Angular terms (`Reactive Forms` and `AppRoutingModule`).
   - *Decision:* Per user instructions to strictly use React JS, we adopted the industry-standard equivalents in React:
     - `AppRoutingModule` is implemented via `react-router-dom` in `src/router/AppRoutes.tsx` with an `AuthGuard` component implementing the `CanActivate` pattern.
     - `Reactive Forms` is implemented via `react-hook-form` in `src/pages/ProductFormPage.tsx`, enforcing schema validations, error messages, and reactive state management.
2. **Bulk Ingestion Architecture (Batching vs Messaging Broker):**
   - *Decision:* For catalog uploads up to 100,000 items requiring synchronous HTTP response feedback (total processed, successful imports, failed items), in-process transactional batch streaming in chunks of 1,000 records with `ChangeTracker.Clear()` was chosen. This avoids the operational complexity of deploying RabbitMQ/Kafka, workers, and eventual consistency polling for this evaluation scope while executing 5,000 items in ~1.0 second.
3. **Category Picture Binary Representation vs Public URL:**
   - *Decision:* PostgreSQL stores pictures as `bytea` (`byte[]` in C#) for relational schema compatibility. The API serializes this data into both raw binary format and a data URI Base64 string (`data:image/jpeg;base64,...`) within `GET /Products/{id}`, allowing immediate rendering in web `<img />` tags without additional file storage dependencies.
4. **Core Categories Normalization:**
   - *Decision:* Predefined enterprise categories `'SERVIDORES'` and `'CLOUD'` are automatically normalized to uppercase and seeded if absent during bulk ingestion, ensuring foreign key referential integrity at all times.
5. **JWT Token Structure and Security Defaults:**
   - *Decision:* Signed using HMAC-SHA256 with standard claims (`sub`, `email`, `role`, `jti`, `organization`) and a 60-minute lifetime. Default evaluation accounts (`admin@asisya.com` / `Admin123!` and `operator@asisya.com` / `Operator123!`) are preconfigured for instant evaluation.

---

## 8. Teardown

To stop and remove running containers and volumes:

```bash
docker compose down -v
```
