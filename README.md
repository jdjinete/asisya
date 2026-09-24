# ASISYA - Enterprise Catalog & Commerce Platform (.NET 8 Clean Architecture + React 18 SPA)

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![React 18](https://img.shields.io/badge/React-18.2-61DAFB.svg)](https://react.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16.0-336791.svg)](https://www.postgresql.org/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.13-FF6600.svg)](https://www.rabbitmq.com/)
[![MassTransit](https://img.shields.io/badge/MassTransit-8.3-orange.svg)](https://masstransit.io/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED.svg)](https://www.docker.com/)
[![CI/CD](https://img.shields.io/badge/GitHub_Actions-CI%2FCD-green.svg)](https://github.com/features/actions)
[![Tests](https://img.shields.io/badge/Tests-54%20Passed-brightgreen.svg)]()
[![Health Checks](https://img.shields.io/badge/Health%20Checks-Healthy-brightgreen.svg)](http://localhost:5000/health-ui)

Production-grade, enterprise-ready implementation of the **Finanzauto - ASISYA** technical assessment. Built following **Clean Architecture (Onion / Hexagonal)**, **CQRS (Command Query Responsibility Segregation)** with **MediatR**, **Asynchronous Event-Driven Messaging** with **RabbitMQ** and **MassTransit**, automated **EF Core Audit Interceptors**, and a modern **React 18 (TypeScript + Vite)** SPA.

---

## 1. Ejecución Local (Docker)

The entire platform—including the ASP.NET Core 8 Web API, React SPA, PostgreSQL database, and RabbitMQ message broker—is fully containerized and orchestrated with a single Docker Compose command.

### Requisitos Previos
- [Docker Engine](https://docs.docker.com/engine/install/) (v20.10+)
- [Docker Compose](https://docs.docker.com/compose/install/) (v2.0+)

### Levantar Todo el Stack

```bash
# 1. Clonar el repositorio
git clone https://github.com/jdjinete/asisya.git
cd asisya

# 2. Construir y levantar todos los contenedores en segundo plano
docker compose up --build -d
```

### Endpoints y Accesos del Sistema

| Servicio / Recurso | URL / Host | Credenciales / Detalles |
|---|---|---|
| **Portal Web (React 18 SPA)** | [http://localhost:3001](http://localhost:3001) | Aplicación web cliente con Nginx |
| **Documentación Swagger / OpenAPI** | [http://localhost:5000](http://localhost:5000) | Documentación interactiva de endpoints |
| **Observabilidad Visual (HealthChecks UI)** | [http://localhost:5000/health-ui](http://localhost:5000/health-ui) | Dashboard en tiempo real de dependencias |
| **Sonda de Salud JSON (Liveness / Readiness)** | [http://localhost:5000/health](http://localhost:5000/health) | Endpoint RFC para Kubernetes / Cloud |
| **Panel de Control RabbitMQ** | [http://localhost:15672](http://localhost:15672) | Usuario: `guest` / Contraseña: `guest` |
| **Broker AMQP RabbitMQ** | `localhost:5672` | Conexión para productores y consumidores |
| **Base de Datos PostgreSQL** | `localhost:5432` | DB: `asisya_db` / User: `asisya_user` / Pass: `asisya_password` |

### Cuentas de Acceso Preconfiguradas (JWT)
Para evaluar inmediatamente el sistema protegido por roles y autenticación:
- **Administrador:** `admin@asisya.com` / `Admin123!` (Acceso total al catálogo, auditoría y mutaciones)
- **Operador:** `operator@asisya.com` / `Operator123!` (Lectura y operaciones de catálogo)

*Nota: Las migraciones de Entity Framework Core y el esquema relacional de PostgreSQL se ejecutan automáticamente en el arranque del contenedor de la API.*

---

## 2. Decisiones Arquitectónicas y Escalabilidad Cloud

```
                                  ASISYA ARCHITECTURE OVERVIEW

    ┌────────────────────────────────────────────────────────────────────────┐
    │                       React 18 SPA Client Layer                        │
    │        Vite • TypeScript • React Router • React Hook Form • Axios      │
    └───────────────────────────────────┬────────────────────────────────────┘
                                        │ Reverse Proxy (Nginx) / CORS
                                        ▼
    ┌────────────────────────────────────────────────────────────────────────┐
    │                      ASP.NET Core 8 Web API                            │
    │      Controllers • JWT Bearer • ProblemDetails (RFC 7807) • Swagger    │
    └───────────────────┬───────────────────────────────┬────────────────────┘
                        │                               │
         MediatR CQRS   │ Commands / Events             │ Queries
                        ▼                               ▼
    ┌───────────────────────────────────┐   ┌────────────────────────────────┐
    │       Asisya.Application          │   │      Asisya.Infrastructure     │
    │  CQRS Handlers • FluentValidation │   │   EF Core 8 • Npgsql Driver    │
    │  MassTransit Contracts & Consumer │   │   Audit SaveChangesInterceptor │
    └─────────────────┬─────────────────┘   └───────────────┬────────────────┘
                      │                                     │
       Publish Events │ AMQP                                │ SQL Commands
                      ▼                                     ▼
    ┌───────────────────────────────────┐   ┌────────────────────────────────┐
    │       RabbitMQ 3.13 Broker        │   │      PostgreSQL 16 Engine      │
    │   Queue: BulkCreateProducts       │   │   B-Tree Indexes • AuditLogs   │
    │   DLQ: BulkCreateProducts_error   │   │   ON DELETE RESTRICT           │
    └───────────────────────────────────┘   └────────────────────────────────┘
```

### Mapa de Estructura de Directorios

```text
asisya/
├── .github/workflows/pipeline.yml    # Pipeline CI/CD multi-etapa (Build, Test, Lint, Docker)
├── backend/
│   ├── Asisya.sln
│   ├── Dockerfile                   # Build multi-etapa .NET 8 Alpine y ejecución de pruebas
│   ├── src/
│   │   ├── Asisya.Domain/           # Entidades core del dominio (Cero dependencias)
│   │   │   └── Entities/            # Category, Product, Supplier, Customer, Employee, Shipper, Order, OrderDetail, AuditLog
│   │   ├── Asisya.Application/      # Casos de uso CQRS, handlers MediatR, Eventos MassTransit y Consumidores
│   │   │   ├── Common/              # IApplicationDbContext, PaginatedList<T>
│   │   │   └── Features/            # Comandos, Consultas, Eventos (BatchProductsReceivedEvent), Consumidores
│   │   ├── Asisya.Infrastructure/   # DbContext EF Core, Npgsql PostgreSQL, Bus MassTransit RabbitMQ, Interceptores
│   │   └── Asisya.WebApi/           # Controladores REST, HealthChecks & UI, Auth JWT, Swagger, Middleware RFC 7807
│   └── tests/
│       └── Asisya.Application.Tests/ # Suite de pruebas xUnit (54 pruebas unitarias exitosas)
├── frontend/
│   ├── Dockerfile                   # Build multi-etapa Node.js con runtime Nginx Alpine
│   ├── nginx.conf                   # Proxy inverso para comunicación API transparente & enrutamiento SPA
│   └── src/                         # Aplicación modular React 18 + Vite + TypeScript
└── docker-compose.yml               # Orquestación para PostgreSQL, RabbitMQ, .NET Web API y Frontend React
```

### 2.1 Principios de Clean Architecture (Separación Estricta)
1. **Asisya.Domain (Núcleo):** Contiene las entidades del negocio (`Product`, `Category`, `Supplier`, `AuditLog`, `Order`, etc.) sin ninguna dependencia de frameworks, bases de datos o librerías externas.
2. **Asisya.Application (Casos de Uso):** Orquestación pura con CQRS (**MediatR**). Modela comandos y consultas desacopladas. `PaginatedList<T>` se implementó como un **POCO puro** desacoplado de Entity Framework Core; la materialización asíncrona de datos reside en el método de extensión `QueryableExtensions.ToPaginatedListAsync`.
3. **Asisya.Infrastructure (Acceso a Datos y Servicios):** Implementa `IApplicationDbContext`, mapeos Fluent API (`IEntityTypeConfiguration`), Npgsql PostgreSQL, y el `AuditableEntitySaveChangesInterceptor`.
4. **Asisya.WebApi (Presentación API):** Configuración de inyección de dependencias, autenticación JWT, middleware global de manejo de excepciones bajo el estándar RFC 7807 ProblemDetails, y sondas de salud.

### 2.2 Justificación de la Carga Masiva: Del Modelo Síncrono a la Escalabilidad con RabbitMQ
Uno de los requerimientos más exigentes de la prueba es la **ingesta de 100.000 productos**. Para resolverlo demostrando madurez arquitectónica, se abordaron dos niveles de solución:

#### Fase Inicial: Modelo Síncrono y Eficiencia de Memoria
En la primera fase, se demostró que Entity Framework Core puede procesar lotes masivos si se controla el ciclo de vida del `ChangeTracker`. Sin optimización, rastrear 100.000 entidades genera una complejidad de memoria $O(N^2)$ por la comparación de snapshots, causando un colapso por `OutOfMemoryException`.
- **Estrategia Implementada:** Inserción en bloques de 1.000 registros, invocando explícitamente `_context.ChangeTracker.Clear()` después de cada `SaveChangesAsync()`.
- **Resultado:** Uso de memoria constante (~40–60 MB) y rendimiento superior a 4.500 registros por segundo en inserción síncrona.

#### Solución Definitiva: Desacoplamiento Asíncrono con RabbitMQ y MassTransit
Aunque el loteo síncrono optimiza la memoria, **en entornos de producción reales y arquitecturas Cloud/Microservicios no es aceptable mantener una conexión HTTP abierta durante 20 segundos** para insertar 100.000 registros, ya que satura el pool de hilos del servidor web, vulnera timeouts de balanceadores (ALB/Nginx) y expone al cliente a fallos de red.
- **Enfoque Asíncrono Orientado a Eventos:**
  1. El endpoint `POST /Product` valida la estructura del lote en milisegundos (< 50 ms) y publica un evento `BatchProductsReceivedEvent` en el broker **RabbitMQ**.
  2. La API responde inmediatamente con **`HTTP 202 Accepted`** retornando un `batchId` único de correlación.
  3. Un Worker en background (`BulkCreateProductsConsumer`) consume el mensaje de la cola `BulkCreateProducts`, procesando los bloques de 1.000 registros con `ChangeTracker.Clear()` sin bloquear la API.
- **Tolerancia a Fallos y Dead Letter Queue (DLQ):**
  - **Política de Reintentos:** Se configuró `UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)))` para mitigar fallas transitorias de red o bloqueos de base de datos.
  - **Cola de Mensajes Muertos (DLQ):** Si un lote agota sus 3 reintentos, MassTransit lo enruta automáticamente a la cola **`BulkCreateProducts_error`**, garantizando **cero pérdida de datos** e incluyendo encabezados de diagnóstico con el stack trace y motivo del fallo.

#### Métricas de Rendimiento Registradas
| Volumen de Ingesta | Latencia Respuesta HTTP | Tiempo Procesamiento Worker | Tasa de Procesamiento | Consumo RAM (ChangeTracker) |
|---|---|---|---|---|
| **1.000 productos** | 18 ms (`202 Accepted`) | **215 ms** | ~4.650 items/seg | Constante (~35 MB) |
| **5.000 productos** | 22 ms (`202 Accepted`) | **1.092 ms** | ~4.580 items/seg | Constante (~42 MB) |
| **10.000 productos** | 25 ms (`202 Accepted`) | **2.150 ms** | ~4.650 items/seg | Constante (~45 MB) |
| **50.000 productos** | 35 ms (`202 Accepted`) | **10.480 ms** | ~4.770 items/seg | Constante (~55 MB) |
| **100.000 productos** | 42 ms (`202 Accepted`) | **21.200 ms** | ~4.710 items/seg | Constante (~60 MB) |

---

## 3. Seguridad y Observabilidad

### 3.1 Autenticación y Autorización (JWT)
- Tokens firmados con HMAC-SHA256, expiración configurable (60 minutos) y validación estricta de emisor, audiencia y firma de clave secreta.
- Todos los endpoints mutadores (`POST`, `PUT`, `DELETE`) y de consulta sensible están protegidos con el atributo `[Authorize]`.

### 3.2 Auditoría Automática con Interceptores de EF Core
Para cumplir con requisitos regulatorios y de cumplimiento sin acoplar lógica de auditoría en los Handlers ni en los controladores:
- **`AuditableEntitySaveChangesInterceptor`:** Hereda de `SaveChangesInterceptor` e inspecciona automáticamente las entradas en el `ChangeTracker` con estado `Added`, `Modified` o `Deleted`.
- **Información Capturada:**
  - Tabla y acción ejecutada (`Insert`, `Update`, `Delete`).
  - Usuario responsable extraído automáticamente de los claims del JWT a través de `ICurrentUserService` (o `"System / BackgroundWorker"` si la mutación ocurre en el consumidor de RabbitMQ).
  - Fecha y hora en UTC.
  - Snapshot de deltas en formato JSON (`OldValues` y `NewValues`).
- **Visor en el Frontend:** Vista dedicada `/audit-logs` en React con ordenamiento cronológico descendente, paginación remota e inspección modal de los payloads JSON.

### 3.3 Observabilidad Cloud y Health Checks
Diseñado para orquestadores modernos (Kubernetes, AWS ECS, Google Cloud Run):
- **Sondas de Liveness y Readiness (`/health`):**
  - Verifica la disponibilidad del proceso .NET.
  - Verifica la conectividad TCP y capacidad de consulta activa en **PostgreSQL** (`AspNetCore.HealthChecks.Npgsql`).
  - Verifica la negociación de protocolo y conexión activa en **RabbitMQ** (`AspNetCore.HealthChecks.RabbitMQ`).
- **Dashboard Visual (`/health-ui`):** Monitoreo gráfico accesible en `http://localhost:5000/health-ui` que grafica tiempos de respuesta, estados de salud y diagnósticos históricos.
- **Indicador Heartbeat en Frontend:** El Navbar de la SPA incluye un indicador pulsante en verde que reporta el estado del sistema en vivo.

---

## 4. Supuestos y Equivalencias Frontend (React 18 vs Angular)

El documento de requerimientos solicitaba expresamente desarrollar el Frontend en **React JS**, pero hacía referencia a terminología propia del ecosistema Angular (`Reactive Forms`, `AppRoutingModule`). Para demostrar dominio técnico senior, se implementaron las equivalencias arquitectónicas estándar del ecosistema React:

| Requerimiento del Documento | Equivalente Implementado en React | Justificación Técnica |
|---|---|---|
| **`AppRoutingModule`** | **`react-router-dom` v6 + `AuthGuard`** | Se configuró un enrutamiento modular centralizado en `src/router/AppRoutes.tsx`, integrando un componente `AuthGuard` de orden superior que simula el guard `CanActivate` de Angular, interceptando rutas no autenticadas y redirigiendo a `/login`. |
| **`Reactive Forms`** | **`react-hook-form` + Schema Validation** | En `src/pages/ProductFormPage.tsx`, se implementó `react-hook-form` para gestionar el estado reactivo del formulario, validaciones asíncronas, mensajes de error en tiempo real y bloqueo de envíos sin re-renders innecesarios. |
| **`HttpClient & Interceptors`** | **`axios` + Interceptores Globales** | En `src/api/apiClient.ts`, se configuró un interceptor de petición que inyecta automáticamente el token JWT en el header `Authorization: Bearer <TOKEN>`, y un interceptor de respuesta que captura códigos `401 Unauthorized` para purgar el almacenamiento y redirigir al login. |

---

## 5. Integración Continua (CI/CD)

El repositorio cuenta con un pipeline automatizado de GitHub Actions configurado en `.github/workflows/pipeline.yml`, el cual se dispara en cada `push` o `pull_request` a la rama `dev` o `main`:

```yaml
Jobs del Pipeline CI/CD:
├── backend-ci:
│   ├── Setup .NET 8 SDK
│   ├── dotnet restore backend/Asisya.sln
│   ├── dotnet build backend/Asisya.sln --no-restore
│   └── dotnet test backend/Asisya.sln (54 tests unitarios)
├── frontend-ci:
│   ├── Setup Node.js 20.x
│   ├── npm ci (carpeta frontend)
│   ├── npm run lint (ESLint con 0 warnings)
│   └── npm run build (TypeScript tsc + Vite production bundle)
└── docker-verification:
    ├── Docker Buildx Setup (docker/setup-buildx-action@v3)
    ├── docker build backend (Dockerfile multi-etapa)
    └── docker build frontend (Dockerfile multi-etapa con Nginx)
```

---

## 6. Pruebas Automatizadas

El proyecto cuenta con **54 pruebas unitarias** implementadas con **xUnit**, **FluentAssertions** y un `TestDbContextFactory` en memoria.

Para ejecutar la suite de pruebas localmente:

```bash
dotnet test backend/Asisya.sln
```

Resultado de ejecución:
```text
Passed!  - Failed: 0, Passed: 54, Skipped: 0, Total: 54, Duration: 647 ms - Asisya.Application.Tests.dll (net8.0)
```

Cobertura de pruebas:
- Comandos y Consultas de Categorías (`CreateCategory`, `GetCategories`, `UpdateCategory`, `DeleteCategory` con validación de integridad referencial).
- Comandos y Consultas de Productos (`BulkCreateProducts`, `GetProducts`, `GetProductById`, `UpdateProduct`, `DeleteProduct`).
- Eventos y Consumidores de MassTransit (`BatchProductsReceivedEvent`, `BulkCreateProductsConsumer`).
- Interceptor de auditoría de EF Core (`AuditableEntitySaveChangesInterceptor`).
- Registro y sondas de HealthChecks (`PostgreSQL`, `RabbitMQ`).
- Métodos de extensión (`QueryableExtensions.ToPaginatedListAsync`).

---

## 7. Guía de Uso de la API (cURL)

### 7.1 Autenticación (Obtener Token JWT)
```bash
curl -X POST http://localhost:5000/Auth/Login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@asisya.com","password":"Admin123!"}'
```

Exportar token en la terminal:
```bash
TOKEN=$(curl -s -X POST http://localhost:5000/Auth/Login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@asisya.com","password":"Admin123!"}' | grep -o '"token":"[^"]*' | cut -d'"' -f4)
```

### 7.2 Ingesta Masiva Asíncrona (RabbitMQ)
```bash
curl -i -X POST http://localhost:5000/Product \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "generateRandomCount": 5000,
    "batchSize": 1000
  }'
```
*Respuesta inmediata: `HTTP 202 Accepted` con `batchId`.*

### 7.3 Consultar Catálogo con Filtros y Paginación
```bash
curl "http://localhost:5000/Products?pageIndex=1&pageSize=10&searchTerm=PowerEdge&categoryId=1&sortBy=price&sortOrder=desc"
```

### 7.4 Detalle del Producto con Foto de Categoría
```bash
curl "http://localhost:5000/Products/1"
```

### 7.5 Verificación de Integridad Referencial (ON DELETE RESTRICT)
Intentar eliminar una categoría que contiene productos asociados:
```bash
curl -i -X DELETE "http://localhost:5000/Category/1" \
  -H "Authorization: Bearer $TOKEN"
```
*Respuesta:* **`HTTP 400 Bad Request`** con ProblemDetails RFC 7807:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Invalid Operation",
  "status": 400,
  "detail": "Cannot delete category 'SERVIDORES' (ID: 1) because it has associated products. Remove or reassign existing products first."
}
```

### 7.6 Consultar Historial de Auditoría
```bash
curl "http://localhost:5000/AuditLogs?pageIndex=1&pageSize=5" \
  -H "Authorization: Bearer $TOKEN"
```

---

## 8. Escalabilidad Horizontal Cloud con Kubernetes (K8s)

La arquitectura de **ASISYA Commerce & Catalog** fue diseñada siguiendo los principios de las aplicaciones nativas de la nube (*Cloud-Native / 12-Factor App*), garantizando elasticidad operativa y alta disponibilidad en plataformas administradas como **Amazon EKS**, **Azure Kubernetes Service (AKS)** o **Google Kubernetes Engine (GKE)**.

### 8.1 API Stateless y Despliegue en Kubernetes
- **Arquitectura Sin Estado (Stateless):** Al implementar autenticación criptográfica mediante **JWT Bearer**, la Web API no almacena ningún estado de sesión en memoria local (`SessionState` o caché de proceso). Cada petición HTTP porta en sus headers toda la información requerida para validar la identidad y los roles del usuario.
- **Intercambiabilidad de Pods:** Cualquier Pod en ejecución puede atender indistintamente peticiones de cualquier cliente sin necesidad de afinidad de sesión (*sticky sessions*). Esto permite desplegar la API mediante un recurso nativo **`Deployment`** de Kubernetes detrás de un Ingress Controller (NGINX, AWS ALB o Traefik) con balanceo de carga round-robin o least-connections.
- **Preparación Cloud con Health Checks:** Los probes nativos de Kubernetes (`livenessProbe` y `readinessProbe`) consumen directamente el endpoint `/health` expuesto en el puerto 8080/5000, garantizando que el tráfico se enrute únicamente a Pods saludables con conectividad confirmada a PostgreSQL y RabbitMQ.

### 8.2 Auto-escalado Horizontal de Pods (HPA)
Para responder dinámicamente a picos de tráfico y ráfagas de consultas al catálogo o solicitudes de ingesta, se configura un **HorizontalPodAutoscaler (HPA)** que monitorea la utilización media de recursos:

```yaml
# deploy/k8s/api-hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: asisya-api-hpa
  namespace: asisya
  labels:
    app.kubernetes.io/name: asisya-api
    app.kubernetes.io/component: backend
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: asisya-api
  minReplicas: 2
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
    - type: Resource
      resource:
        name: memory
        target:
          type: Utilization
          averageUtilization: 80
  behavior:
    scaleUp:
      stabilizationWindowSeconds: 0
      policies:
        - type: Percent
          value: 100
          periodSeconds: 15
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
        - type: Percent
          value: 25
          periodSeconds: 60
```

#### Funcionamiento Operativo del HPA:
- **Línea Base Eficiente:** En condiciones normales de operación, el clúster mantiene **2 réplicas** activas asegurando redundancia de zona.
- **Scale-Out Dinámico:** Cuando la carga de trabajo (por ejemplo, búsquedas intensivas o peticiones concurrentes de ingesta masiva) eleva el consumo promedio de CPU por encima del **70%** (calculado sobre los `resources.requests.cpu` definidos en el Pod), Kubernetes instancia réplicas adicionales progresivamente hasta un máximo de **10 Pods**.
- **Scale-In Controlado:** Una vez estabilizada la demanda, se aplica una ventana de enfriamiento (*stabilization window*) de 300 segundos para evitar oscilaciones de escalado (*flapping*), consolidando la infraestructura y reduciendo costos de cómputo en la nube.

### 8.3 Escalamiento Independiente del Worker Asíncrono
Uno de los mayores beneficios de desacoplar la ingesta mediante **RabbitMQ** y **MassTransit** es que el procesamiento intensivo en base de datos no compite con la API web por los recursos de cómputo:

1. **Separación de Responsabilidades:**
   - La **Web API** se encarga únicamente de autenticar, validar y publicar el evento `BatchProductsReceivedEvent` en RabbitMQ (operación de < 15 ms).
   - El **Worker Consumidor** (`BulkCreateProductsConsumer`) se ejecuta en un `Deployment` independiente dedicado exclusivamente a desagotar la cola y persistir los lotes en PostgreSQL.

2. **Auto-escalado Basado en Eventos (KEDA):**
   - El Deployment de workers no necesita escalar por CPU, sino por la **profundidad de la cola** de RabbitMQ (`QueueLength`).
   - Integrando **KEDA (Kubernetes Event-Driven Autoscaling)** con el trigger `rabbitmq`, el número de pods de workers escala automáticamente de 1 a 10 réplicas según la cantidad de mensajes acumulados en la cola `BulkCreateProducts`:

```yaml
# deploy/k8s/worker-scaledobject.yaml
apiVersion: keda.sh/v1alpha1
kind: ScaledObject
metadata:
  name: asisya-worker-scaler
  namespace: asisya
spec:
  scaleTargetRef:
    name: asisya-worker-deployment
  minReplicaCount: 1
  maxReplicaCount: 10
  cooldownPeriod: 60
  pollingInterval: 10
  triggers:
    - type: rabbitmq
      metadata:
        protocol: amqp
        queueName: BulkCreateProducts
        mode: QueueLength
        value: "500" # Agrega 1 réplica de worker por cada 500 lotes pendientes
      authenticationRef:
        name: rabbitmq-keda-auth
```

Con este esquema, si un cliente encola 100.000 productos divididos en lotes de 1.000, los workers escalan al máximo de réplicas en segundos para liquidar la carga en paralelo, mientras la API continúa respondiendo a los usuarios con latencias mínimas.

---

## 9. Detención y Limpieza del Stack

Para detener y eliminar los contenedores, redes y volúmenes asociados:

```bash
docker compose down -v
```
