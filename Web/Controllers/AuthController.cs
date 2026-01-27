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

    // ============================================
    // REGISTER
    // ============================================
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

      if(loginResult.success&&loginResult.customer!=null)
      {
        await SignInUserAsync(loginResult.customer);
        TempData["Success"]=$"Welcome to CarRental, {loginResult.customer.Name}!";
        return RedirectToAction("Index","Home");
      }

      TempData["Success"]="Registration successful! Please login.";
      return RedirectToAction(nameof(Login));
    }

    // ============================================
    // LOGIN
    // ============================================
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

      if(!result.success||result.customer==null)
      {
        ModelState.AddModelError(string.Empty,result.message);
        return View(model);
      }

      await SignInUserAsync(result.customer);
      TempData["Success"]=$"Welcome back, {result.customer.Name}!";

      if(!string.IsNullOrEmpty(returnUrl)&&Url.IsLocalUrl(returnUrl))
        return Redirect(returnUrl);

      return RedirectToAction("Index","Home");
    }

    // ============================================
    // LOGOUT
    // ============================================
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
      await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
      TempData["Info"]="You have been logged out successfully.";
      return RedirectToAction(nameof(Login));
    }

    // ============================================
    // HELPER METHOD
    // ============================================
    private async Task SignInUserAsync(ApplicationCore.Entities.Customer customer)
    {
      var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, customer.Id.ToString()),
                new Claim(ClaimTypes.Name, customer.Name),
                new Claim(ClaimTypes.Email, customer.Email),
                new Claim("CustomerId", customer.Id.ToString())
            };

      var claimsIdentity = new ClaimsIdentity(
          claims,
          CookieAuthenticationDefaults.AuthenticationScheme); // ✅ مهم جداً

      var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

      var authProperties = new AuthenticationProperties
      {
        IsPersistent=true,
        ExpiresUtc=DateTimeOffset.UtcNow.AddDays(7),
        AllowRefresh=true
      };

      await HttpContext.SignInAsync(
          CookieAuthenticationDefaults.AuthenticationScheme, // ✅ نفس الاسم
          claimsPrincipal,
          authProperties);
    }
  }
}