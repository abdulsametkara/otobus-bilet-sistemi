using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace OtobusBiletSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OtobusController : Controller
    {
        private readonly IRepository<Otobus> _otobusRepository;
        private readonly AppDbContext _context;

        public OtobusController(IRepository<Otobus> otobusRepository, AppDbContext context)
        {
            _otobusRepository = otobusRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchPlaka, string filterTip)
        {
            var otobusler = await _otobusRepository.GetAllAsync();
            
            if (!string.IsNullOrEmpty(searchPlaka))
            {
                otobusler = otobusler.Where(o => o.Plaka.ToLower().Contains(searchPlaka.ToLower())).ToList();
                ViewBag.SearchPlaka = searchPlaka;
            }
            
            if (!string.IsNullOrEmpty(filterTip))
            {
                otobusler = otobusler.Where(o => o.OtobusTipi.Equals(filterTip, StringComparison.OrdinalIgnoreCase)).ToList();
                ViewBag.FilterTip = filterTip;
            }
            
            return View(otobusler);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Otobus otobus)
        {
            try
            {
                var plaka = Request.Form["Plaka"].ToString();
                var otobusTipi = Request.Form["OtobusTipi"].ToString();
                var koltukSayisiStr = Request.Form["KoltukSayısı"].ToString();

                if (string.IsNullOrEmpty(plaka) || string.IsNullOrEmpty(otobusTipi) || 
                    string.IsNullOrEmpty(koltukSayisiStr))
                {
                    ModelState.AddModelError("", "Tüm alanlar zorunludur.");
                    return View(otobus);
                }

                if (!int.TryParse(koltukSayisiStr, out int koltukSayisi) || koltukSayisi < 20 || koltukSayisi > 60)
                {
                    ModelState.AddModelError("", "Koltuk sayısı 20-60 arasında olmalıdır.");
                    return View(otobus);
                }

                var yeniOtobus = new Otobus
                {
                    Plaka = plaka.ToUpper(),
                    OtobusTipi = otobusTipi,
                    KoltukSayısı = koltukSayisi
                };

                await _otobusRepository.AddAsync(yeniOtobus);
                TempData["SuccessMessage"] = "Otobüs başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata: {ex.Message}");
            }

            return View(otobus);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var otobus = await _otobusRepository.GetByIdAsync(id);
            if (otobus == null)
            {
                return NotFound();
            }
            return View(otobus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Otobus otobus)
        {
            try
            {
                var plaka = Request.Form["Plaka"].ToString();
                var otobusTipi = Request.Form["OtobusTipi"].ToString();
                var koltukSayisiStr = Request.Form["KoltukSayısı"].ToString();

                if (string.IsNullOrEmpty(plaka) || string.IsNullOrEmpty(otobusTipi) || 
                    string.IsNullOrEmpty(koltukSayisiStr))
                {
                    ModelState.AddModelError("", "Tüm alanlar zorunludur.");
                    var otobusForError = await _otobusRepository.GetByIdAsync(id);
                    return View(otobusForError ?? otobus);
                }

                if (!int.TryParse(koltukSayisiStr, out int koltukSayisi) || koltukSayisi < 20 || koltukSayisi > 60)
                {
                    ModelState.AddModelError("", "Koltuk sayısı 20-60 arasında olmalıdır.");
                    var otobusForError = await _otobusRepository.GetByIdAsync(id);
                    return View(otobusForError ?? otobus);
                }

                var otobusToUpdate = await _otobusRepository.GetByIdAsync(id);
                if (otobusToUpdate == null)
                {
                    TempData["ErrorMessage"] = "Otobüs bulunamadı.";
                    return RedirectToAction("Index");
                }

                otobusToUpdate.Plaka = plaka.ToUpper();
                otobusToUpdate.OtobusTipi = otobusTipi;
                otobusToUpdate.KoltukSayısı = koltukSayisi;

                await _otobusRepository.UpdateAsync(otobusToUpdate);
                TempData["SuccessMessage"] = "Otobüs başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata: {ex.Message}");
            }

            var otobusForView = await _otobusRepository.GetByIdAsync(id);
            return View(otobusForView ?? otobus);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var otobus = await _otobusRepository.GetByIdAsync(id);
            if (otobus == null)
            {
                return NotFound();
            }
            return View(otobus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var otobus = await _otobusRepository.GetByIdAsync(id);
                if (otobus == null)
                {
                    TempData["ErrorMessage"] = "Otobüs bulunamadı.";
                    return RedirectToAction("Index");
                }

                var sefers = await _context.Seferler.Where(s => s.OtobusID == id).CountAsync();
                if (sefers > 0)
                {
                    TempData["ErrorMessage"] = $"{otobus.Plaka} plakasındaki otobüsün aktif seferleri bulunuyor. Önce seferleri silin.";
                    return RedirectToAction("Index");
                }

                var deleted = await _otobusRepository.DeleteAsync(id);
                if (deleted)
                {
                    TempData["SuccessMessage"] = $"{otobus.Plaka} plakasındaki otobüs başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Otobüs silinemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Otobüs silinirken hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var otobus = await _otobusRepository.GetByIdAsync(id);
            if (otobus == null)
            {
                return NotFound();
            }
            return View(otobus);
        }
    }
} 