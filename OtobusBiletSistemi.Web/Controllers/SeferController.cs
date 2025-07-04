using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Web.Models;

namespace OtobusBiletSistemi.Web.Controllers
{
    public class SeferController : Controller
    {
        private readonly IRepository<Sefer> _seferRepository;
        private readonly IRepository<Guzergah> _guzergahRepository;
        private readonly IRepository<Otobus> _otobusRepository;
        private readonly IRepository<Koltuk> _koltukRepository;
        private readonly IRepository<Bilet> _biletRepository;
        private readonly ILogger<SeferController> _logger;

        public SeferController(
            IRepository<Sefer> seferRepository,
            IRepository<Guzergah> guzergahRepository,
            IRepository<Otobus> otobusRepository,
            IRepository<Koltuk> koltukRepository,
            IRepository<Bilet> biletRepository,
            ILogger<SeferController> logger)
        {
            _seferRepository = seferRepository;
            _guzergahRepository = guzergahRepository;
            _otobusRepository = otobusRepository;
            _koltukRepository = koltukRepository;
            _biletRepository = biletRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var seferler = await _seferRepository.GetAllAsync();
            return View(seferler);
        }

        public async Task<IActionResult> SeferListesi(string nereden, string nereye, string tarih, int yolcuSayisi = 1)
        {
            try
            {
                _logger.LogInformation("SeferListesi metodu çağrıldı. Parametreler: nereden={Nereden}, nereye={Nereye}, tarih={Tarih}, yolcuSayisi={YolcuSayisi}", 
                    nereden, nereye, tarih, yolcuSayisi);

                // Repository'lerin null olmadığını kontrol et
                if (_seferRepository == null)
                {
                    _logger.LogError("SeferRepository is null!");
                    throw new InvalidOperationException("SeferRepository servisi bulunamadı");
                }

                if (_guzergahRepository == null)
                {
                    _logger.LogError("GuzergahRepository is null!");
                    throw new InvalidOperationException("GuzergahRepository servisi bulunamadı");
                }

                _logger.LogInformation("Repository'ler kontrol edildi, verileri çekiliyor...");

                // Verileri çek
            var seferler = await _seferRepository.GetAllAsync();
                _logger.LogInformation("Toplam {SeferSayisi} sefer bulundu", seferler?.Count() ?? 0);

            var otobusler = await _otobusRepository.GetAllAsync();
                _logger.LogInformation("Toplam {OtobusSayisi} otobüs bulundu", otobusler?.Count() ?? 0);

            var guzergahlar = await _guzergahRepository.GetAllAsync();
                _logger.LogInformation("Toplam {GuzergahSayisi} güzergah bulundu", guzergahlar?.Count() ?? 0);

            var koltuklar = await _koltukRepository.GetAllAsync();
                _logger.LogInformation("Toplam {KoltukSayisi} koltuk bulundu", koltuklar?.Count() ?? 0);

            var biletler = await _biletRepository.GetAllAsync();
                _logger.LogInformation("Toplam {BiletSayisi} bilet bulundu", biletler?.Count() ?? 0);

                // Null kontrolleri
                if (seferler == null)
                {
                    _logger.LogWarning("Seferler null döndü, boş liste oluşturuluyor");
                    seferler = new List<Sefer>();
                }

                if (guzergahlar == null)
                {
                    _logger.LogWarning("Güzergahlar null döndü, boş liste oluşturuluyor");
                    guzergahlar = new List<Guzergah>();
                }

                if (otobusler == null)
                {
                    _logger.LogWarning("Otobüsler null döndü, boş liste oluşturuluyor");
                    otobusler = new List<Otobus>();
                }

                if (koltuklar == null)
                {
                    _logger.LogWarning("Koltuklar null döndü, boş liste oluşturuluyor");
                    koltuklar = new List<Koltuk>();
                }

                if (biletler == null)
                {
                    _logger.LogWarning("Biletler null döndü, boş liste oluşturuluyor");
                    biletler = new List<Bilet>();
                }
            
            // Arama filtresi - Tam eşleşme ile güzergah kontrolü
            if (!string.IsNullOrEmpty(nereden) && !string.IsNullOrEmpty(nereye))
            {
                    _logger.LogInformation("Güzergah filtreleme başlıyor: {Nereden} -> {Nereye}", nereden, nereye);
                    
                // Tam eşleşme ile güzergah bul
                var uygunGuzergah = guzergahlar.FirstOrDefault(g => 
                    g.Nereden.Equals(nereden, StringComparison.OrdinalIgnoreCase) && 
                    g.Nereye.Equals(nereye, StringComparison.OrdinalIgnoreCase));
                
                if (uygunGuzergah != null)
                {
                        _logger.LogInformation("Uygun güzergah bulundu: ID={GuzergahId}", uygunGuzergah.GuzergahID);
                    // Bu güzergaha ait seferleri filtrele
                    seferler = seferler.Where(s => s.GuzergahID == uygunGuzergah.GuzergahID).ToList();
                        _logger.LogInformation("Güzergah filtresinden sonra {SeferSayisi} sefer kaldı", seferler.Count());
                }
                else
                {
                        _logger.LogWarning("Uygun güzergah bulunamadı: {Nereden} -> {Nereye}", nereden, nereye);
                    // Eğer güzergah bulunamazsa, hiç sefer döndürme
                    seferler = new List<Sefer>();
                }
            }

            // Tarih filtresi
            if (!string.IsNullOrEmpty(tarih))
            {
                    _logger.LogInformation("Tarih filtreleme başlıyor: {Tarih}", tarih);
                if (DateTime.TryParse(tarih, out DateTime seferTarihi))
                {
                        var oncekiSayisi = seferler.Count();
                    seferler = seferler.Where(s => s.Tarih.Date == seferTarihi.Date).ToList();
                        _logger.LogInformation("Tarih filtresinden sonra {SeferSayisi} sefer kaldı (önceden {OncekiSayisi})", seferler.Count(), oncekiSayisi);
                    }
                    else
                    {
                        _logger.LogWarning("Geçersiz tarih formatı: {Tarih}", tarih);
                }
            }

            // Sefer detaylarını hazırla
                _logger.LogInformation("Sefer detayları hazırlanıyor...");
            var seferDetaylari = new List<SeferDetayViewModel>();
            
            foreach (var sefer in seferler)
            {
                    try
                    {
                        _logger.LogDebug("Sefer işleniyor: ID={SeferId}", sefer.SeferID);

                var otobus = otobusler.FirstOrDefault(o => o.OtobusID == sefer.OtobusID);
                        if (otobus == null)
                        {
                            _logger.LogWarning("Sefer {SeferId} için otobüs bulunamadı: OtobusID={OtobusId}", sefer.SeferID, sefer.OtobusID);
                        }

                var guzergah = guzergahlar.FirstOrDefault(g => g.GuzergahID == sefer.GuzergahID);
                        if (guzergah == null)
                        {
                            _logger.LogWarning("Sefer {SeferId} için güzergah bulunamadı: GuzergahID={GuzergahId}", sefer.SeferID, sefer.GuzergahID);
                        }
                
                // Bu otobüsün koltuk sayısı
                var otobusKoltuklari = koltuklar.Where(k => k.OtobusID == sefer.OtobusID).ToList();
                var toplamKoltuk = otobusKoltuklari.Count();
                
                // Bu sefer için satılan aktif biletler (iptal edilenler hariç)
                var seferBiletleri = biletler.Where(b => b.SeferID == sefer.SeferID && b.BiletDurumu == "Aktif").ToList();
                var doluKoltuk = seferBiletleri.Count();
                
                        var detay = new SeferDetayViewModel
                {
                    Sefer = sefer,
                    Otobus = otobus,
                    Guzergah = guzergah,
                    ToplamKoltukSayisi = toplamKoltuk > 0 ? toplamKoltuk : (otobus?.KoltukSayısı ?? 45),
                    DoluKoltukSayisi = doluKoltuk,
                    BiletFiyati = sefer.Fiyat, // Gerçek sefer fiyatını kullan
                    SatilanBiletSayisi = doluKoltuk,
                    DolulukOrani = toplamKoltuk > 0 ? Math.Round((doluKoltuk / (decimal)toplamKoltuk) * 100, 1) : 0
                        };

                        seferDetaylari.Add(detay);
                        _logger.LogDebug("Sefer detayı eklendi: ID={SeferId}, ToplamKoltuk={ToplamKoltuk}, DoluKoltuk={DoluKoltuk}", 
                            sefer.SeferID, detay.ToplamKoltukSayisi, detay.DoluKoltukSayisi);
            }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Sefer {SeferId} işlenirken hata oluştu", sefer.SeferID);
                        // Bu seferi atla, diğerlerini işlemeye devam et
                        continue;
                    }
                }

                _logger.LogInformation("Toplam {DetaySayisi} sefer detayı hazırlandı", seferDetaylari.Count);

            ViewBag.Nereden = nereden;
            ViewBag.Nereye = nereye;
            ViewBag.Tarih = tarih;
            ViewBag.YolcuSayisi = yolcuSayisi;

                _logger.LogInformation("SeferListesi metodu başarıyla tamamlandı");
            return View(seferDetaylari);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SeferListesi metodunda beklenmeyen hata oluştu. Parametreler: nereden={Nereden}, nereye={Nereye}, tarih={Tarih}, yolcuSayisi={YolcuSayisi}", 
                    nereden, nereye, tarih, yolcuSayisi);

                // Kullanıcıya hata mesajı göster
                TempData["Error"] = "Sefer arama sırasında bir hata oluştu. Lütfen tekrar deneyin.";
                
                // Ana sayfaya yönlendir
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Detay(int id)
        {
            var sefer = await _seferRepository.GetByIdAsync(id);
            if (sefer == null)
            {
                return NotFound();
            }

            return View(sefer);
        }
    }
} 


 
 
 
 