# Car Rental Enterprise ERP System

An enterprise-grade Car Rental & Vehicle Dealership Management ERP System built with .NET 8, ASP.NET Core MVC, Entity Framework Core, and SQL Server. The system follows Domain-Driven Design (DDD) query projection patterns and Clean Architecture principles to deliver a high-performance operational, financial, dealership sales, and fleet management platform.

---

## Architectural Overview

The solution is structured into distinct projects following Clean Architecture & DDD principles:

- **ApplicationCore**: Contains domain entities, system enums, repository interfaces, and core business domain models.
- **Application**: Implements application services, DTOs, domain services, audit logging, and optimized read-only DDD providers for ultra-fast query execution.
- **Infrastructure**: Handles database context (`AppDbContext`), EF Core migrations, repository implementations, Unit of Work pattern, and SQL Server data access routines.
- **Web**: ASP.NET Core MVC application containing controllers, ViewModels, Razor views, middleware pipeline, authentication, localization, and responsive glassmorphic UI layout.
- **CarRental.Tests**: Test suite for validating domain rules and application services.

---

## Core System Modules & Recent Enhancements

### 1. Executive ERP Dashboard & Real-Time Analytics
- **Live Fleet Metrics**: Real-time fleet utilization percentage, available vehicles, rented count, and maintenance tracking.
- **Financial Analytics**: Combined gross revenue (`Rental Payments + Dealership Vehicle Sales`), net profit calculation (`Net Profit = Gross Revenue - Total Fleet Maintenance Expenses`), and collected vs. pending receivables.
- **Interactive Analytics (Chart.js)**:
  - Monthly Contract Velocity (Line Chart).
  - Monthly Cash Flow & Revenue Comparison (Bar Chart).
  - Fleet Distribution & Availability (Doughnut Chart).
- **Rankings & Timelines**: Top 5 revenue-generating vehicles and upcoming/overdue contract returns.

### 2. Real-Time Admin Audit Stream & Live Floating Side-Toasts 🔔
- **Background Event Tracking**: Captures all employee operational actions across contracts, payments, vehicle sales, and maintenance.
- **Real-Time Side-Toast Alerts**: Client-side background polling (`/AuditLogs/GetLatestAudit`) that automatically slides in floating side-toast notifications at the top corner of the screen for the Admin.
- **Adaptive Light & Dark Mode Support**: Toast cards dynamically adjust colors, borders, and shadows seamlessly when toggling between Light and Dark themes.

### 3. Dealership Vehicle Sales & Installment Financing Engine 🏷️
- **Dual Fleet Listing Modes**: Fleet vehicles can be listed for `RentalOnly`, `SaleOnly`, or `Both`.
- **Payment Options**: Cash Sale (Full Settlement) or Installment Financing Plan (`6, 12, 24, 36, 48, 60` months).
- **Smart Deal Negotiator Assistant**: Interactive financing calculator analyzing total cost basis (Purchase Price + Refurbishment Cost), asking target price, minimum floor price limit, floor breach alert warnings, and profit margins.
- **Automatic Inventory Management**: Finalized sales update vehicle status to `Sold` / `OutOfService`, automatically removing sold vehicles from active showroom catalogs and rental availability while preserving historical ledgers.

### 4. Multi-Language Localization (English & Arabic RTL) 🌐
- **Dynamic Language Switcher**: Seamless switching between English (`en-US`) and Arabic (`ar-EG`).
- **Complete RTL Support**: Full Right-To-Left layout support with adjusted CSS grids, flex alignments, sidebars, and icons when Arabic is activated.
- **Comprehensive Key Translation**: Covers all dashboard cards, titles, quick action buttons, charts, tables, forms, and navigation menus via `@Loc.GetString(...)`.

### 5. Fleet & Maintenance Operations 🚗
- **Comprehensive Fleet Catalog**: Detailed vehicle tracking (Plate Number, Model, Year, Category, Daily Rate, Fuel Type, Transmission, Odometer, License/Insurance Expiry).
- **Maintenance Lifecycle**: Adding maintenance records sets vehicle status to `Maintenance`. Finishing maintenance restores status to `Available`.
- **Live GPS Telemetry**: Real-time GPS location coordinates and engine status logs.

### 6. Universal Newest-First DataGrid Ordering 📊
- **Enterprise DataTables**: Integrated with export capabilities (Copy, Excel, PDF, Print).
- **Default Reverse Chronological Order**: All tables initialize with `order: [[0, 'desc']]`, ensuring the newest contracts, sales, payments, and logs appear at the top by default.

### 7. Security, Audit Logging & User Access Control 🔐
- **Identity-Aware Audit Logging**: Injects `IHttpContextAccessor` to automatically log actions under the actual active logged-in user name (`Admin` / Employee Name).
- **Role-Based Access Control (RBAC)**: User roles (`Admin`, `Manager`, `Accountant`, `Employee`) with customizable action policies.
- **User Permissions Control Center**: Role assignments, one-click account suspension/activation, password resets, and role matrix reference.

---

## Technical Stack

- **Framework**: .NET 8 SDK
- **Web Layer**: ASP.NET Core MVC, Razor Views, Bootstrap 5, Bootstrap Icons, Chart.js, DataTables
- **ORM & Data Access**: Entity Framework Core 8, LINQ, SQL Server
- **Architecture**: Clean Architecture, DDD Query Providers, Repository & Unit of Work Patterns
- **Authentication**: ASP.NET Core Cookie Authentication & Custom Authorization Policies
- **Notifications**: NToastNotify (Toastr integration) + Custom Live Side-Toast Notification Stream

---

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (LocalDB `(localdb)\ProjectModels` or SQL Server Express / Enterprise)
- Visual Studio 2022 / VS Code / Rider

### Connection String Configuration
Update `Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\ProjectModels;Database=PortFioloApp1;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Running the Application

Using .NET CLI:
```bash
dotnet run --project Web/Web.csproj --launch-profile https
```

Open your browser and navigate to:
**`https://localhost:7258`**

---

## License

This project is developed for enterprise car rental and vehicle dealership operations. All rights reserved.
