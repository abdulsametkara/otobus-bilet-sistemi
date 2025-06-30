using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OtobusBiletSistemi.Core.Interfaces;
using OtobusBiletSistemi.Core.Entities;
using OtobusBiletSistemi.Core.Data;

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

        public HomeController(
            IRepository<Bilet> biletRepository,
            IRepository<Sefer> seferRepository,
            UserManager<YolcuUser> userManager,
            IRepository<Odeme> odemeRepository)
        {
            _biletRepository = biletRepository;
            _seferRepository = seferRepository;
            _userManager = userManager;
            _odemeRepository = odemeRepository;
        }

        public async Task<IActionResult> Index()
        {
            var tumBiletler = await _biletRepository.GetAllAsync();
            var tumSeferler = await _seferRepository.GetAllAsync();
            var tumKullanicilar = _userManager.Users.ToList();
            var tumOdemeler = await _odemeRepository.GetAllAsync();

            var bugun = DateTime.Today;
            var bugunBiletler = tumBiletler.Where(b => b.BiletTarihi?.Date == bugun).ToList();
            var bugunOdemeler = tumOdemeler.Where(o => o.OdemeTarihi.Date == bugun).ToList();

            var ayBasi = new DateTime(bugun.Year, bugun.Month, 1);
            var aylikBiletler = tumBiletler.Where(b => b.BiletTarihi >= ayBasi).ToList();
            var aylikOdemeler = tumOdemeler.Where(o => o.OdemeTarihi >= ayBasi).ToList();

            var model = new AdminDashboardViewModel
            {
                ToplamBiletSayisi = tumBiletler.Count(),
                ToplamSeferSayisi = tumSeferler.Count(),
                ToplamKullaniciSayisi = tumKullanicilar.Count(),
                ToplamGelir = tumOdemeler.Sum(o => o.OdemeTutari),

                BugunBiletSayisi = bugunBiletler.Count(),
                BugunGelir = bugunOdemeler.Sum(o => o.OdemeTutari),

                AylikBiletSayisi = aylikBiletler.Count(),
                AylikGelir = aylikOdemeler.Sum(o => o.OdemeTutari),

                AktifBiletSayisi = tumBiletler.Count(b => b.BiletDurumu == "Aktif"),
                IptalBiletSayisi = tumBiletler.Count(b => b.BiletDurumu == "İptal")
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
    }
} 