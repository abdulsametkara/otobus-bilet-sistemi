using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Web.Models;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Entities;

namespace OtobusBiletSistemi.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<Guzergah> _guzergahRepository;

    public HomeController(ILogger<HomeController> logger, IRepository<Guzergah> guzergahRepository)
    {
        _logger = logger;
        _guzergahRepository = guzergahRepository;
    }

    public async Task<IActionResult> Index()
    {
        var model = new SeferAramaViewModel();
        try
        {
            var guzergahlar = await _guzergahRepository.GetAllAsync();
            ViewBag.Guzergahlar = guzergahlar ?? new List<Guzergah>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Güzergah verileri yüklenemedi");
            ViewBag.Guzergahlar = new List<Guzergah>();
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SeferAra(SeferAramaViewModel model)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("SeferListesi", "Sefer", new { 
                nereden = model.Nereden, 
                nereye = model.Nereye, 
                tarih = model.Tarih.ToString("yyyy-MM-dd"),
                yolcuSayisi = model.YolcuSayisi 
            });
        }
        
        // Form hatası varsa güzergahları tekrar yükle
        try
        {
            var guzergahlar = await _guzergahRepository.GetAllAsync();
            ViewBag.Guzergahlar = guzergahlar ?? new List<Guzergah>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Güzergah verileri yüklenemedi");
            ViewBag.Guzergahlar = new List<Guzergah>();
        }
        
        return View("Index", model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
