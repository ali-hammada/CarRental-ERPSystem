using Application.Services;
using Application.Services.Interfaces;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;
using InFrastructure.Repositories;
using InFrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NToastNotify;


namespace Web
{
  public class Program
  {

    public static void Main(string[] args)
    {
      var builder = WebApplication.CreateBuilder(args);



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
      builder.Services.AddControllersWithViews();
      builder.Services.AddAntiforgery();
      builder.Services.AddScoped<ICustomerRepository,CustomerRepository>();
      builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();

      builder.Services.AddScoped<ICustomerServices,CustomerServices>();
      builder.Services.AddScoped<IEmployeeServices,EmployeeServices>();
      builder.Services.AddScoped<IEmployeeRepository,EmployeeRepository>();

      builder.Services.AddScoped<ICarServices,CarServices>();
      builder.Services.AddScoped<ITokenServices,TokenServices>();
      builder.Services.AddScoped<IRentalServices,RentalServices>();
      builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
      builder.Services.AddScoped<IAuthenticationServices,AuthenticationServices>();
      builder.Services.AddScoped<IPaymentServices,PaymentServices>();
      builder.Services.AddMvc().AddNToastNotifyToastr(new ToastrOptions()
      {
        ProgressBar=true,
        PositionClass=ToastPositions.TopLeft,
        PreventDuplicates=true,
        CloseButton=true,
      });

      builder.Services.AddScoped<IRentalContractRepository,RentalContractRepository>();
      builder.Services.AddScoped<ICarRepository,CarRepository>();
      builder.Services.AddDbContext<AppDbContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


      var app = builder.Build();

      // Configure the HTTP request pipeline.
      if(!app.Environment.IsDevelopment())
      {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
      }

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
