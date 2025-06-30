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

        public SeferController(
            IRepository<Sefer> seferRepository,
            IRepository<Guzergah> guzergahRepository,
            IRepository<Otobus> otobusRepository,
            IRepository<Koltuk> koltukRepository,
            IRepository<Bilet> biletRepository)
        {
            _seferRepository = seferRepository;
            _guzergahRepository = guzergahRepository;
            _otobusRepository = otobusRepository;
            _koltukRepository = koltukRepository;
            _biletRepository = biletRepository;
        }

        public async Task<IActionResult> Index()
        {
            var seferler = await _seferRepository.GetAllAsync();
            return View(seferler);
        }

        public async Task<IActionResult> SeferListesi(string nereden, string nereye, string tarih, int yolcuSayisi = 1)
        {
            var seferler = await _seferRepository.GetAllAsync();
            var otobusler = await _otobusRepository.GetAllAsync();
            var guzergahlar = await _guzergahRepository.GetAllAsync();
            var koltuklar = await _koltukRepository.GetAllAsync();
            var biletler = await _biletRepository.GetAllAsync();
            
            // Arama filtresi
            if (!string.IsNullOrEmpty(nereden) && !string.IsNullOrEmpty(nereye))
            {
                var uygunGuzergah = guzergahlar.FirstOrDefault(g => 
                    g.Nereden.Contains(nereden) && g.Nereye.Contains(nereye));
                
                if (uygunGuzergah != null)
                {
                    seferler = seferler.Where(s => s.GuzergahID == uygunGuzergah.GuzergahID).ToList();
                }
            }

            if (!string.IsNullOrEmpty(tarih))
            {
                if (DateTime.TryParse(tarih, out DateTime seferTarihi))
                {
                    seferler = seferler.Where(s => s.Tarih.Date == seferTarihi.Date).ToList();
                }
            }

            // Sefer detaylarını hazırla
            var seferDetaylari = new List<SeferDetayViewModel>();
            
            foreach (var sefer in seferler)
            {
                var otobus = otobusler.FirstOrDefault(o => o.OtobusID == sefer.OtobusID);
                var guzergah = guzergahlar.FirstOrDefault(g => g.GuzergahID == sefer.GuzergahID);
                
                // Bu otobüsün koltuk sayısı
                var otobusKoltuklari = koltuklar.Where(k => k.OtobusID == sefer.OtobusID).ToList();
                var toplamKoltuk = otobusKoltuklari.Count();
                
                // Bu sefer için satılan aktif biletler (iptal edilenler hariç)
                var seferBiletleri = biletler.Where(b => b.SeferID == sefer.SeferID && b.BiletDurumu == "Aktif").ToList();
                var doluKoltuk = seferBiletleri.Count();
                
                seferDetaylari.Add(new SeferDetayViewModel
                {
                    Sefer = sefer,
                    Otobus = otobus,
                    Guzergah = guzergah,
                    ToplamKoltukSayisi = toplamKoltuk > 0 ? toplamKoltuk : (otobus?.KoltukSayısı ?? 45),
                    DoluKoltukSayisi = doluKoltuk
                });
            }

            ViewBag.Nereden = nereden;
            ViewBag.Nereye = nereye;
            ViewBag.Tarih = tarih;
            ViewBag.YolcuSayisi = yolcuSayisi;

            return View(seferDetaylari);
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


 
 
 
 