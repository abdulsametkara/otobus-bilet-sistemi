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
    public class GuzergahController : Controller
    {
        private readonly IRepository<Guzergah> _guzergahRepository;
        private readonly AppDbContext _context;

        public GuzergahController(IRepository<Guzergah> guzergahRepository, AppDbContext context)
        {
            _guzergahRepository = guzergahRepository;
            _context = context;
        }

        // Türkiye şehirleri listesi
        private List<string> GetTurkishCities()
        {
            return new List<string>
            {
                "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya", "Ankara", "Antalya", "Artvin",
                "Aydın", "Balıkesir", "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa",
                "Çanakkale", "Çankırı", "Çorum", "Denizli", "Diyarbakır", "Edirne", "Elazığ", "Erzincan",
                "Erzurum", "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkari", "Hatay", "Isparta",
                "Mersin", "İstanbul", "İzmir", "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir",
                "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Kahramanmaraş", "Mardin", "Muğla",
                "Muş", "Nevşehir", "Niğde", "Ordu", "Rize", "Sakarya", "Samsun", "Siirt", "Sinop",
                "Sivas", "Tekirdağ", "Tokat", "Trabzon", "Tunceli", "Şanlıurfa", "Uşak", "Van",
                "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman", "Kırıkkale", "Batman", "Şırnak",
                "Bartın", "Ardahan", "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce"
            };
        }

        public async Task<IActionResult> Index(string searchNereden, string searchNereye)
        {
            var guzergahlar = await _guzergahRepository.GetAllAsync();
            
            if (!string.IsNullOrEmpty(searchNereden))
            {
                guzergahlar = guzergahlar.Where(g => g.Nereden.ToLower().Contains(searchNereden.ToLower())).ToList();
                ViewBag.SearchNereden = searchNereden;
            }
            
            if (!string.IsNullOrEmpty(searchNereye))
            {
                guzergahlar = guzergahlar.Where(g => g.Nereye.ToLower().Contains(searchNereye.ToLower())).ToList();
                ViewBag.SearchNereye = searchNereye;
            }
            
            return View(guzergahlar);
        }

        public IActionResult Create()
        {
            ViewBag.Cities = GetTurkishCities();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guzergah guzergah)
        {
            try
            {
                var nereden = Request.Form["Nereden"].ToString();
                var nereye = Request.Form["Nereye"].ToString();
                var mesafeStr = Request.Form["Mesafe"].ToString();

                if (string.IsNullOrEmpty(nereden) || string.IsNullOrEmpty(nereye) || 
                    string.IsNullOrEmpty(mesafeStr))
                {
                    ModelState.AddModelError("", "Tüm alanlar zorunludur.");
                    ViewBag.Cities = GetTurkishCities();
                    return View(guzergah);
                }

                if (nereden == nereye)
                {
                    ModelState.AddModelError("", "Kalkış ve varış şehri aynı olamaz.");
                    ViewBag.Cities = GetTurkishCities();
                    return View(guzergah);
                }

                if (!int.TryParse(mesafeStr, out int mesafe) || mesafe < 1 || mesafe > 2000)
                {
                    ModelState.AddModelError("", "Mesafe 1-2000 km arasında olmalıdır.");
                    ViewBag.Cities = GetTurkishCities();
                    return View(guzergah);
                }

                // Aynı güzergah zaten var mı kontrol et
                var existingGuzergah = await _guzergahRepository.GetAllAsync();
                var duplicate = existingGuzergah.FirstOrDefault(g => 
                    g.Nereden.Equals(nereden, StringComparison.OrdinalIgnoreCase) &&
                    g.Nereye.Equals(nereye, StringComparison.OrdinalIgnoreCase));

                if (duplicate != null)
                {
                    ModelState.AddModelError("", "Bu güzergah zaten mevcut.");
                    ViewBag.Cities = GetTurkishCities();
                    return View(guzergah);
                }

                var yeniGuzergah = new Guzergah
                {
                    Nereden = nereden,
                    Nereye = nereye,
                    Mesafe = mesafe
                };

                await _guzergahRepository.AddAsync(yeniGuzergah);
                TempData["SuccessMessage"] = "Güzergah başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata: {ex.Message}");
            }

            ViewBag.Cities = GetTurkishCities();
            return View(guzergah);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var guzergah = await _guzergahRepository.GetByIdAsync(id);
            if (guzergah == null)
            {
                return NotFound();
            }
            ViewBag.Cities = GetTurkishCities();
            return View(guzergah);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Guzergah guzergah)
        {
            try
            {
                var nereden = Request.Form["Nereden"].ToString();
                var nereye = Request.Form["Nereye"].ToString();
                var mesafeStr = Request.Form["Mesafe"].ToString();

                if (string.IsNullOrEmpty(nereden) || string.IsNullOrEmpty(nereye) || 
                    string.IsNullOrEmpty(mesafeStr))
                {
                    ModelState.AddModelError("", "Tüm alanlar zorunludur.");
                    ViewBag.Cities = GetTurkishCities();
                    var guzergahForError = await _guzergahRepository.GetByIdAsync(id);
                    return View(guzergahForError ?? guzergah);
                }

                if (nereden == nereye)
                {
                    ModelState.AddModelError("", "Kalkış ve varış şehri aynı olamaz.");
                    ViewBag.Cities = GetTurkishCities();
                    var guzergahForError = await _guzergahRepository.GetByIdAsync(id);
                    return View(guzergahForError ?? guzergah);
                }

                if (!int.TryParse(mesafeStr, out int mesafe) || mesafe < 1 || mesafe > 2000)
                {
                    ModelState.AddModelError("", "Mesafe 1-2000 km arasında olmalıdır.");
                    ViewBag.Cities = GetTurkishCities();
                    var guzergahForError = await _guzergahRepository.GetByIdAsync(id);
                    return View(guzergahForError ?? guzergah);
                }

                var guzergahToUpdate = await _guzergahRepository.GetByIdAsync(id);
                if (guzergahToUpdate == null)
                {
                    TempData["ErrorMessage"] = "Güzergah bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Düzenleme sırasında aynı güzergah kontrolü (kendisi hariç)
                var existingGuzergahlar = await _guzergahRepository.GetAllAsync();
                var duplicate = existingGuzergahlar.FirstOrDefault(g => 
                    g.GuzergahID != id &&
                    g.Nereden.Equals(nereden, StringComparison.OrdinalIgnoreCase) &&
                    g.Nereye.Equals(nereye, StringComparison.OrdinalIgnoreCase));

                if (duplicate != null)
                {
                    ModelState.AddModelError("", "Bu güzergah zaten mevcut.");
                    ViewBag.Cities = GetTurkishCities();
                    return View(guzergahToUpdate);
                }

                guzergahToUpdate.Nereden = nereden;
                guzergahToUpdate.Nereye = nereye;
                guzergahToUpdate.Mesafe = mesafe;

                await _guzergahRepository.UpdateAsync(guzergahToUpdate);
                TempData["SuccessMessage"] = "Güzergah başarıyla güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata: {ex.Message}");
            }

            ViewBag.Cities = GetTurkishCities();
            var guzergahForView = await _guzergahRepository.GetByIdAsync(id);
            return View(guzergahForView ?? guzergah);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var guzergah = await _guzergahRepository.GetByIdAsync(id);
            if (guzergah == null)
            {
                return NotFound();
            }
            return View(guzergah);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var guzergah = await _guzergahRepository.GetByIdAsync(id);
                if (guzergah == null)
                {
                    TempData["ErrorMessage"] = "Güzergah bulunamadı.";
                    return RedirectToAction("Index");
                }

                // Bu güzergahın aktif seferleri var mı kontrol et
                var seferler = await _context.Seferler.Where(s => s.GuzergahID == id).CountAsync();
                if (seferler > 0)
                {
                    TempData["ErrorMessage"] = $"{guzergah.Nereden} - {guzergah.Nereye} güzergahının aktif seferleri bulunuyor. Önce seferleri silin.";
                    return RedirectToAction("Index");
                }

                var deleted = await _guzergahRepository.DeleteAsync(id);
                if (deleted)
                {
                    TempData["SuccessMessage"] = $"{guzergah.Nereden} - {guzergah.Nereye} güzergahı başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Güzergah silinemedi.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Güzergah silinirken hata oluştu: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            var guzergah = await _guzergahRepository.GetByIdAsync(id);
            if (guzergah == null)
            {
                return NotFound();
            }

            // Bu güzergaha ait seferleri de getir
            var seferler = await _context.Seferler
                .Include(s => s.Otobus)
                .Where(s => s.GuzergahID == id)
                .OrderBy(s => s.Tarih)
                .ThenBy(s => s.Saat)
                .ToListAsync();

            ViewBag.Seferler = seferler;
            return View(guzergah);
        }
    }
} 