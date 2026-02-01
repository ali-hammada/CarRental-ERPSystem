using Application.Services;
using Application.Services.Interfaces;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;
using InFrastructure.Repositories;
using InFrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
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
      builder.Services.AddLocalization(options => options.ResourcesPath="Resources");

      builder.Services.AddControllersWithViews()
             .AddViewLocalization()
             .AddDataAnnotationsLocalization();

      // ------------------------------
      // Authentication (Cookie)
      // ------------------------------
      builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
          .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,options =>
          {
            options.LoginPath="/Auth/Login";
            options.LogoutPath="/Auth/Logout";
            options.AccessDeniedPath="/Auth/AccessDenied";
            options.ExpireTimeSpan=TimeSpan.FromDays(7);
            options.SlidingExpiration=true;
            options.Cookie.HttpOnly=true;
            options.Cookie.Name="CarRental.Auth";
          });

      // ------------------------------
      // Dependency Injection
      // ------------------------------
      builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
      builder.Services.AddScoped<ICustomerRepository,CustomerRepository>();
      builder.Services.AddScoped<IEmployeeRepository,EmployeeRepository>();
      builder.Services.AddScoped<ICarRepository,CarRepository>();
      builder.Services.AddScoped<IRentalContractRepository,RentalContractRepository>();

      builder.Services.AddScoped<ICustomerServices,CustomerServices>();
      builder.Services.AddScoped<IEmployeeServices,EmployeeServices>();
      builder.Services.AddScoped<ICarServices,CarServices>();
      builder.Services.AddScoped<IRentalServices,RentalServices>();
      builder.Services.AddScoped<IAuthenticationServices,AuthenticationServices>();
      builder.Services.AddScoped<IPaymentServices,PaymentServices>();
      builder.Services.AddScoped<ITokenServices,TokenServices>();
      builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));

      builder.Services.AddDbContext<AppDbContext>(options =>
          options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

      builder.Services.AddAuthorization(options =>
      {
        options.AddPolicy("AdminOnly",policy =>
            policy.RequireClaim(ClaimTypes.Role,"Admin"));
        options.AddPolicy("AdminOrManager",policy =>
            policy.RequireClaim(ClaimTypes.Role,"Admin","Manager"));
        options.AddPolicy("AdminOrHR",policy =>
            policy.RequireClaim(ClaimTypes.Role,"Admin","HR"));
        options.AddPolicy("AdminOrAccountant",policy =>
            policy.RequireClaim(ClaimTypes.Role,"Admin","Accountant"));
      });

      builder.Services.AddMvc().AddNToastNotifyToastr(new ToastrOptions()
      {
        ProgressBar=true,
        PositionClass=ToastPositions.TopLeft,
        PreventDuplicates=true,
        CloseButton=true,
      });

      var app = builder.Build();

      // ------------------------------
      // Middleware pipeline
      // ------------------------------
      if(!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();

      // Localization setup
      var supportedCultures = new[]
      {
                new CultureInfo("en"),
                new CultureInfo("ar")
            };

      var localizationOptions = new RequestLocalizationOptions
      {
        DefaultRequestCulture=new RequestCulture("ar"),
        SupportedCultures=supportedCultures,
        SupportedUICultures=supportedCultures
      };

      // يسمح بتغيير اللغة عن طريق Cookie أو Query String
      localizationOptions.RequestCultureProviders.Insert(0,new CookieRequestCultureProvider());
      localizationOptions.RequestCultureProviders.Insert(1,new QueryStringRequestCultureProvider());

      app.UseRequestLocalization(localizationOptions);

      app.UseRouting();
      app.UseAuthentication();
      app.UseAuthorization();

      // Default route
      app.MapControllerRoute(
          name: "default",
          pattern: "{controller=Home}/{action=Dashboard}/{id?}");

      app.Run();
    }
  }
}
