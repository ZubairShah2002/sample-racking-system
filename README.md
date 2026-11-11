# Sample Racking Management System

A **standalone, enterprise-grade application** for managing physical QC sample racks:
registration/receiving, an interactive 3D rack, search & tracking, aging/overdue
monitoring, testing, movement history, disposal with approval, master data, QR
labels, Excel import, reports, notifications, and its own user management & security.

This is a completely independent system — its own frontend, backend, database,
authentication, dashboard, navigation, and deployment. It does **not** depend on,
embed into, or share anything with any other application.

---

## Architecture

| Layer | Technology |
| --- | --- |
| Frontend | React 19 + TypeScript + Vite, React Query, Zustand, Three.js / React-Three-Fiber (3D rack) |
| Backend | .NET 8 Web API, Clean Architecture + DDD, CQRS (MediatR), FluentValidation |
| Database | PostgreSQL 16 (per-module schemas, Row-Level Security, audit trail) |
| Auth | JWT bearer tokens, BCrypt password hashing, role-based authorization |
| Deploy | Docker Compose (db + api + web) |

```
sample-racking-system/
├── docker-compose.yml          # db + api + web
├── docker/init-db.sql          # idempotent schema + seed (shared/identity/sample_tracking)
├── backend/                    # .NET 8 solution (SampleRack.*)
│   └── src/
│       ├── SampleRack.SharedKernel/     # base types, Result, value objects
│       ├── SampleRack.Domain/           # Identity + SampleTracking aggregates
│       ├── SampleRack.Application/      # CQRS commands/queries, auth, validators
│       ├── SampleRack.Infrastructure/   # EF Core, repositories, JWT, storage
│       └── SampleRack.Api/              # controllers, middleware, startup, seeding
└── frontend/                   # React + Vite app
    └── src/
        ├── components/  (ui, feedback, layout, ProtectedRoute)
        ├── features/    (auth, sampleTracking)
        ├── pages/       (LoginPage, sample-tracking/*)
        └── lib / stores / styles
```

### Roles

| Role | Capabilities |
| --- | --- |
| **Administrator** | Everything: user management, settings, master data, disposal approval |
| **QC Manager** | Manage racks & samples, approve disposals, oversee QC |
| **QC Technician** | Register, test, and move samples |
| **Viewer** | Read-only dashboards, inventory, and reports |

---

## Quick start (Docker)

```bash
cp .env.example .env     # then edit secrets (set a strong JWT_KEY)
docker compose up --build
```

- Web UI: <http://localhost:8080>
- API + Swagger: <http://localhost:5280/swagger>
- Default login: **admin / Admin@12345** (change immediately via **Users → Reset**, or set `ADMIN_PASSWORD` before first run)

The database schema and the default administrator are created automatically on
first startup. The schema script is idempotent, so restarts are safe.

---

## Local development

**Backend** (needs .NET 8 SDK + a PostgreSQL instance):

```bash
cd backend
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=samplerack;Username=postgres;Password=postgres"
export Jwt__Key="a-long-random-dev-secret-at-least-32-bytes"
dotnet run --project src/SampleRack.Api        # http://localhost:5280
dotnet test                                     # run unit tests
```

**Frontend** (needs Node 20+):

```bash
cd frontend
npm install
npm run dev        # http://localhost:5173  (proxies /api to http://localhost:5280)
npm run build      # type-check + production build
```

---

## Business rules

- Photo-matched location codes: **A1-01-10 … A1-03-40** (3 bays × 4 shelves, shelf levels in tens).
- Multiple samples may share one location (capacity-aware).
- **Moving a sample never resets its aging.** Testing status and aging are independent.
- Overdue samples are **never auto-disposed**; disposal requires an authorized approval.
- Disposed samples retain their full history.
- Every create/update/delete is written to an audit trail.

---

## Configuration

All configuration is via environment variables (see `.env.example`):
`ConnectionStrings__DefaultConnection`, `Jwt__Key` / `Jwt__Issuer` / `Jwt__Audience`
/ `Jwt__ExpiryMinutes`, `Seed__Admin*`, `FileStorage__RootPath`, `Cors__AllowedOrigins`.

> **Security:** always set a strong, random `JWT_KEY` and change the seeded admin
> password before exposing the system.
