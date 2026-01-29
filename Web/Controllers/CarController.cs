using Application.Services.Interfaces;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
  [Authorize]
  public class CarController:Controller
  {
    private readonly ICarServices _carServices;

    public CarController(ICarServices carServices)
    {
      _carServices=carServices;
    }

    public async Task<IActionResult> Index()
    {
      var cars = await _carServices.GetAllCarsAsync();
      return View(cars);
    }

    [HttpGet]
    public IActionResult Create()
    {
      var car = new Car();
      return View(car);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Car car)
    {
      if(!ModelState.IsValid)
        return View(car);

      car.Status=CarStatus.Available;
      await _carServices.AddCarAsync(car);

      TempData["Success"]="Car added successfully!";
      return RedirectToAction("Index");
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      var car = await _carServices.GetByIdAsync(id);
      if(car==null)
      {
        TempData["Success"]="Car Updated successfully!";
        return RedirectToAction("Index");
      }
      return View(car);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Car car)
    {
      if(!ModelState.IsValid)
        return View(car);


      await _carServices.UpdateCarAsync(car);

      TempData["Success"]="Car updated successfully!";
      return RedirectToAction("Index");
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
      var result = await _carServices.DeleteCarAsync(id);
      if(!result)
      {
        TempData["Error"]="Car could not be deleted!";
      }
      else
      {
        TempData["Success"]="Car deleted successfully!";
      }

      return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id,CarStatus status)
    {
      var car = await _carServices.GetByIdAsync(id);
      if(car==null)
      {
        TempData["Error"]="Car not found!";
        return RedirectToAction("Index");
      }

      car.Status=status;
      await _carServices.UpdateCarAsync(car);

      TempData["Success"]="Car status updated!";
      return RedirectToAction("Index");
    }
  }
}
