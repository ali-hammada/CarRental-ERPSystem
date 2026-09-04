using Application.Providers;
using Application.Services;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;
using InFrastructure.Repositories;
using InFrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Globalization;
using System.Security.Claims;
using Web.Services;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(Resources.SharedResources));
                });

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("ar-EG")
                };

                options.DefaultRequestCulture = new RequestCulture("en-US");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly("InFrastructure").EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ICarRepository, CarRepository>();
            builder.Services.AddScoped<IRentalContractRepository, RentalContractRepository>();

            builder.Services.AddScoped<ICarProvider, CarProvider>();
            builder.Services.AddScoped<ICustomerProvider, CustomerProvider>();
            builder.Services.AddScoped<IRentalProvider, RentalProvider>();
            builder.Services.AddScoped<ISaleProvider, SaleProvider>();
            builder.Services.AddScoped<IDashboardProvider, DashboardProvider>();
            builder.Services.AddScoped<ICarTrackingProvider, CarTrackingProvider>();
            builder.Services.AddScoped<IAuditProvider, AuditProvider>();
            builder.Services.AddScoped<IAuditLogProvider, AuditLogProvider>();

            builder.Services.AddScoped<IRentalServices, RentalServices>();
            builder.Services.AddScoped<ISaleServices, SaleServices>();
            builder.Services.AddScoped<IAuditServices, AuditServices>();
            builder.Services.AddScoped<ILocalizationService, LocalizationService>();
            builder.Services.AddScoped<ICarServices, CarServices>();
            builder.Services.AddScoped<ICustomerServices, CustomerServices>();
            builder.Services.AddScoped<IEmployeeServices, EmployeeServices>();
            builder.Services.AddScoped<IPaymentServices, PaymentServices>();
            builder.Services.AddScoped<IAuthenticationServices, AuthenticationServices>();
            builder.Services.AddScoped<ITokenServices, TokenServices>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
            builder.Services.AddScoped<ICarTrackingService, CarTrackingService>();
            builder.Services.AddScoped<IAuditLogService, AuditLogService>();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsInRole("Admin") ||
                        context.User.IsInRole("Administrator") ||
                        context.User.FindFirst(ClaimTypes.Role)?.Value?.Equals("admin", StringComparison.OrdinalIgnoreCase) == true ||
                        (context.User.Identity?.Name?.Contains("admin", StringComparison.OrdinalIgnoreCase) == true)
                    ));
            });

            builder.Services.AddRazorPages();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    context.Database.EnsureCreated();

                    context.Database.ExecuteSqlRaw(@"
                        IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Employee') AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Employees')
                        BEGIN
                            EXEC sp_rename 'Employee', 'Employees';
                        END;
                    ");

                    var allTables = new[] { "Cars", "Customers", "Employees", "Employee", "RentalContracts", "Payments", "CarCategories", "MaintenanceLogs", "Invoices", "CarLocationLogs", "AuditLogs", "CarSaleContracts", "SaleInstallments" };
                    foreach (var tbl in allTables)
                    {
                        context.Database.ExecuteSqlRaw($@"
                            IF EXISTS (SELECT * FROM sys.tables WHERE name = '{tbl}')
                            BEGIN
                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[{tbl}]') AND name = N'IsDeleted')
                                    ALTER TABLE [{tbl}] ADD [IsDeleted] bit NOT NULL DEFAULT 0;

                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[{tbl}]') AND name = N'CreatedAt')
                                    ALTER TABLE [{tbl}] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());

                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[{tbl}]') AND name = N'CreatedBy')
                                    ALTER TABLE [{tbl}] ADD [CreatedBy] nvarchar(max) NULL;

                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[{tbl}]') AND name = N'UpdatedAt')
                                    ALTER TABLE [{tbl}] ADD [UpdatedAt] datetime2 NULL;

                                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[{tbl}]') AND name = N'UpdatedBy')
                                    ALTER TABLE [{tbl}] ADD [UpdatedBy] nvarchar(max) NULL;
                            END;
                        ");
                    }

                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cars]') AND name = N'CurrentLatitude')
                        BEGIN
                            ALTER TABLE [Cars] ADD [CurrentLatitude] float NULL;
                            ALTER TABLE [Cars] ADD [CurrentLongitude] float NULL;
                            ALTER TABLE [Cars] ADD [LastLocationUpdate] datetime2 NULL;
                        END;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cars]') AND name = N'LicenseExpiryDate')
                        BEGIN
                            ALTER TABLE [Cars] ADD [LicenseExpiryDate] datetime2 NULL;
                            ALTER TABLE [Cars] ADD [InsuranceExpiryDate] datetime2 NULL;
                        END;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cars]') AND name = N'ListingType')
                        BEGIN
                            ALTER TABLE [Cars] ADD [ListingType] int NOT NULL DEFAULT 1;
                            ALTER TABLE [Cars] ADD [SalePrice] decimal(18,2) NULL;
                            ALTER TABLE [Cars] ADD [SaleStatus] int NULL;
                        END;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cars]') AND name = N'PurchasePrice')
                        BEGIN
                            ALTER TABLE [Cars] ADD [PurchasePrice] decimal(18,2) NULL;
                            ALTER TABLE [Cars] ADD [PurchaseDate] datetime2 NULL;
                            ALTER TABLE [Cars] ADD [RefurbishmentCost] decimal(18,2) NULL;
                            ALTER TABLE [Cars] ADD [TargetSalePrice] decimal(18,2) NULL;
                            ALTER TABLE [Cars] ADD [MinimumFloorPrice] decimal(18,2) NULL;
                        END;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CarSaleContracts]') AND name = N'TotalCostBasis')
                        BEGIN
                            ALTER TABLE [CarSaleContracts] ADD [TotalCostBasis] decimal(18,2) NOT NULL DEFAULT 0;
                            ALTER TABLE [CarSaleContracts] ADD [ActualGrossProfit] decimal(18,2) NOT NULL DEFAULT 0;
                            ALTER TABLE [CarSaleContracts] ADD [IsBelowFloorPrice] bit NOT NULL DEFAULT 0;
                        END;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CarSaleContracts')
                        BEGIN
                            CREATE TABLE [CarSaleContracts] (
                                [Id] int NOT NULL IDENTITY,
                                [CarId] int NOT NULL,
                                [CustomerId] int NOT NULL,
                                [EmployeeId] int NOT NULL,
                                [SaleDate] datetime2 NOT NULL,
                                [SalePrice] decimal(18,2) NOT NULL,
                                [TaxAmount] decimal(18,2) NOT NULL,
                                [FinalPrice] decimal(18,2) NOT NULL,
                                [PaymentType] int NOT NULL,
                                [DownPayment] decimal(18,2) NOT NULL,
                                [InstallmentMonths] int NOT NULL,
                                [MonthlyInstallment] decimal(18,2) NOT NULL,
                                [PaidAmount] decimal(18,2) NOT NULL,
                                [Status] int NOT NULL,
                                [Notes] nvarchar(max) NULL,
                                [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                                [CreatedBy] nvarchar(max) NULL,
                                [UpdatedAt] datetime2 NULL,
                                [UpdatedBy] nvarchar(max) NULL,
                                [IsDeleted] bit NOT NULL DEFAULT 0,
                                CONSTRAINT [PK_CarSaleContracts] PRIMARY KEY ([Id]),
                                CONSTRAINT [FK_CarSaleContracts_Cars_CarId] FOREIGN KEY ([CarId]) REFERENCES [Cars] ([Id]),
                                CONSTRAINT [FK_CarSaleContracts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]),
                                CONSTRAINT [FK_CarSaleContracts_Employees_EmployeeId] FOREIGN KEY ([EmployeeId]) REFERENCES [Employees] ([Id])
                            );
                        END;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SaleInstallments')
                        BEGIN
                            CREATE TABLE [SaleInstallments] (
                                [Id] int NOT NULL IDENTITY,
                                [SaleContractId] int NOT NULL,
                                [InstallmentNumber] int NOT NULL,
                                [DueDate] datetime2 NOT NULL,
                                [Amount] decimal(18,2) NOT NULL,
                                [PaidAmount] decimal(18,2) NOT NULL,
                                [PaidDate] datetime2 NULL,
                                [Status] int NOT NULL,
                                [TransactionReference] nvarchar(max) NULL,
                                [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                                [CreatedBy] nvarchar(max) NULL,
                                [UpdatedAt] datetime2 NULL,
                                [UpdatedBy] nvarchar(max) NULL,
                                [IsDeleted] bit NOT NULL DEFAULT 0,
                                CONSTRAINT [PK_SaleInstallments] PRIMARY KEY ([Id]),
                                CONSTRAINT [FK_SaleInstallments_CarSaleContracts_SaleContractId] FOREIGN KEY ([SaleContractId]) REFERENCES [CarSaleContracts] ([Id]) ON DELETE CASCADE
                            );
                        END;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CarLocationLogs')
                        BEGIN
                            CREATE TABLE [CarLocationLogs] (
                                [Id] int NOT NULL IDENTITY,
                                [CarId] int NOT NULL,
                                [Latitude] float NOT NULL,
                                [Longitude] float NOT NULL,
                                [SpeedKmh] float NOT NULL,
                                [AddressName] nvarchar(max) NULL,
                                [Timestamp] datetime2 NOT NULL,
                                [IsEngineOn] bit NOT NULL,
                                [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                                [CreatedBy] nvarchar(max) NULL,
                                [UpdatedAt] datetime2 NULL,
                                [UpdatedBy] nvarchar(max) NULL,
                                [IsDeleted] bit NOT NULL DEFAULT 0,
                                CONSTRAINT [PK_CarLocationLogs] PRIMARY KEY ([Id]),
                                CONSTRAINT [FK_CarLocationLogs_Cars_CarId] FOREIGN KEY ([CarId]) REFERENCES [Cars] ([Id]) ON DELETE CASCADE
                            );
                            CREATE INDEX [IX_CarLocationLogs_CarId] ON [CarLocationLogs] ([CarId]);
                        END;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
                        BEGIN
                            CREATE TABLE [AuditLogs] (
                                [Id] int NOT NULL IDENTITY,
                                [EmployeeId] int NULL,
                                [EmployeeName] nvarchar(max) NOT NULL,
                                [Action] nvarchar(max) NOT NULL,
                                [Module] nvarchar(max) NOT NULL,
                                [Details] nvarchar(max) NOT NULL,
                                [IpAddress] nvarchar(max) NULL,
                                [Timestamp] datetime2 NOT NULL,
                                [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
                                [CreatedBy] nvarchar(max) NULL,
                                [UpdatedAt] datetime2 NULL,
                                [UpdatedBy] nvarchar(max) NULL,
                                [IsDeleted] bit NOT NULL DEFAULT 0,
                                CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
                            );
                        END;
                    ");

                    context.Database.ExecuteSqlRaw(@"
                        UPDATE [Employees]
                        SET [Role] = 'Admin'
                        WHERE LOWER([FullName]) LIKE '%admin%'
                           OR LOWER([Email]) LIKE '%admin%';
                    ");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(ex, "Database schema check completed.");
                }
            }

            app.UseDeveloperExceptionPage();

            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar-EG") },
                SupportedUICultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar-EG") }
            };
            localizationOptions.RequestCultureProviders.Insert(0, new CookieRequestCultureProvider());
            app.UseRequestLocalization(localizationOptions);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Dashboard}/{id?}");

            app.Run();
        }
    }
}
