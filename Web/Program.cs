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
using NToastNotify;
using System.Globalization;
using System.Security.Claims;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ------------------------------
            // Localization
            // ------------------------------
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

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

            // ------------------------------
            // DB Context with Transient Failure Resiliency
            // ------------------------------
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly("InFrastructure").EnableRetryOnFailure()));

            // ------------------------------
            // Repositories & Services
            // ------------------------------
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ICarRepository, CarRepository>();
            builder.Services.AddScoped<IRentalContractRepository, RentalContractRepository>();

            // DDD Fast Providers
            builder.Services.AddScoped<IRentalProvider, RentalProvider>();
            builder.Services.AddScoped<ICarProvider, CarProvider>();
            builder.Services.AddScoped<ICustomerProvider, CustomerProvider>();
            builder.Services.AddScoped<IDashboardProvider, DashboardProvider>();
            builder.Services.AddScoped<ICarTrackingProvider, CarTrackingProvider>();
            builder.Services.AddScoped<IAuditLogProvider, AuditLogProvider>();

            // DDD Domain Services
            builder.Services.AddScoped<IRentalServices, RentalServices>();
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

            // ------------------------------
            // Cookie Authentication
            // ------------------------------
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/AccessDenied";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = true;
                });

            // ------------------------------
            // NToastNotify
            // ------------------------------
            builder.Services.AddRazorPages().AddNToastNotifyToastr(new ToastrOptions
            {
                ProgressBar = true,
                PositionClass = ToastPositions.TopLeft,
                PreventDuplicates = true,
                CloseButton = true,
            });

            var app = builder.Build();

            // Automatic Database & New Tables Safe Initialization
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    context.Database.EnsureCreated();

                    // Ensure GPS columns exist on Cars table
                    context.Database.ExecuteSqlRaw(@"
                        IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Cars]') AND name = N'CurrentLatitude')
                        BEGIN
                            ALTER TABLE [Cars] ADD [CurrentLatitude] float NULL;
                            ALTER TABLE [Cars] ADD [CurrentLongitude] float NULL;
                            ALTER TABLE [Cars] ADD [LastLocationUpdate] datetime2 NULL;
                        END;
                    ");

                    // Ensure CarLocationLogs table exists
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

                    // Ensure AuditLogs table exists
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
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogWarning(ex, "Database initialization note.");
                }
            }

            // ------------------------------
            // Middleware pipeline
            // ------------------------------
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar-EG") },
                SupportedUICultures = new[] { new CultureInfo("en-US"), new CultureInfo("ar-EG") }
            };
            app.UseRequestLocalization(localizationOptions);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseNToastNotify();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Dashboard}/{id?}");

            app.Run();
        }
    }
}
