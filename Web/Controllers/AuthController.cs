using Application.Services.DTO_s;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
  public class AuthController:Controller
  {
    private readonly IAuthenticationServices _authService;

    public AuthController(IAuthenticationServices authService)
    {
      _authService=authService;
    }
    [HttpGet]
    public IActionResult Register()
    {
      if(User.Identity?.IsAuthenticated==true)
        return RedirectToAction("Index","Home");

      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto model)
    {
      if(!ModelState.IsValid)
        return View(model);

      var result = await _authService.RegisterAsync(model);

      if(!result.success)
      {
        ModelState.AddModelError(string.Empty,result.message);
        return View(model);
      }

      // Login تلقائي
      var loginResult = await _authService.LogInAsync(new LoginDto
      {
        Email=model.Email,
        Password=model.Password
      });

      if(loginResult.success&&loginResult.employee!=null)
      {
        await SignInUserAsync(loginResult.employee);
        TempData["Success"]=$"Welcome to CarRental, {loginResult.employee.FullName}!";
        return RedirectToAction("Index","Home");
      }

      TempData["Success"]="Registration successful! Please login.";
      return RedirectToAction(nameof(Login));
    }
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
      if(User.Identity?.IsAuthenticated==true)
        return RedirectToAction("Index","Home");

      ViewData["ReturnUrl"]=returnUrl;
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model,string? returnUrl = null)
    {
      if(!ModelState.IsValid)
        return View(model);

      var result = await _authService.LogInAsync(model);

      if(!result.success||result.employee==null)
      {
        ModelState.AddModelError(string.Empty,result.message);
        return View(model);
      }

      await SignInUserAsync(result.employee);
      TempData["Success"]=$"Welcome back, {result.employee.FullName}!";

      if(!string.IsNullOrEmpty(returnUrl)&&Url.IsLocalUrl(returnUrl))
        return Redirect(returnUrl);

      return RedirectToAction("Index","Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      TempData["Info"]="You have been logged out successfully.";
      return RedirectToAction(nameof(Login));
    }

    private async Task SignInUserAsync(ApplicationCore.Entities.Employees employee)
    {
      var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
        new Claim(ClaimTypes.Name, employee.FullName),
        new Claim(ClaimTypes.Email, employee.Email),
        new Claim(ClaimTypes.Role, employee.Role),
        new Claim("EmployeeId", employee.Id.ToString())
    };

      var claimsIdentity = new ClaimsIdentity(
          claims,
          CookieAuthenticationDefaults.AuthenticationScheme);

      var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      var authProperties = new AuthenticationProperties
      {
        IsPersistent=true,
        ExpiresUtc=DateTimeOffset.UtcNow.AddDays(7),
        AllowRefresh=true
      };

      await HttpContext.SignInAsync(
          CookieAuthenticationDefaults.AuthenticationScheme,
          claimsPrincipal,
          authProperties);
    }

  }
}