using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Data;

namespace OtobusBiletSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BiletController : Controller
    {
        private readonly IRepository<Bilet> _biletRepository;
        private readonly AppDbContext _context;

        public BiletController(IRepository<Bilet> biletRepository, AppDbContext context)
        {
            _biletRepository = biletRepository;
            _context = context;
        }

        public async Task<IActionResult> Index(string searchBiletNo, string searchYolcu, string searchSefer, 
            string filterDurum, DateTime? filterTarih, int? filterGuzergah)
        {
            try
            {
                // Tüm biletleri ilişkili veriler ile birlikte doğrudan yükle
                var biletler = await _context.Biletler
                    .Include(b => b.Sefer)
                        .ThenInclude(s => s.Guzergah)
                    .Include(b => b.Sefer)
                        .ThenInclude(s => s.Otobus)
                    .Include(b => b.Koltuk)
                    .ToListAsync();

                // YolcuUser bilgilerini manuel olarak yükleyip ilişkilendir
                var yolcuUsers = await _context.Users.ToListAsync();
                
                // Her bilet için ilgili YolcuUser'ı bulup ViewData'da sakla
                var yolcuBilgileri = new Dictionary<int, YolcuUser>();
                foreach (var bilet in biletler)
                {
                    var yolcuUser = yolcuUsers.FirstOrDefault(y => y.Id == bilet.YolcuID);
                    if (yolcuUser != null)
                    {
                        yolcuBilgileri[bilet.BiletID] = yolcuUser;
                    }
                }
                ViewBag.YolcuBilgileri = yolcuBilgileri;

                // Filtreleme
                if (!string.IsNullOrEmpty(searchBiletNo))
                {
                    biletler = biletler.Where(b => 
                        b.BiletNo.ToLower().Contains(searchBiletNo.ToLower()) ||
                        b.BiletID.ToString().Contains(searchBiletNo)).ToList();
                    ViewBag.SearchBiletNo = searchBiletNo;
                }

                if (!string.IsNullOrEmpty(searchYolcu))
                {
                    biletler = biletler.Where(b => 
                    {
                        if (yolcuBilgileri.TryGetValue(b.BiletID, out var yolcu))
                        {
                            return yolcu.Ad.ToLower().Contains(searchYolcu.ToLower()) ||
                                   yolcu.Soyad.ToLower().Contains(searchYolcu.ToLower()) ||
                                   yolcu.TCNo.Contains(searchYolcu);
                        }
                        return false;
                    }).ToList();
                    ViewBag.SearchYolcu = searchYolcu;
                }

                if (!string.IsNullOrEmpty(searchSefer))
                {
                    biletler = biletler.Where(b => 
                        b.Sefer != null && (
                        b.Sefer.SeferID.ToString().Contains(searchSefer) ||
                        b.Sefer.Guzergah != null && (
                        b.Sefer.Guzergah.Nereden.ToLower().Contains(searchSefer.ToLower()) ||
                        b.Sefer.Guzergah.Nereye.ToLower().Contains(searchSefer.ToLower())))).ToList();
                    ViewBag.SearchSefer = searchSefer;
                }

                // Durum filtresi
                if (!string.IsNullOrEmpty(filterDurum))
                {
                    biletler = biletler.Where(b => b.BiletDurumu == filterDurum).ToList();
                    ViewBag.FilterDurum = filterDurum;
                }

                // Tarih filtresi
                if (filterTarih.HasValue)
                {
                    biletler = biletler.Where(b => 
                        b.Sefer != null && b.Sefer.Tarih.Date == filterTarih.Value.Date).ToList();
                    ViewBag.FilterTarih = filterTarih.Value.ToString("yyyy-MM-dd");
                }

                // Güzergah filtresi
                if (filterGuzergah.HasValue)
                {
                    biletler = biletler.Where(b => 
                        b.Sefer != null && b.Sefer.GuzergahID == filterGuzergah.Value).ToList();
                    ViewBag.FilterGuzergah = filterGuzergah.Value;
                }

                // Dropdown'lar için veriler
                ViewBag.Guzergahlar = await _context.Guzergahlar.ToListAsync();
                ViewBag.BiletDurumlari = new List<string> { "Aktif", "İptal", "Kullanıldı", "Beklemede" };

                return View(biletler);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Biletler yüklenirken hata oluştu: " + ex.Message;
                return View(new List<Bilet>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var bilet = await _context.Biletler
                .Include(b => b.Sefer)
                    .ThenInclude(s => s.Guzergah)
                .Include(b => b.Sefer)
                    .ThenInclude(s => s.Otobus)
                .Include(b => b.Koltuk)
                .Include(b => b.Odeme)
                .FirstOrDefaultAsync(b => b.BiletID == id);
            
            if (bilet == null)
            {
                return NotFound();
            }

            // YolcuUser bilgisini manuel olarak yükle
            var yolcu = await _context.Users.FindAsync(bilet.YolcuID);
            ViewBag.YolcuBilgisi = yolcu;

            return View(bilet);
        }

        public async Task<IActionResult> Create()
        {
            // Dropdown'lar için veriler
            ViewBag.Seferler = await _context.Seferler
                .Include(s => s.Guzergah)
                .Include(s => s.Otobus)
                .Where(s => s.Tarih >= DateTime.Today)
                .OrderBy(s => s.Tarih)
                .ThenBy(s => s.Saat)
                .ToListAsync();

            ViewBag.Yolcular = await _context.Users.ToListAsync();
            ViewBag.BiletDurumlari = new List<string> { "Aktif", "Beklemede" };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bilet bilet)
        {
            try
            {
                var seferID = Request.Form["SeferID"].ToString();
                var yolcuID = Request.Form["YolcuID"].ToString();
                var koltukID = Request.Form["KoltukID"].ToString();
                var biletDurumu = Request.Form["BiletDurumu"].ToString();

                if (string.IsNullOrEmpty(seferID) || string.IsNullOrEmpty(yolcuID) || 
                    string.IsNullOrEmpty(koltukID))
                {
                    ModelState.AddModelError("", "Sefer, yolcu ve koltuk seçimi zorunludur.");
                    ViewBag.Seferler = await _context.Seferler.Include(s => s.Guzergah).Include(s => s.Otobus).ToListAsync();
                    ViewBag.Yolcular = await _context.Users.ToListAsync();
                    ViewBag.BiletDurumlari = new List<string> { "Aktif", "Beklemede" };
                    return View(bilet);
                }

                if (!int.TryParse(seferID, out int parsedSeferID) ||
                    !int.TryParse(yolcuID, out int parsedYolcuID) ||
                    !int.TryParse(koltukID, out int parsedKoltukID))
                {
                    ModelState.AddModelError("", "Geçersiz veri formatı.");
                    ViewBag.Seferler = await _context.Seferler.Include(s => s.Guzergah).Include(s => s.Otobus).ToListAsync();
                    ViewBag.Yolcular = await _context.Users.ToListAsync();
                    ViewBag.BiletDurumlari = new List<string> { "Aktif", "Beklemede" };
                    return View(bilet);
                }

                // Sefer kontrolü
                var selectedSefer = await _context.Seferler.FindAsync(parsedSeferID);
                if (selectedSefer == null)
                {
                    ModelState.AddModelError("", "Seçilen sefer bulunamadı.");
                    ViewBag.Seferler = await _context.Seferler.Include(s => s.Guzergah).Include(s => s.Otobus).ToListAsync();
                    ViewBag.Yolcular = await _context.Users.ToListAsync();
                    ViewBag.BiletDurumlari = new List<string> { "Aktif", "Beklemede" };
                    return View(bilet);
                }

                // Koltuk müsaitlik kontrolü
                var existingBilet = await _context.Biletler
                    .Where(b => b.SeferID == parsedSeferID && b.KoltukID == parsedKoltukID && 
                               b.BiletDurumu == "Aktif")
                    .FirstOrDefaultAsync();

                if (existingBilet != null)
                {
                    ModelState.AddModelError("", "Bu koltuk için zaten aktif bilet bulunuyor.");
                    ViewBag.Seferler = await _context.Seferler.Include(s => s.Guzergah).Include(s => s.Otobus).ToListAsync();
                    ViewBag.Yolcular = await _context.Users.ToListAsync();
                    ViewBag.BiletDurumlari = new List<string> { "Aktif", "Beklemede" };
                    return View(bilet);
                }

                var yeniBilet = new Bilet
                {
                    SeferID = parsedSeferID,
                    YolcuID = parsedYolcuID,
                    KoltukID = parsedKoltukID,
                    BiletDurumu = biletDurumu,
                    BiletTarihi = DateTime.Now
                };

                await _biletRepository.AddAsync(yeniBilet);
                TempData["SuccessMessage"] = "Bilet başarıyla oluşturuldu.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata: {ex.Message}");
            }

            ViewBag.Seferler = await _context.Seferler.Include(s => s.Guzergah).Include(s => s.Otobus).ToListAsync();
            ViewBag.Yolcular = await _context.Users.ToListAsync();
            ViewBag.BiletDurumlari = new List<string> { "Aktif", "Beklemede" };
            return View(bilet);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var bilet = await _context.Biletler
                .Include(b => b.Yolcu)
                .Include(b => b.Sefer)
                    .ThenInclude(s => s.Guzergah)
                .Include(b => b.Sefer)
                    .ThenInclude(s => s.Otobus)
                .Include(b => b.Koltuk)
                .FirstOrDefaultAsync(b => b.BiletID == id);

            if (bilet == null)
            {
                return NotFound();
            }

            ViewBag.BiletDurumlari = new List<string> { "Aktif", "İptal", "Kullanıldı", "Beklemede" };
            return View(bilet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Bilet bilet)
        {
            try
            {
                var biletDurumu = Request.Form["BiletDurumu"].ToString();

                if (string.IsNullOrEmpty(biletDurumu))
                {
                    ModelState.AddModelError("", "Bilet durumu zorunludur.");
                    ViewBag.BiletDurumlari = new List<string> { "Aktif", "İptal", "Kullanıldı", "Beklemede" };
                    var biletForError = await _context.Biletler.Include(b => b.Yolcu).Include(b => b.Sefer).ThenInclude(s => s.Guzergah).FirstOrDefaultAsync(b => b.BiletID == id);
                    return View(biletForError ?? bilet);
                }

                var biletToUpdate = await _biletRepository.GetByIdAsync(id);
                if (biletToUpdate == null)
                {
                    TempData["ErrorMessage"] = "Bilet bulunamadı.";
                    return RedirectToAction("Index");
                }

                biletToUpdate.BiletDurumu = biletDurumu;

                await _biletRepository.UpdateAsync(biletToUpdate);
                TempData["SuccessMessage"] = "Bilet başarıyla güncellendi.";
                return RedirectToAction("Details", new { id = id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata: {ex.Message}");
            }

            ViewBag.BiletDurumlari = new List<string> { "Aktif", "İptal", "Kullanıldı", "Beklemede" };
            var biletForView = await _context.Biletler.Include(b => b.Yolcu).Include(b => b.Sefer).ThenInclude(s => s.Guzergah).FirstOrDefaultAsync(b => b.BiletID == id);
            return View(biletForView ?? bilet);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var bilet = await _context.Biletler
                .Include(b => b.Yolcu)
                .Include(b => b.Sefer)
                    .ThenInclude(s => s.Guzergah)
                .Include(b => b.Sefer)
                    .ThenInclude(s => s.Otobus)
                .Include(b => b.Koltuk)
                .FirstOrDefaultAsync(b => b.BiletID == id);

            if (bilet == null)
            {
                return NotFound();
            }
            return View(bilet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var bilet = await _biletRepository.GetByIdAsync(id);
                if (bilet == null)
                {
                    TempData["ErrorMessage"] = "Bilet bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Bilet zaten iptal mi kontrol et
                if (bilet.BiletDurumu == "İptal")
                {
                    TempData["ErrorMessage"] = "Bilet zaten iptal edilmiş.";
                    return RedirectToAction("Index");
                }

                // Bileti iptal et (silme, sadece durumu değiştir)
                bilet.BiletDurumu = "İptal";
                await _biletRepository.UpdateAsync(bilet);

                TempData["SuccessMessage"] = $"Bilet #{bilet.BiletNo} başarıyla iptal edildi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Bilet iptal edilirken hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // Gerçek silme işlemi için ayrı method
        public async Task<IActionResult> HardDelete(int id)
        {
            var bilet = await _context.Biletler
                .Include(b => b.Yolcu)
                .Include(b => b.Sefer)
                    .ThenInclude(s => s.Guzergah)
                .Include(b => b.Sefer)
                    .ThenInclude(s => s.Otobus)
                .Include(b => b.Koltuk)
                .FirstOrDefaultAsync(b => b.BiletID == id);

            if (bilet == null)
            {
                return NotFound();
            }
            return View(bilet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDeleteConfirmed(int id)
        {
            try
            {
                var bilet = await _biletRepository.GetByIdAsync(id);
                if (bilet == null)
                {
                    TempData["ErrorMessage"] = "Bilet bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Sadece iptal edilmiş biletler kalıcı olarak silinebilir
                if (bilet.BiletDurumu != "İptal")
                {
                    TempData["ErrorMessage"] = "Sadece iptal edilmiş biletler kalıcı olarak silinebilir.";
                    return RedirectToAction("Index");
                }

                var deleted = await _biletRepository.DeleteAsync(id);
                if (deleted)
                {
                    TempData["SuccessMessage"] = $"Bilet #{bilet.BiletNo} kalıcı olarak silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Bilet silinemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Bilet silinirken hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var bilet = await _biletRepository.GetByIdAsync(id);
                if (bilet == null)
                {
                    return Json(new { success = false, message = "Bilet bulunamadı." });
                }

                bilet.BiletDurumu = status;
                await _biletRepository.UpdateAsync(bilet);

                return Json(new { success = true, message = "Bilet durumu güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSeats(int seferID)
        {
            try
            {
                var sefer = await _context.Seferler
                    .Include(s => s.Otobus)
                    .FirstOrDefaultAsync(s => s.SeferID == seferID);

                if (sefer == null)
                {
                    return Json(new { success = false, message = "Sefer bulunamadı." });
                }

                var tumKoltuklar = await _context.Koltuklar
                    .Where(k => k.OtobusID == sefer.OtobusID)
                    .ToListAsync();

                var doluKoltuklar = await _context.Biletler
                    .Where(b => b.SeferID == seferID && b.BiletDurumu == "Aktif")
                    .Select(b => b.KoltukID)
                    .ToListAsync();

                var bosKoltuklar = tumKoltuklar
                    .Where(k => !doluKoltuklar.Contains(k.KoltukID))
                    .Select(k => new { 
                        KoltukID = k.KoltukID, 
                        KoltukNo = k.KoltukNo 
                    })
                    .ToList();
                
                // Debug bilgilerini ekle
                var debugInfo = new {
                    SeferID = seferID,
                    OtobusID = sefer.OtobusID,
                    OtobusPlaka = sefer.Otobus?.Plaka,
                    TumKoltukSayisi = tumKoltuklar.Count,
                    DoluKoltukSayisi = doluKoltuklar.Count,
                    BosKoltukSayisi = bosKoltuklar.Count,
                    TumKoltuklar = tumKoltuklar.Select(k => new { k.KoltukID, k.OtobusID, k.KoltukNo }).ToList()
                };

                return Json(new { 
                    success = true, 
                    seats = bosKoltuklar,
                    debug = debugInfo
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    success = false, 
                    message = $"Hata: {ex.Message}",
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
} 
 