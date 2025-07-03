using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Data;
using Microsoft.EntityFrameworkCore;
using OtobusBiletSistemi.Web.Services;

namespace OtobusBiletSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly IRepository<Bilet> _biletRepository;
        private readonly IRepository<Sefer> _seferRepository;
        private readonly UserManager<YolcuUser> _userManager;
        private readonly IRepository<Odeme> _odemeRepository;
        private readonly IRaporService _raporService;

        public HomeController(
            IRepository<Bilet> biletRepository,
            IRepository<Sefer> seferRepository,
            UserManager<YolcuUser> userManager,
            IRepository<Odeme> odemeRepository,
            IRaporService raporService)
        {
            _biletRepository = biletRepository;
            _seferRepository = seferRepository;
            _userManager = userManager;
            _odemeRepository = odemeRepository;
            _raporService = raporService;
        }

        public async Task<IActionResult> Index()
        {
            // RaporService'den güncel gelir verilerini al
            var gelirRaporu = await _raporService.GetGelirRaporuAsync();
            var satisRaporu = await _raporService.GetSatisRaporuAsync();

            // Diğer temel veriler için hızlı hesaplamalar
            var tumBiletler = await _biletRepository.GetAllAsync();
            var tumSeferler = await _seferRepository.GetAllAsync();
            var tumKullanicilar = _userManager.Users.ToList();
            var tumOdemeler = await _odemeRepository.GetAllAsync();

            // Son işlemleri çek
            var sonIslemler = new List<SonIslemModel>();

            // Son 5 bilet satışı
            var sonBiletler = tumBiletler
                .Where(b => b.BiletTarihi.HasValue && b.BiletDurumu == "Aktif")
                .OrderByDescending(b => b.BiletTarihi)
                .Take(5)
                .ToList();

            foreach (var bilet in sonBiletler)
            {
                sonIslemler.Add(new SonIslemModel
                {
                    Tip = "Bilet Satışı",
                    Aciklama = $"Bilet satışı gerçekleşti - {bilet.BiletNo}",
                    Tarih = bilet.BiletTarihi ?? DateTime.Now,
                    Icon = "fas fa-ticket-alt",
                    CssClass = "success"
                });
            }

            // Son 3 ödeme
            var sonOdemeler = tumOdemeler
                .Where(o => o.OdemeDurumu == "Tamamlandı")
                .OrderByDescending(o => o.OdemeTarihi)
                .Take(3)
                .ToList();

            foreach (var odeme in sonOdemeler)
            {
                sonIslemler.Add(new SonIslemModel
                {
                    Tip = "Ödeme",
                    Aciklama = $"₺{odeme.OdemeTutari:N2} tutarında ödeme alındı",
                    Tarih = odeme.OdemeTarihi,
                    Icon = "fas fa-credit-card",
                    CssClass = "info"
                });
            }

            // Son 2 iptal
            var sonIptaller = tumBiletler
                .Where(b => b.BiletDurumu == "İptal")
                .OrderByDescending(b => b.BiletTarihi)
                .Take(2)
                .ToList();

            foreach (var iptal in sonIptaller)
            {
                sonIslemler.Add(new SonIslemModel
                {
                    Tip = "Bilet İptali",
                    Aciklama = $"Bilet iptal edildi - {iptal.BiletNo}",
                    Tarih = iptal.BiletTarihi ?? DateTime.Now,
                    Icon = "fas fa-times-circle",
                    CssClass = "danger"
                });
            }

            // Tarihe göre sırala
            sonIslemler = sonIslemler.OrderByDescending(x => x.Tarih).Take(10).ToList();

            var model = new AdminDashboardViewModel
            {
                // Rapor servisinden güncel veriler
                ToplamGelir = gelirRaporu.ToplamGelir,
                BugunGelir = gelirRaporu.GunlukGelir,
                AylikGelir = gelirRaporu.AylikGelir,

                ToplamBiletSayisi = satisRaporu.ToplamBiletSatisi,
                BugunBiletSayisi = satisRaporu.GunlukSatis,
                AylikBiletSayisi = satisRaporu.AylikSatis,

                // Diğer temel veriler
                ToplamSeferSayisi = tumSeferler.Count(),
                ToplamKullaniciSayisi = tumKullanicilar.Count(),
                AktifBiletSayisi = tumBiletler.Count(b => b.BiletDurumu == "Aktif"),
                IptalBiletSayisi = tumBiletler.Count(b => b.BiletDurumu == "İptal"),

                SonIslemler = sonIslemler
            };

            return View(model);
        }
    }

    public class AdminDashboardViewModel
    {
        public int ToplamBiletSayisi { get; set; }
        public int ToplamSeferSayisi { get; set; }
        public int ToplamKullaniciSayisi { get; set; }
        public decimal ToplamGelir { get; set; }

        public int BugunBiletSayisi { get; set; }
        public decimal BugunGelir { get; set; }

        public int AylikBiletSayisi { get; set; }
        public decimal AylikGelir { get; set; }

        public int AktifBiletSayisi { get; set; }
        public int IptalBiletSayisi { get; set; }

        public List<SonIslemModel> SonIslemler { get; set; } = new List<SonIslemModel>();
    }

    public class SonIslemModel
    {
        public string Tip { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public DateTime Tarih { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string CssClass { get; set; } = string.Empty;
        
        public string TarihFormatli => Tarih.ToString("dd.MM.yyyy HH:mm");
        public string ZamanFarki
        {
            get
            {
                var fark = DateTime.Now - Tarih;
                if (fark.TotalMinutes < 1)
                    return "Şimdi";
                if (fark.TotalMinutes < 60)
                    return $"{(int)fark.TotalMinutes} dakika önce";
                if (fark.TotalHours < 24)
                    return $"{(int)fark.TotalHours} saat önce";
                return $"{(int)fark.TotalDays} gün önce";
            }
        }
    }
} 