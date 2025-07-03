using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OtobusBiletSistemi.Core.Data;
using OtobusBiletSistemi.Web.Models;
using OtobusBiletSistemi.Web.Extensions;

namespace OtobusBiletSistemi.Web.Controllers
{
    public class KoltukController : Controller
    {
        private readonly AppDbContext _context;

        public KoltukController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Secim(int id, int yolcuSayisi = 1)
        {
            try
            {
                // Sefer bilgilerini al
                var sefer = await _context.Seferler
                    .Include(s => s.Otobus)
                    .Include(s => s.Guzergah)
                    .FirstOrDefaultAsync(s => s.SeferID == id);

                if (sefer == null)
                {
                    TempData["Error"] = "Sefer bulunamadı.";
                    return RedirectToAction("Index", "Home");
                }

                // Otobüsün tüm koltukları
                var tumKoltuklar = await _context.Koltuklar
                    .Where(k => k.OtobusID == sefer.OtobusID)
                    .OrderBy(k => k.KoltukNo)
                    .ToListAsync();

                // Bu sefer için satılan biletlerdeki dolu koltuk ID'leri
                var doluKoltukIdleri = await _context.Biletler
                    .Where(b => b.SeferID == id && b.BiletDurumu != "İptal")
                    .Select(b => b.KoltukID)
                    .ToHashSetAsync();

                // ViewModel oluştur
                var model = new KoltukSecimViewModel
                {
                    Sefer = sefer,
                    Otobus = sefer.Otobus,
                    Guzergah = sefer.Guzergah,
                    YolcuSayisi = yolcuSayisi,
                    TumKoltuklar = tumKoltuklar,
                    DoluKoltukIdleri = doluKoltukIdleri,
                    BiletFiyati = sefer.Fiyat
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Koltuk seçimi yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> KoltukOnayla(int seferId, string seciliKoltuklar)
        {
            try
            {
                if (string.IsNullOrEmpty(seciliKoltuklar))
                {
                    TempData["Error"] = "Lütfen en az bir koltuk seçin.";
                    return RedirectToAction("Secim", new { id = seferId });
                }

                // Seçili koltuk ID'lerini parse et
                var koltukIdleri = seciliKoltuklar.Split(',')
                    .Where(x => int.TryParse(x, out _))
                    .Select(x => int.Parse(x))
                    .ToList();

                if (!koltukIdleri.Any())
                {
                    TempData["Error"] = "Geçersiz koltuk seçimi.";
                    return RedirectToAction("Secim", new { id = seferId });
                }

                // Sefer ve koltuk bilgilerini doğrula
                var sefer = await _context.Seferler
                    .Include(s => s.Otobus)
                    .Include(s => s.Guzergah)
                    .FirstOrDefaultAsync(s => s.SeferID == seferId);

                if (sefer == null)
                {
                    TempData["Error"] = "Sefer bulunamadı.";
                    return RedirectToAction("Index", "Home");
                }

                // Seçili koltukları doğrula
                var seciliKoltukListesi = await _context.Koltuklar
                    .Where(k => koltukIdleri.Contains(k.KoltukID) && k.OtobusID == sefer.OtobusID)
                    .ToListAsync();

                if (seciliKoltukListesi.Count != koltukIdleri.Count)
                {
                    TempData["Error"] = "Geçersiz koltuk seçimi.";
                    return RedirectToAction("Secim", new { id = seferId });
                }

                // Koltukların müsait olduğunu kontrol et
                var doluKoltuklar = await _context.Biletler
                    .Where(b => b.SeferID == seferId && 
                               koltukIdleri.Contains(b.KoltukID) && 
                               b.BiletDurumu != "İptal")
                    .ToListAsync();

                if (doluKoltuklar.Any())
                {
                    TempData["Error"] = "Seçtiğiniz koltukların bazıları artık müsait değil. Lütfen tekrar seçim yapın.";
                    return RedirectToAction("Secim", new { id = seferId });
                }

                // Session'a seçimi kaydet (ödeme sayfasında kullanmak için)
                HttpContext.Session.SetInt32("SeferId", seferId);
                HttpContext.Session.SetObject("SeciliKoltuklar", koltukIdleri);

                // Ödeme sayfasına yönlendir (önce yolcu bilgileri)
                return RedirectToAction("YolcuBilgileri", "Bilet");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Koltuk onaylama sırasında bir hata oluştu: " + ex.Message;
                return RedirectToAction("Secim", new { id = seferId });
            }
        }
    }
}
