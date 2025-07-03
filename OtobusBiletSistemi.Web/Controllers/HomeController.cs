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
    private readonly IRepository<Sefer> _seferRepository;

    public HomeController(ILogger<HomeController> logger, IRepository<Guzergah> guzergahRepository, IRepository<Sefer> seferRepository)
    {
        _logger = logger;
        _guzergahRepository = guzergahRepository;
        _seferRepository = seferRepository;
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

    [HttpGet]
    public async Task<IActionResult> GetMevcutSeferTarihleri(string nereden, string nereye)
    {
        try
        {
            if (string.IsNullOrEmpty(nereden) || string.IsNullOrEmpty(nereye))
            {
                return Json(new { success = false, message = "Lütfen kalkış ve varış şehirlerini seçin." });
            }

            // İlgili güzergahı bul
            var guzergahlar = await _guzergahRepository.GetAllAsync();
            var uygunGuzergah = guzergahlar.FirstOrDefault(g => 
                g.Nereden.Equals(nereden, StringComparison.OrdinalIgnoreCase) && 
                g.Nereye.Equals(nereye, StringComparison.OrdinalIgnoreCase));

            if (uygunGuzergah == null)
            {
                return Json(new { success = false, message = $"{nereden} - {nereye} güzergahı için tanımlı rota bulunamadı." });
            }

            // Bu güzergaha ait bugünden itibaren olan seferleri getir
            var seferler = await _seferRepository.GetAllAsync();
            var bugundenSonrakiSeferler = seferler
                .Where(s => s.GuzergahID == uygunGuzergah.GuzergahID && s.Tarih.Date >= DateTime.Today)
                .OrderBy(s => s.Tarih)
                .ToList();

            if (!bugundenSonrakiSeferler.Any())
            {
                return Json(new { 
                    success = false, 
                    message = $"{nereden} - {nereye} güzergahı için yakın tarihte sefer bulunmamaktadır." 
                });
            }

            // Benzersiz tarihleri çıkar ve string formatına çevir
            var mevcutTarihler = bugundenSonrakiSeferler
                .Select(s => s.Tarih.Date)
                .Distinct()
                .OrderBy(t => t)
                .Select(t => t.ToString("yyyy-MM-dd"))
                .ToList();

            return Json(new { 
                success = true, 
                dates = mevcutTarihler,
                message = $"{mevcutTarihler.Count} farklı tarihte sefer bulunmaktadır."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sefer tarihleri getirilirken hata oluştu");
            return Json(new { success = false, message = "Sefer tarihleri yüklenirken bir hata oluştu." });
        }
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
