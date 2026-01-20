using InFrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web.Models;

namespace Web.Controllers
{
  public class HomeController:Controller
  {
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _appDbContext;
    public HomeController(ILogger<HomeController> logger,AppDbContext appDbContext)
    {
      _logger=logger;
      _appDbContext=appDbContext;
    }

    public IActionResult Index()
    {
      return View();
    }
    public IActionResult About()
    {
      return View();
    }


    public IActionResult Privacy()
    {
      return View();
    }

    [ResponseCache(Duration = 0,Location = ResponseCacheLocation.None,NoStore = true)]
    public IActionResult Error()
    {
      return View(new ErrorViewModel { RequestId=Activity.Current?.Id??HttpContext.TraceIdentifier });
    }
  }
}
