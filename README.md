# Car Rental Enterprise ERP System

An enterprise-grade Car Rental Management & ERP System built with .NET 8, ASP.NET Core MVC, Entity Framework Core, and SQL Server. The system follows Domain-Driven Design (DDD) query projection patterns and Clean Architecture principles to deliver a high-performance operational, financial, and fleet management platform.

---

## Architectural Overview

The solution is divided into distinct projects following Clean Architecture principles:

- **ApplicationCore**: Contains domain entities, system enums, repository interfaces, and core business abstractions.
- **Application**: Implements application services, DTOs, domain services, and optimized read-only DDD providers for fast data retrieval.
- **Infrastructure**: Handles database context (`AppDbContext`), EF Core migrations, repository implementations, Unit of Work pattern, and data access routines.
- **Web**: ASP.NET Core MVC application containing controllers, ViewModels, Razor views, middleware pipeline, authentication, and responsive glassmorphic UI layout.
- **CarRental.Tests**: Test suite for validating domain rules and application services.

---

## Core System Modules

### Executive ERP Dashboard
- Real-time fleet utilization rate calculation.
- Net operating revenue calculation (`Net Profit = Gross Revenue - Total Fleet Maintenance Expenses`).
- Financial metrics covering collected revenue, pending receivables, and payment method breakdowns.
- Visual analytics using Chart.js:
  - Monthly Contract Velocity (Line Chart).
  - Monthly Cash Flow & Revenues (Bar Chart).
  - Fleet Distribution & Availability (Doughnut Chart).
- Top 5 revenue-generating vehicles ranking list.
- Upcoming and overdue return contract timeline with quick action triggers.

### Fleet & Maintenance Operations
- Full fleet catalog management (Brand, Model, Year, Category, Daily Price, Fuel Type, Transmission).
- Real-time vehicle status tracking (`Available`, `Rented`, `Maintenance`, `OutOfService`).
- Maintenance log management (Service Type, Service Date, Provider, Odometer, Cost).
- Maintenance lifecycle workflow:
  - Adding maintenance records sets vehicle status to `Maintenance`.
  - "End Maintenance" action restores vehicle status to `Available` for immediate re-booking.
  - Automatic deduction of maintenance expenses from net profit metrics.
- Live GPS vehicle telemetry and location tracking.

### Rental Contracts Management
- Rental contract lifecycle (Open, Close, Cancel, Extend).
- Automatic period conflict detection to prevent double bookings.
- Odometer reading and fuel level tracking at departure and return.
- Pricing calculation including daily rates, extra fees, deposits, and remaining balances.

### Financials & Tax Invoices
- Multi-purpose payment ledger (Deposit, Partial, Final, Penalty, Refund).
- Support for multiple payment methods (Cash, Credit Card, Debit Card, Bank Transfer, Online Gateway).
- Automatic generation of tax invoices and printable payment receipts.
- Outstanding balance tracking across active contracts.

### Security, Roles & User Permissions
- Cookie-based authentication with role claims.
- Role-Based Access Control (RBAC) with custom authorization policies.
- User Roles & Access Control Center (`/Employees/Permissions`):
  - User role assignment (`Admin`, `Manager`, `Accountant`, `Employee`).
  - One-click account activation and suspension toggle.
  - Quick administrative password reset.
  - Role capability matrix reference.
- Automatic scope isolation: Staff users manage their own contracts, while Admin users access system-wide enterprise data.

### Audit & Security Monitor
- System-wide audit logging tracking user actions, entity modifications, timestamps, and module activities.

---

## Technical Stack

- **Framework**: .NET 8 SDK
- **Web Layer**: ASP.NET Core MVC, Razor Pages, Bootstrap 5, Bootstrap Icons, Chart.js, DataTables
- **ORM & Data Access**: Entity Framework Core 8, LINQ, SQL Server
- **Architecture**: Clean Architecture, Domain-Driven Design (DDD) Providers, Repository & Unit of Work Patterns
- **Authentication**: ASP.NET Core Cookie Authentication & Custom Authorization Policies
- **Notifications**: NToastNotify (Toastr integration)

---

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (LocalDB or SQL Server Express / Enterprise)
- Visual Studio 2022 / VS Code / Rider

### Configuration
Update the connection string in `Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CarRentalDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Database Initialization
The application automatically creates the database schema and required tables on initial launch via EF Core `EnsureCreated()` and startup scripts. Alternatively, you can apply migrations manually:

```bash
dotnet ef database update --project Infrastructure --startup-project Web
```

### Running the Application

Using .NET CLI:
```bash
dotnet run --project Web/Web.csproj
```

Open your browser and navigate to `https://localhost:7001` or `http://localhost:5000`.

---

## License

This project is developed for enterprise car rental operations and management. All rights reserved.
