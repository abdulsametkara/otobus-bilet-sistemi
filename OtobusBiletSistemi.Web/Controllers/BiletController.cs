using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OtobusBiletSistemi.Web.Models;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Entities;
using Microsoft.AspNetCore.Identity;
using OtobusBiletSistemi.Core.Data;

namespace OtobusBiletSistemi.Web.Controllers
{
    [Authorize]
    public class BiletController : Controller
    {
        private readonly IRepository<Sefer> _seferRepository;
        private readonly IRepository<Koltuk> _koltukRepository;
        private readonly IRepository<Otobus> _otobusRepository;
        private readonly IRepository<Guzergah> _guzergahRepository;
        private readonly IRepository<Bilet> _biletRepository;
        private readonly IRepository<Odeme> _odemeRepository;
        private readonly UserManager<YolcuUser> _userManager;

        public BiletController(
            IRepository<Sefer> seferRepository,
            IRepository<Koltuk> koltukRepository,
            IRepository<Otobus> otobusRepository,
            IRepository<Guzergah> guzergahRepository,
            IRepository<Bilet> biletRepository,
            IRepository<Odeme> odemeRepository,
            UserManager<YolcuUser> userManager)
        {
            _seferRepository = seferRepository;
            _koltukRepository = koltukRepository;
            _otobusRepository = otobusRepository;
            _guzergahRepository = guzergahRepository;
            _biletRepository = biletRepository;
            _odemeRepository = odemeRepository;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> YolcuBilgileri()
        {
            // ModelState'i temizle
            ModelState.Clear();
            // Session'dan seçili koltukları al
            var seciliKoltuklar = HttpContext.Session.GetObject<List<int>>("SeciliKoltuklar");
            var seferId = HttpContext.Session.GetInt32("SeferId");

            if (seciliKoltuklar == null || !seciliKoltuklar.Any() || seferId == null)
            {
                TempData["Error"] = "Koltuk seçimi bulunamadı. Lütfen tekrar koltuk seçimi yapın.";
                return RedirectToAction("Index", "Home");
            }

            // Sefer bilgilerini getir
            var sefer = await _seferRepository.GetByIdAsync(seferId.Value);
            if (sefer == null)
            {
                TempData["Error"] = "Sefer bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            // Koltuk bilgilerini getir
            var koltuklar = new List<Koltuk>();
            foreach (var koltukId in seciliKoltuklar)
            {
                var koltuk = await _koltukRepository.GetByIdAsync(koltukId);
                if (koltuk != null)
                {
                    koltuklar.Add(koltuk);
                }
            }

            // Otobüs ve güzergah bilgilerini getir
            var otobus = await _otobusRepository.GetByIdAsync(sefer.OtobusID);
            var guzergah = await _guzergahRepository.GetByIdAsync(sefer.GuzergahID);

            // Sefer tablosundan güncel fiyat bilgisini al
            var biletFiyati = sefer.Fiyat;
            
            var model = new YolcuBilgileriViewModel
            {
                Sefer = sefer,
                Otobus = otobus,
                Guzergah = guzergah,
                Koltuklar = koltuklar,
                YolcuBilgileri = seciliKoltuklar.Select(koltukId => new YolcuBilgisiModel
                {
                    KoltukID = koltukId,
                    KoltukNo = koltuklar.FirstOrDefault(k => k.KoltukID == koltukId)?.KoltukNo ?? ""
                }).ToList(),
                BiletFiyati = biletFiyati
            };

            // Fiyat bilgisini session'a kaydet
            HttpContext.Session.SetString("BiletFiyati", biletFiyati.ToString());

            return View(model);
        }

        [HttpPost]
        public IActionResult YolcuBilgileri(YolcuBilgileriViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Hata durumunda tüm bilgileri session'dan tekrar yükle
                return RedirectToAction("YolcuBilgileri");
            }

            // Yolcu bilgilerini session'a kaydet
            HttpContext.Session.SetObject("YolcuBilgileri", model.YolcuBilgileri);

            // Ödeme sayfasına yönlendir
            return RedirectToAction("Odeme");
        }

        [HttpGet]
        public async Task<IActionResult> Odeme()
        {
            // Session'dan gerekli bilgileri al
            var seciliKoltuklar = HttpContext.Session.GetObject<List<int>>("SeciliKoltuklar");
            var seferId = HttpContext.Session.GetInt32("SeferId");
            var yolcuBilgileri = HttpContext.Session.GetObject<List<YolcuBilgisiModel>>("YolcuBilgileri");
            var biletFiyatiStr = HttpContext.Session.GetString("BiletFiyati");

            if (seciliKoltuklar == null || !seciliKoltuklar.Any() || 
                seferId == null || yolcuBilgileri == null || !yolcuBilgileri.Any())
            {
                TempData["Error"] = "Rezervasyon bilgileri bulunamadı. Lütfen tekrar rezervasyon yapın.";
                return RedirectToAction("Index", "Home");
            }

            // Sefer bilgilerini getir
            var sefer = await _seferRepository.GetByIdAsync(seferId.Value);
            if (sefer == null)
            {
                TempData["Error"] = "Sefer bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            // Koltuk bilgilerini getir
            var koltuklar = new List<Koltuk>();
            foreach (var koltukId in seciliKoltuklar)
            {
                var koltuk = await _koltukRepository.GetByIdAsync(koltukId);
                if (koltuk != null)
                {
                    koltuklar.Add(koltuk);
                }
            }

            // Otobüs ve güzergah bilgilerini getir
            var otobus = await _otobusRepository.GetByIdAsync(sefer.OtobusID);
            var guzergah = await _guzergahRepository.GetByIdAsync(sefer.GuzergahID);

            // Bilet fiyatını sefer tablosundan al (session'a öncelik ver ama sefer fiyatını fallback olarak kullan)
            decimal biletFiyati = sefer.Fiyat; // Sefer tablosundan güncel fiyat
            if (decimal.TryParse(biletFiyatiStr, out decimal sessionFiyat))
            {
                biletFiyati = sessionFiyat; // Session'daki fiyat varsa onu kullan
            }

            // İletişim bilgilerini yolcu bilgilerinden al (ilk yolcudan)
            var ilkYolcu = yolcuBilgileri.First();

            var model = new OdemeViewModel
            {
                Sefer = sefer,
                Otobus = otobus,
                Guzergah = guzergah,
                Koltuklar = koltuklar,
                YolcuBilgileri = yolcuBilgileri,
                BiletFiyati = biletFiyati,
                Telefon = ilkYolcu.Telefon,
                Email = ilkYolcu.Email
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Odeme(OdemeViewModel model)
        {
            // Session'dan ortak bilgileri al
            var seciliKoltuklar = HttpContext.Session.GetObject<List<int>>("SeciliKoltuklar");
            var seferId = HttpContext.Session.GetInt32("SeferId");
            var yolcuBilgileri = HttpContext.Session.GetObject<List<YolcuBilgisiModel>>("YolcuBilgileri");
            var biletFiyatiStr = HttpContext.Session.GetString("BiletFiyati");
            
            // Sefer bilgisini al ve güncel fiyatı kullan
            var sefer = await _seferRepository.GetByIdAsync(seferId.Value);
            decimal biletFiyati = sefer?.Fiyat ?? 120m; // Sefer tablosundan fiyat, yoksa default
            if (decimal.TryParse(biletFiyatiStr, out decimal sessionFiyat))
            {
                biletFiyati = sessionFiyat; // Session'daki fiyat varsa onu kullan
            }
            decimal toplamTutar = (yolcuBilgileri?.Count ?? 1) * biletFiyati;

            // Kart numarası validasyonu
            var cleanCardNumber = model.CleanKartNumarasi;
            if (string.IsNullOrEmpty(cleanCardNumber) || cleanCardNumber.Length != 16 || !cleanCardNumber.All(char.IsDigit))
            {
                ModelState.AddModelError("KartNumarasi", "Kart numarası 16 haneli olmalıdır");
            }

            // Test amaçlı başarısız ödeme simülasyonu
            if (cleanCardNumber == "4444444444444444")
            {
                var seferForError = seferId.HasValue ? await _seferRepository.GetByIdAsync(seferId.Value) : null;
                var guzergah = seferForError != null ? await _guzergahRepository.GetByIdAsync(seferForError.GuzergahID) : null;
                
                TempData["HataMesaji"] = "Kartınızda yeterli bakiye bulunmamaktadır.";
                TempData["SeferBilgisi"] = guzergah != null ? $"{guzergah.Nereden} - {guzergah.Nereye}" : "Bilinmiyor";
                TempData["SeferTarihi"] = seferForError?.Tarih.ToString("dd.MM.yyyy") ?? "Bilinmiyor";
                TempData["ToplamTutar"] = toplamTutar.ToString("N2");

                return RedirectToAction("OdemeBasarisiz");
            }

            if (!ModelState.IsValid)
            {
                // Model validation başarısız, view'ı tekrar yükle
                if (seciliKoltuklar != null && seferId != null && yolcuBilgileri != null)
                {
                    var seferForModel = await _seferRepository.GetByIdAsync(seferId.Value);
                    var otobus = await _otobusRepository.GetByIdAsync(seferForModel?.OtobusID ?? 0);
                    var guzergah = await _guzergahRepository.GetByIdAsync(seferForModel?.GuzergahID ?? 0);
                    
                    var koltuklar = new List<Koltuk>();
                    foreach (var koltukId in seciliKoltuklar)
                    {
                        var koltuk = await _koltukRepository.GetByIdAsync(koltukId);
                        if (koltuk != null) koltuklar.Add(koltuk);
                    }

                    model.Sefer = seferForModel;
                    model.Otobus = otobus;
                    model.Guzergah = guzergah;
                    model.Koltuklar = koltuklar;
                    model.YolcuBilgileri = yolcuBilgileri;
                    model.BiletFiyati = biletFiyati;
                }
                
                return View(model);
            }

            try
            {
                if (seciliKoltuklar == null || seferId == null || yolcuBilgileri == null)
                {
                    TempData["Error"] = "Rezervasyon bilgileri kayboldu. Lütfen tekrar rezervasyon yapın.";
                    return RedirectToAction("Index", "Home");
                }

                // Kullanıcı bilgisini al
                var user = await _userManager.GetUserAsync(User);
                var yolcuId = user?.Id ?? 1; // Fallback to guest user

                // Fiyat bilgisini session'dan al ve hesapla (tekrar hesapla güncel verilerle)
                toplamTutar = yolcuBilgileri.Count * biletFiyati;

                // Debug: Console'a bilgileri yazdır
                Console.WriteLine($"Ödeme işlemi başlıyor - YolcuID: {yolcuId}, Tutar: {toplamTutar}, Bilet Fiyatı: {biletFiyati}, Yolcu Sayısı: {yolcuBilgileri.Count}");

                // Önce ödeme kaydı oluştur
                var odeme = new Odeme
                {
                    YolcuID = yolcuId,
                    OdemeTarihi = DateTime.Now,
                    OdemeTutari = toplamTutar, // Hesaplanan tutarı kullan
                    OdemeYontemi = "Kredi Kartı",
                    OdemeDurumu = "Tamamlandı",
                    KartSahibiAdi = model.KartSahibiAdi ?? "Demo Kullanıcı",
                    KartNumarasi = !string.IsNullOrEmpty(cleanCardNumber) && cleanCardNumber.Length >= 4 
                        ? "****" + cleanCardNumber.Substring(cleanCardNumber.Length - 4) 
                        : "****1234",
                    BiletSayisi = seciliKoltuklar.Count
                };

                Console.WriteLine($"Ödeme nesnesi oluşturuldu: {odeme.OdemeYontemi}, {odeme.OdemeDurumu}");
                await _odemeRepository.AddAsync(odeme);
                Console.WriteLine($"Ödeme veritabanına eklendi - OdemeID: {odeme.OdemeID}");

                // Her koltuk için bilet oluştur (ödeme ID'si ile)
                var biletler = new List<Bilet>();

                for (int i = 0; i < seciliKoltuklar.Count; i++)
                {
                    var bilet = new Bilet
                    {
                        SeferID = seferId.Value,
                        KoltukID = seciliKoltuklar[i],
                        YolcuID = yolcuId,
                        OdemeID = odeme.OdemeID, // Ödeme ID'sini ekle
                        BiletTarihi = DateTime.Now,
                        BiletDurumu = "Aktif"
                    };

                    await _biletRepository.AddAsync(bilet);
                    biletler.Add(bilet);
                }

                // Session'ı temizle
                HttpContext.Session.Remove("SeciliKoltuklar");
                HttpContext.Session.Remove("SeferId");
                HttpContext.Session.Remove("YolcuBilgileri");
                HttpContext.Session.Remove("BiletFiyati");

                // Başarı sayfasına yönlendir  
                var ilkBilet = biletler.FirstOrDefault();
                
                // Sefer ve güzergah bilgisini al
                var seferForSuccess = await _seferRepository.GetByIdAsync(seferId.Value);
                var guzergah2 = seferForSuccess != null ? await _guzergahRepository.GetByIdAsync(seferForSuccess.GuzergahID) : null;
                
                TempData["BiletNumarasi"] = ilkBilet?.BiletNo ?? "N/A";
                TempData["BiletSayisi"] = biletler.Count.ToString();
                TempData["ToplamTutar"] = toplamTutar.ToString("N2");
                TempData["SeferBilgisi"] = guzergah2 != null ? $"{guzergah2.Nereden} - {guzergah2.Nereye}" : "Bilinmiyor";
                TempData["SeferTarihi"] = seferForSuccess?.Tarih.ToString("dd.MM.yyyy") ?? "Bilinmiyor";
                
                return RedirectToAction("OdemeBasarili");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ödeme hatası: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                // Hata bilgilerini TempData'ya ekle
                var seferForError2 = seferId.HasValue ? await _seferRepository.GetByIdAsync(seferId.Value) : null;
                var guzergah3 = seferForError2 != null ? await _guzergahRepository.GetByIdAsync(seferForError2.GuzergahID) : null;
                
                TempData["HataMesaji"] = "Ödeme işlemi sırasında teknik bir sorun oluştu. Lütfen tekrar deneyiniz.";
                TempData["SeferBilgisi"] = guzergah3 != null ? $"{guzergah3.Nereden} - {guzergah3.Nereye}" : "Bilinmiyor";
                TempData["SeferTarihi"] = seferForError2?.Tarih.ToString("dd.MM.yyyy") ?? "Bilinmiyor";
                TempData["ToplamTutar"] = toplamTutar.ToString("N2");

                return RedirectToAction("OdemeBasarisiz");
            }
        }

        [HttpGet]
        public IActionResult OdemeBasarili()
        {
            // TempData'dan bilgileri al
            ViewBag.BiletNumarasi = TempData["BiletNumarasi"];
            ViewBag.BiletSayisi = TempData["BiletSayisi"];
            ViewBag.ToplamTutar = TempData["ToplamTutar"];
            ViewBag.SeferBilgisi = TempData["SeferBilgisi"];
            ViewBag.SeferTarihi = TempData["SeferTarihi"];

            if (ViewBag.BiletNumarasi == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpGet]
        public IActionResult OdemeBasarisiz(string hataMesaji = null)
        {
            // TempData'dan hata bilgilerini al
            ViewBag.HataMesaji = hataMesaji ?? TempData["HataMesaji"] ?? "Bilinmeyen bir hata oluştu.";
            ViewBag.SeferBilgisi = TempData["SeferBilgisi"];
            ViewBag.SeferTarihi = TempData["SeferTarihi"];
            ViewBag.ToplamTutar = TempData["ToplamTutar"];

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Biletlerim()
        {
            // Kullanıcı bilgisini al
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Kullanıcının biletlerini getir
            var tumBiletler = await _biletRepository.GetAllAsync();
            var kullaniciBiletleri = tumBiletler.Where(b => b.YolcuID == user.Id).ToList();

            // Her bilet için detay bilgilerini getir
            var biletDetaylari = new List<BiletDetayViewModel>();
            
            foreach (var bilet in kullaniciBiletleri)
            {
                var sefer = await _seferRepository.GetByIdAsync(bilet.SeferID);
                var koltuk = await _koltukRepository.GetByIdAsync(bilet.KoltukID);
                var otobus = await _otobusRepository.GetByIdAsync(sefer?.OtobusID ?? 0);
                var guzergah = await _guzergahRepository.GetByIdAsync(sefer?.GuzergahID ?? 0);

                biletDetaylari.Add(new BiletDetayViewModel
                {
                    Bilet = bilet,
                    Sefer = sefer,
                    Koltuk = koltuk,
                    Otobus = otobus,
                    Guzergah = guzergah
                });
            }

            return View(biletDetaylari);
        }

        [HttpPost]
        public async Task<IActionResult> IptalEt([FromBody] IptalEtRequest request)
        {
            try
            {
                // Kullanıcı bilgisini al
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
                }

                // Bileti getir
                var bilet = await _biletRepository.GetByIdAsync(request.BiletId);
                if (bilet == null)
                {
                    return Json(new { success = false, message = "Bilet bulunamadı." });
                }

                // Bilet kullanıcının mı kontrol et
                if (bilet.YolcuID != user.Id)
                {
                    return Json(new { success = false, message = "Bu bilet size ait değil." });
                }

                // Bilet zaten iptal mi kontrol et
                if (bilet.BiletDurumu == "İptal")
                {
                    return Json(new { success = false, message = "Bu bilet zaten iptal edilmiş." });
                }

                // Bileti iptal et
                bilet.BiletDurumu = "İptal";
                await _biletRepository.UpdateAsync(bilet);

                return Json(new { success = true, message = "Bilet başarıyla iptal edildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Sil([FromBody] SilRequest request)
        {
            try
            {
                // Kullanıcı bilgisini al
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });
                }

                // Bileti getir
                var bilet = await _biletRepository.GetByIdAsync(request.BiletId);
                if (bilet == null)
                {
                    return Json(new { success = false, message = "Bilet bulunamadı." });
                }

                // Bilet kullanıcının mı kontrol et
                if (bilet.YolcuID != user.Id)
                {
                    return Json(new { success = false, message = "Bu bilet size ait değil." });
                }

                // Sadece iptal edilmiş biletler silinebilir
                if (bilet.BiletDurumu != "İptal")
                {
                    return Json(new { success = false, message = "Sadece iptal edilmiş biletler silinebilir." });
                }

                // Bileti kalıcı olarak sil
                await _biletRepository.DeleteAsync(bilet.BiletID);

                return Json(new { success = true, message = "Bilet başarıyla silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu: " + ex.Message });
            }
        }
    }

    public class IptalEtRequest
    {
        public int BiletId { get; set; }
    }

    public class SilRequest
    {
        public int BiletId { get; set; }
    }
}

// Session helper extensions
public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, System.Text.Json.JsonSerializer.Serialize(value));
    }

    public static T? GetObject<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default(T) : System.Text.Json.JsonSerializer.Deserialize<T>(value);
    }
} 