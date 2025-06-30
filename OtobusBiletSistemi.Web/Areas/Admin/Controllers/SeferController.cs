using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Data;
using System.Collections.Generic;
using System.Linq;

namespace OtobusBiletSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SeferController : Controller
    {
        private readonly IRepository<Sefer> _seferRepository;
        private readonly AppDbContext _context;

        public SeferController(IRepository<Sefer> seferRepository, AppDbContext context)
        {
            _seferRepository = seferRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchGuzergah, DateTime? filterTarih, string searchOtobus)
        {
            var seferler = await _context.Seferler
                .Include(s => s.Otobus)
                .Include(s => s.Guzergah)
                .ToListAsync();

            // Güzergah araması
            if (!string.IsNullOrEmpty(searchGuzergah))
            {
                seferler = seferler.Where(s => 
                    (s.Guzergah.Nereden.ToLower().Contains(searchGuzergah.ToLower())) ||
                    (s.Guzergah.Nereye.ToLower().Contains(searchGuzergah.ToLower()))
                ).ToList();
                ViewBag.SearchGuzergah = searchGuzergah;
            }

            // Tarih filtresi
            if (filterTarih.HasValue)
            {
                seferler = seferler.Where(s => s.Tarih.Date == filterTarih.Value.Date).ToList();
                ViewBag.FilterTarih = filterTarih.Value.ToString("yyyy-MM-dd");
            }

            // Otobüs araması
            if (!string.IsNullOrEmpty(searchOtobus))
            {
                seferler = seferler.Where(s => s.Otobus.Plaka.ToLower().Contains(searchOtobus.ToLower())).ToList();
                ViewBag.SearchOtobus = searchOtobus;
            }

            return View(seferler);
        }

        public async Task<IActionResult> Details(int id)
        {
            var sefer = await _context.Seferler
                .Include(s => s.Otobus)
                .Include(s => s.Guzergah)
                .FirstOrDefaultAsync(s => s.SeferID == id);
            
            if (sefer == null)
            {
                return NotFound();
            }
            return View(sefer);
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                var otobusler = await _context.Otobusler.ToListAsync();
                var guzergahlar = await _context.Guzergahlar.ToListAsync();
                
                ViewBag.Otobusler = otobusler;
                ViewBag.Guzergahlar = guzergahlar;
                
                // Boş model gönder
                var model = new Sefer
                {
                    Tarih = DateTime.Today.AddDays(1), // Yarın
                    Saat = "08:00" // Varsayılan saat
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Sayfa yüklenirken hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sefer model)
        {
            try
            {
                // DEBUG: Request bilgilerini kontrol et
                Console.WriteLine($"DEBUG - Request Method: {Request.Method}");
                Console.WriteLine($"DEBUG - Content Type: {Request.ContentType}");
                Console.WriteLine($"DEBUG - Form Keys: {string.Join(", ", Request.Form.Keys)}");
                
                // Form verilerini ayrı ayrı kontrol et
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"DEBUG - Form[{key}]: '{Request.Form[key]}'");
                }
                
                // DEBUG: Model binding sonuçlarını kontrol et
                Console.WriteLine($"DEBUG - Model binding sonuçları:");
                Console.WriteLine($"GuzergahID: {model?.GuzergahID}");
                Console.WriteLine($"OtobusID: {model?.OtobusID}");
                Console.WriteLine($"Tarih: {model?.Tarih}");
                Console.WriteLine($"Saat: '{model?.Saat}'");
                Console.WriteLine($"Kalkis: '{model?.Kalkis}'");
                Console.WriteLine($"Varis: '{model?.Varis}'");
                
                // Model null kontrolü
                if (model == null)
                {
                    model = new Sefer();
                }

                // Manuel validation (ModelState'i temizle ve kendi validation'ımızı kullan)
                ModelState.Clear();
                var errors = new List<string>();
                
                if (model.GuzergahID <= 0)
                    errors.Add("Güzergah seçilmesi zorunludur.");
                    
                if (model.OtobusID <= 0)
                    errors.Add("Otobüs seçilmesi zorunludur.");
                    
                if (model.Tarih == default(DateTime) || model.Tarih.Date < DateTime.Today)
                    errors.Add("Geçerli bir sefer tarihi girilmesi zorunludur.");
                    
                if (string.IsNullOrWhiteSpace(model.Saat))
                    errors.Add("Kalkış saati girilmesi zorunludur.");
                    
                if (string.IsNullOrWhiteSpace(model.Kalkis))
                    errors.Add("Kalkış terminali girilmesi zorunludur.");
                    
                if (string.IsNullOrWhiteSpace(model.Varis))
                    errors.Add("Varış terminali girilmesi zorunludur.");

                // Eğer hatalar varsa geri dön
                if (errors.Any())
                {
                    Console.WriteLine($"DEBUG - {errors.Count} validation hatası bulundu:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  - {error}");
                        ModelState.AddModelError("", error);
                    }
                    
                    // ViewBag'leri tekrar yükle
                    ViewBag.Otobusler = await _context.Otobusler.ToListAsync();
                    ViewBag.Guzergahlar = await _context.Guzergahlar.ToListAsync();
                    
                    return View(model);
                }

                // Çakışma kontrolü
                var existingSefer = await _context.Seferler
                    .Where(s => s.OtobusID == model.OtobusID && 
                               s.Tarih.Date == model.Tarih.Date && 
                               s.Saat == model.Saat.Trim())
                    .FirstOrDefaultAsync();

                if (existingSefer != null)
                {
                    ModelState.AddModelError("", "Bu otobüs için aynı tarih ve saatte zaten bir sefer bulunuyor.");
                    
                    ViewBag.Otobusler = await _context.Otobusler.ToListAsync();
                    ViewBag.Guzergahlar = await _context.Guzergahlar.ToListAsync();
                    
                    return View(model);
                }

                // Yeni sefer oluştur
                var yeniSefer = new Sefer
                {
                    GuzergahID = model.GuzergahID,
                    OtobusID = model.OtobusID,
                    Tarih = model.Tarih,
                    Saat = model.Saat.Trim(),
                    Kalkis = model.Kalkis.Trim(),
                    Varis = model.Varis.Trim()
                };

                await _seferRepository.AddAsync(yeniSefer);
                TempData["SuccessMessage"] = "Sefer başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Sefer eklenirken hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // ID parametresini kontrol et
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "Geçersiz sefer ID'si.";
                    return RedirectToAction("Index");
                }

                var sefer = await _context.Seferler
                    .Include(s => s.Otobus)
                    .Include(s => s.Guzergah)
                    .FirstOrDefaultAsync(s => s.SeferID == id);
                
                if (sefer == null)
                {
                    TempData["ErrorMessage"] = $"#{id} numaralı sefer bulunamadı.";
                    return RedirectToAction("Index");
                }
                
                // ViewBag'leri yükle
                var otobusler = await _context.Otobusler.ToListAsync();
                var guzergahlar = await _context.Guzergahlar.ToListAsync();
                
                ViewBag.Otobusler = otobusler;
                ViewBag.Guzergahlar = guzergahlar;
                
                return View(sefer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Sefer düzenleme sayfası yüklenirken hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sefer sefer)
        {
            try
            {
                // DEBUG: Form verilerini kontrol et
                Console.WriteLine($"DEBUG - Edit POST - Form Keys: {string.Join(", ", Request.Form.Keys)}");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"DEBUG - Edit Form[{key}]: '{Request.Form[key]}'");
                }
                
                Console.WriteLine($"DEBUG - Edit Model binding sonuçları:");
                Console.WriteLine($"SeferID: {sefer?.SeferID}");
                Console.WriteLine($"GuzergahID: {sefer?.GuzergahID}");
                Console.WriteLine($"OtobusID: {sefer?.OtobusID}");
                Console.WriteLine($"Tarih: {sefer?.Tarih}");
                Console.WriteLine($"Saat: '{sefer?.Saat}'");
                Console.WriteLine($"Kalkis: '{sefer?.Kalkis}'");
                Console.WriteLine($"Varis: '{sefer?.Varis}'");
                
                Console.WriteLine($"DEBUG - URL ID parametresi: {id}");
                
                if (sefer == null)
                {
                    TempData["ErrorMessage"] = "Model boş geldi.";
                    return RedirectToAction("Index");
                }
                
                if (id != sefer.SeferID)
                {
                    Console.WriteLine($"DEBUG - ID eşleşmiyor: URL ID={id}, Model SeferID={sefer.SeferID}");
                    TempData["ErrorMessage"] = "Sefer ID'si eşleşmiyor.";
                    return RedirectToAction("Index");
                }

                // ModelState'i temizle ve manuel validation kullan
                ModelState.Clear();
                var errors = new List<string>();
                
                if (sefer.GuzergahID <= 0)
                    errors.Add("Güzergah seçilmesi zorunludur.");
                    
                if (sefer.OtobusID <= 0)
                    errors.Add("Otobüs seçilmesi zorunludur.");
                    
                if (sefer.Tarih == default(DateTime) || sefer.Tarih.Date < DateTime.Today)
                    errors.Add("Geçerli bir sefer tarihi girilmesi zorunludur.");
                    
                if (string.IsNullOrWhiteSpace(sefer.Saat))
                    errors.Add("Kalkış saati girilmesi zorunludur.");
                    
                if (string.IsNullOrWhiteSpace(sefer.Kalkis))
                    errors.Add("Kalkış terminali girilmesi zorunludur.");
                    
                if (string.IsNullOrWhiteSpace(sefer.Varis))
                    errors.Add("Varış terminali girilmesi zorunludur.");

                // Eğer validation hataları varsa
                if (errors.Any())
                {
                    Console.WriteLine($"DEBUG - Edit {errors.Count} validation hatası bulundu:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  - {error}");
                        ModelState.AddModelError("", error);
                    }
                    
                    // ViewBag'leri tekrar yükle
                    ViewBag.Otobusler = await _context.Otobusler.ToListAsync();
                    ViewBag.Guzergahlar = await _context.Guzergahlar.ToListAsync();
                    
                    return View(sefer);
                }

                // Aynı otobüs, aynı tarih ve saatte başka sefer var mı kontrol et (kendi dışında)
                var existingSefer = await _context.Seferler
                    .Where(s => s.SeferID != id &&
                               s.OtobusID == sefer.OtobusID && 
                               s.Tarih.Date == sefer.Tarih.Date && 
                               s.Saat == sefer.Saat.Trim())
                    .FirstOrDefaultAsync();

                if (existingSefer != null)
                {
                    ModelState.AddModelError("", "Bu otobüs için aynı tarih ve saatte zaten bir sefer bulunuyor.");
                    
                    ViewBag.Otobusler = await _context.Otobusler.ToListAsync();
                    ViewBag.Guzergahlar = await _context.Guzergahlar.ToListAsync();
                    
                    return View(sefer);
                }

                // Güncelleme işlemi
                Console.WriteLine($"DEBUG - Sefer güncelleniyor: SeferID={sefer.SeferID}");
                await _seferRepository.UpdateAsync(sefer);
                TempData["SuccessMessage"] = "Sefer başarıyla güncellendi.";
                Console.WriteLine($"DEBUG - Sefer başarıyla güncellendi, Index'e yönlendiriliyor");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG - Edit exception: {ex.Message}");
                TempData["ErrorMessage"] = $"Sefer güncellenirken hata oluştu: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var sefer = await _context.Seferler
                    .Include(s => s.Otobus)
                    .Include(s => s.Guzergah)
                    .FirstOrDefaultAsync(s => s.SeferID == id);
                
                if (sefer == null)
                {
                    TempData["ErrorMessage"] = "Sefer bulunamadı.";
                    return RedirectToAction("Index");
                }

                var biletler = await _context.Biletler.Where(b => b.SeferID == id).CountAsync();
                if (biletler > 0)
                {
                    TempData["ErrorMessage"] = $"#{sefer.SeferID} numaralı seferin satılmış biletleri bulunuyor. Önce biletleri iptal edin.";
                    return RedirectToAction("Index");
                }

                await _seferRepository.DeleteAsync(sefer);
                TempData["SuccessMessage"] = $"#{sefer.SeferID} numaralı sefer başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Sefer silinirken hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index");
        }


    }
} 
 

