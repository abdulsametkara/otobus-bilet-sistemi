using Microsoft.EntityFrameworkCore;
using OtobusBiletSistemi.Core.Data;
using OtobusBiletSistemi.Web.Models;

namespace OtobusBiletSistemi.Web.Services
{
    public class RaporService : IRaporService
    {
        private readonly AppDbContext _context;

        public RaporService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            // Varsayılan tarih aralığı: Son 30 gün
            var basla = baslangicTarihi ?? DateTime.Now.AddDays(-30);
            var bitir = bitisTarihi ?? DateTime.Now;
            
            // Alt raporları çek
            var satisRaporu = await GetSatisRaporuAsync(basla, bitir, null);
            var gelirRaporu = await GetGelirRaporuAsync(basla, bitir, null);
            var seferPerformans = await GetSeferPerformansAsync(basla, bitir, null);
            var musteriAnalizi = await GetMusteriAnaliziAsync(basla, bitir);
            var iptalIadeRaporu = await GetIptalIadeRaporuAsync(basla, bitir, null);

            var dashboard = new DashboardViewModel
            {
                BaslangicTarihi = basla,
                BitisTarihi = bitir,
                
                // Dashboard ana verileri
                ToplamSatis = satisRaporu.GunlukSatis,
                BuAySatis = satisRaporu.AylikSatis,
                ToplamGelir = gelirRaporu.GunlukGelir,
                BuAyGelir = gelirRaporu.AylikGelir,
                OrtalamaDoluluk = seferPerformans.OrtalamaDolulukOrani,
                AktifSeferSayisi = seferPerformans.AktifSeferSayisi,
                ToplamMusteriSayisi = musteriAnalizi.ToplamMusteriSayisi,
                YeniMusteriler = musteriAnalizi.YeniMusteriSayisi,
                IptalSayisi = iptalIadeRaporu.IptalEdilenBiletSayisi,
                IptalOrani = iptalIadeRaporu.IptalOrani,
                IadeTutari = iptalIadeRaporu.ToplamIadeTutari,
                IadeOrani = iptalIadeRaporu.IptalOrani,
                
                // Detaylı raporlar
                SatisRaporu = satisRaporu,
                GelirRaporu = gelirRaporu,
                SeferPerformans = seferPerformans,
                MusteriAnalizi = musteriAnalizi,
                IptalIadeRaporu = iptalIadeRaporu
            };

            return dashboard;
        }

        public async Task<SatisRaporuViewModel> GetSatisRaporuAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            // Varsayılan tarih aralığı: Veritabanındaki veriler 2025'te olduğu için geniş aralık
            var basla = baslangicTarihi ?? new DateTime(2025, 1, 1);
            var bitir = bitisTarihi ?? new DateTime(2026, 12, 31);
            
            // Debug için önce tüm biletleri çekelim
            var tumBiletler = await _context.Biletler.CountAsync();
            System.Diagnostics.Debug.WriteLine($"Toplam bilet sayısı: {tumBiletler}");
            System.Diagnostics.Debug.WriteLine($"Tarih aralığı: {basla:yyyy-MM-dd} - {bitir:yyyy-MM-dd}");

            var biletlerQuery = _context.Biletler
                .Include(b => b.Sefer)
                .ThenInclude(s => s.Guzergah)
                .Include(b => b.Odeme)
                .AsNoTracking()
                .Where(b => b.BiletDurumu != "İptal") // İptal edilmiş biletleri satış sayısına dahil etme
                .Where(b => b.BiletTarihi.HasValue && b.BiletTarihi.Value >= basla && b.BiletTarihi.Value <= bitir); // TARİH FİLTRESİ EKLENDİ

            if (guzergahId.HasValue)
            {
                biletlerQuery = biletlerQuery.Where(b => b.Sefer.GuzergahID == guzergahId.Value);
            }

            var biletler = await biletlerQuery.ToListAsync();

            var toplamSatis = biletler.Count;
            
            // Debug için bilet sayısını logla
            System.Diagnostics.Debug.WriteLine($"Tarih aralığında çekilen bilet sayısı: {toplamSatis}");
            
            // Günlük hesaplar (seçilen tarih aralığında)
            var gunSayisi = (int)(bitir.Date - basla.Date).TotalDays + 1;
            var gunlukOrtalama = gunSayisi > 0 ? toplamSatis / (decimal)gunSayisi : 0;
            
            // Bugünkü satış
            var gunlukSatis = biletler.Count(b => b.BiletTarihi.HasValue && b.BiletTarihi.Value.Date == DateTime.Today);
            
            // Haftalık hesaplar (son 7 gün)
            var haftalikBaslangic = DateTime.Today.AddDays(-6); // 7 gün (bugün dahil)
            var haftalikSatis = biletler.Count(b => b.BiletTarihi.HasValue && 
                b.BiletTarihi.Value.Date >= haftalikBaslangic && 
                b.BiletTarihi.Value.Date <= DateTime.Today);
            
            // Aylık hesaplar (son 30 gün)
            var aylikBaslangic = DateTime.Today.AddDays(-29); // 30 gün (bugün dahil)
            var aylikSatis = biletler.Count(b => b.BiletTarihi.HasValue && 
                b.BiletTarihi.Value.Date >= aylikBaslangic && 
                b.BiletTarihi.Value.Date <= DateTime.Today);

            System.Diagnostics.Debug.WriteLine($"Çekilen bilet sayısı (tarih aralığında, iptal hariç): {toplamSatis}");
            System.Diagnostics.Debug.WriteLine($"Günlük ortalama: {gunlukOrtalama}");

            // Önceki dönem karşılaştırması
            var donemGunSayisi = (int)(bitir.Date - basla.Date).TotalDays + 1;
            var oncekiDonemBaslangic = basla.AddDays(-donemGunSayisi);
            var oncekiDonemBitis = basla.AddSeconds(-1);
            
            var oncekiDonemSatis = await _context.Biletler
                .AsNoTracking()
                .Where(b => b.BiletTarihi.HasValue && b.BiletTarihi >= oncekiDonemBaslangic && b.BiletTarihi <= oncekiDonemBitis)
                .Where(b => b.BiletDurumu != "İptal")
                .CountAsync();

            var satisArtisOrani = oncekiDonemSatis > 0 ? 
                ((toplamSatis - oncekiDonemSatis) / (decimal)oncekiDonemSatis) * 100 : 
                (toplamSatis > 0 ? 100 : 0);

            // Güzergah bazlı satış
            var guzergahSatis = biletler
                .Where(b => b.Sefer?.Guzergah != null)
                .GroupBy(b => new { b.Sefer.Guzergah.GuzergahID, Guzergah = $"{b.Sefer.Guzergah.Nereden} - {b.Sefer.Guzergah.Nereye}" })
                .Select(g => new GuzergahSatisModel
                {
                    GuzergahAdi = g.Key.Guzergah,
                    BiletSayisi = g.Count(),
                    Gelir = g.Sum(b => b.Sefer.Fiyat),
                    Oran = toplamSatis > 0 ? (g.Count() / (decimal)toplamSatis) * 100 : 0
                })
                .OrderByDescending(g => g.BiletSayisi)
                .ToList();

            // Satış trendi (günlük)
            var satisTrendi = await GetSatisTrendi(basla, bitir, guzergahId);

            // Saatlik satış dağılımı - sadece bugün için (varsayılan)
            var varsayilanGun = DateTime.Today;
            if (varsayilanGun < basla.Date || varsayilanGun > bitir.Date)
            {
                // Eğer bugün seçili tarih aralığında değilse, aralığın ortasını al
                varsayilanGun = basla.Date.AddDays((bitir.Date - basla.Date).Days / 2);
            }
            var saatlikSatis = await GetSaatlikSatisAsync(varsayilanGun, guzergahId);

            // Ortalama bilet fiyatı
            var ortalamaBiletFiyati = biletler.Any() ? biletler.Average(b => b.Sefer?.Fiyat ?? 0) : 0;
            System.Diagnostics.Debug.WriteLine($"Ortalama bilet fiyatı: {ortalamaBiletFiyati}");

            return new SatisRaporuViewModel
            {
                ToplamSatis = toplamSatis,
                ToplamBiletSatisi = toplamSatis,
                GunlukSatis = gunlukSatis,
                GunlukOrtalama = gunlukOrtalama, // Doğru günlük ortalama
                HaftalikSatis = haftalikSatis,
                HaftalikOrtalama = haftalikSatis > 0 ? haftalikSatis / 7m : 0,
                AylikSatis = aylikSatis,
                ArtisOrani = satisArtisOrani,
                SatisArtisOrani = satisArtisOrani,
                OrtalamaBiletFiyati = ortalamaBiletFiyati,
                GuzergahBazliSatis = guzergahSatis.Select(g => new GuzergahSatisModel
                {
                    GuzergahAdi = g.GuzergahAdi,
                    BiletSayisi = g.BiletSayisi,
                    SatisSayisi = g.BiletSayisi,
                    Gelir = g.Gelir,
                    Oran = g.Oran,
                    YuzdeOrani = g.Oran
                }).ToList(),
                SatisTrendi = satisTrendi,
                SaatlikSatis = saatlikSatis
            };
        }

        public async Task<GelirRaporuViewModel> GetGelirRaporuAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            // Varsayılan tarih aralığı: Veritabanındaki veriler 2025'te olduğu için geniş aralık
            var basla = baslangicTarihi ?? new DateTime(2025, 1, 1);
            var bitir = bitisTarihi ?? new DateTime(2026, 12, 31);
            
            System.Diagnostics.Debug.WriteLine($"GetGelirRaporu - Tarih aralığı: {basla:yyyy-MM-dd} - {bitir:yyyy-MM-dd}");

            var biletlerQuery = _context.Biletler
                .Include(b => b.Sefer)
                .ThenInclude(s => s.Guzergah)
                .Include(b => b.Odeme)
                .AsNoTracking()
                .Where(b => b.BiletDurumu != "İptal"); // İptal edilmiş biletleri gelir hesabına dahil etme

            if (guzergahId.HasValue)
            {
                biletlerQuery = biletlerQuery.Where(b => b.Sefer.GuzergahID == guzergahId.Value);
            }

            var tumBiletler = await biletlerQuery.ToListAsync();
            
            // BiletTarihi veya Sefer.Tarih kullan ve tarih aralığına göre filtrele
            var biletler = tumBiletler.Where(b =>
            {
                var kullanilacakTarih = b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date;
                return kullanilacakTarih.HasValue && 
                       kullanilacakTarih.Value >= basla.Date && 
                       kullanilacakTarih.Value <= bitir.Date;
            }).ToList();

            System.Diagnostics.Debug.WriteLine($"GetGelirRaporu - Tarih aralığındaki bilet sayısı: {biletler.Count}");

            var toplamGelir = biletler.Sum(b => b.Sefer?.Fiyat ?? 0);
            
            // Günlük gelir hesaplama (bugünkü gelir)
            var gunlukGelir = biletler
                .Where(b => (b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date) == DateTime.Today)
                .Sum(b => b.Sefer?.Fiyat ?? 0);
            
            // Haftalık gelir (son 7 gün)
            var haftalikBaslangic = DateTime.Today.AddDays(-6);
            var haftalikGelir = biletler
                .Where(b => (b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date) >= haftalikBaslangic && 
                           (b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date) <= DateTime.Today)
                .Sum(b => b.Sefer?.Fiyat ?? 0);
            
            // Aylık gelir (son 30 gün)
            var aylikBaslangic = DateTime.Today.AddDays(-29);
            var aylikGelir = biletler
                .Where(b => (b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date) >= aylikBaslangic && 
                           (b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date) <= DateTime.Today)
                .Sum(b => b.Sefer?.Fiyat ?? 0);

            System.Diagnostics.Debug.WriteLine($"Toplam gelir (tarih aralığında, iptal hariç): {toplamGelir}");
            var ortalamaBiletFiyati = biletler.Any() ? biletler.Average(b => b.Sefer?.Fiyat ?? 0) : 0;

            // Önceki dönem karşılaştırması
            var donemGunSayisi = (int)(bitir.Date - basla.Date).TotalDays + 1;
            var oncekiDonemBaslangic = basla.AddDays(-donemGunSayisi);
            var oncekiDonemBitis = basla.AddSeconds(-1);
            
            System.Diagnostics.Debug.WriteLine($"Önceki dönem: {oncekiDonemBaslangic:yyyy-MM-dd} - {oncekiDonemBitis:yyyy-MM-dd}");
            
            // Önceki dönem biletlerini al
            var oncekiDonemBiletlerQuery = _context.Biletler
                .Include(b => b.Sefer)
                .AsNoTracking()
                .Where(b => b.BiletDurumu != "İptal");

            if (guzergahId.HasValue)
            {
                oncekiDonemBiletlerQuery = oncekiDonemBiletlerQuery.Where(b => b.Sefer.GuzergahID == guzergahId.Value);
            }

            var oncekiDonemTumBiletler = await oncekiDonemBiletlerQuery.ToListAsync();
            
            var oncekiDonemBiletler = oncekiDonemTumBiletler.Where(b =>
            {
                var kullanilacakTarih = b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date;
                return kullanilacakTarih.HasValue && 
                       kullanilacakTarih.Value >= oncekiDonemBaslangic.Date && 
                       kullanilacakTarih.Value <= oncekiDonemBitis.Date;
            }).ToList();

            var oncekiDonemGelir = oncekiDonemBiletler.Sum(b => b.Sefer?.Fiyat ?? 0);
            
            System.Diagnostics.Debug.WriteLine($"Bu dönem gelir: ₺{toplamGelir}, Önceki dönem gelir: ₺{oncekiDonemGelir}");

            var gelirArtisOrani = oncekiDonemGelir > 0 ? 
                ((toplamGelir - oncekiDonemGelir) / oncekiDonemGelir) * 100 : 
                (toplamGelir > 0 ? 100 : 0);

            // Sefer bazlı gelir
            var seferGelir = biletler
                .Where(b => b.Sefer != null)
                .GroupBy(b => new { b.SeferID, b.Sefer.Tarih, Sefer = $"{b.Sefer.Kalkis} - {b.Sefer.Varis}" })
                .Select(g => new SeferGelirModel
                {
                    SeferID = g.Key.SeferID,
                    SeferAdi = g.Key.Sefer,
                    SeferTarihi = g.Key.Tarih,
                    Gelir = g.Sum(b => b.Sefer.Fiyat),
                    BiletSayisi = g.Count(),
                    DolulukOrani = CalculateDolulukOrani(g.Key.SeferID, g.Count())
                })
                .OrderByDescending(s => s.Gelir)
                .Take(20)
                .ToList();

            // Gelir trendi
            var gelirTrendi = await GetGelirTrendi(basla, bitir, guzergahId);

            return new GelirRaporuViewModel
            {
                ToplamGelir = toplamGelir,
                GunlukGelir = gunlukGelir,
                HaftalikGelir = haftalikGelir,
                AylikGelir = aylikGelir,
                OrtalamaBiletFiyati = ortalamaBiletFiyati,
                GelirArtisOrani = gelirArtisOrani,
                SeferBazliGelir = seferGelir,
                GelirTrendi = gelirTrendi
            };
        }

        public async Task<SeferPerformansViewModel> GetSeferPerformansAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            // Varsayılan tarih aralığı: Son 30 gün
            var basla = baslangicTarihi ?? DateTime.Now.AddDays(-30);
            var bitir = bitisTarihi ?? DateTime.Now;
            
            var seferlerQuery = _context.Seferler
                .Include(s => s.Otobus)
                .Include(s => s.Guzergah)
                .Include(s => s.Biletler)
                .AsNoTracking()
                .Where(s => s.Tarih >= basla && s.Tarih <= bitir);

            if (guzergahId.HasValue)
            {
                seferlerQuery = seferlerQuery.Where(s => s.GuzergahID == guzergahId.Value);
            }

            var seferler = await seferlerQuery.ToListAsync();

            var aktifSeferSayisi = seferler.Count(s => s.Tarih >= DateTime.Today);
            var tamamlananSeferSayisi = seferler.Count(s => s.Tarih < DateTime.Today);
            var iptalEdilenSeferSayisi = 0; // Henüz iptal sistemi yok
            var seferIptalOrani = 0;

            // Ortalama doluluk oranı
            var ortalamaDoluluk = await CalculateOrtalamaDoluluk(seferler);

            // Sefer dolulukları
            var seferDoluluklari = seferler
                .Select(s => new SeferDolulukModel
                {
                    SeferID = s.SeferID,
                    SeferAdi = $"{s.Kalkis} - {s.Varis}",
                    SeferTarihi = s.Tarih,
                    ToplamKoltuk = s.Otobus?.KoltukSayısı ?? 40,
                    SatilanKoltuk = s.Biletler?.Count(b => b.BiletDurumu != "İptal") ?? 0,
                    DolulukOrani = CalculateDolulukOrani(s.SeferID, s.Biletler?.Count(b => b.BiletDurumu != "İptal") ?? 0)
                })
                .OrderByDescending(s => s.DolulukOrani)
                .Take(20)
                .ToList();

            // Güzergah bazlı doluluk
            var guzergahDoluluklari = seferler
                .Where(s => s.Guzergah != null)
                .GroupBy(s => new { s.GuzergahID, Guzergah = $"{s.Guzergah.Nereden} - {s.Guzergah.Nereye}" })
                .Select(g => new GuzergahDolulukModel
                {
                    GuzergahID = g.Key.GuzergahID,
                    GuzergahAdi = g.Key.Guzergah,
                    ToplamSefer = g.Count(),
                    OrtalamaDoluluk = g.Average(s => CalculateDolulukOrani(s.SeferID, s.Biletler?.Count(b => b.BiletDurumu != "İptal") ?? 0))
                })
                .OrderByDescending(g => g.OrtalamaDoluluk)
                .ToList();

            return new SeferPerformansViewModel
            {
                AktifSeferSayisi = aktifSeferSayisi,
                TamamlananSeferSayisi = tamamlananSeferSayisi,
                IptalEdilenSeferSayisi = iptalEdilenSeferSayisi,
                SeferIptalOrani = seferIptalOrani,
                OrtalamaDolulukOrani = ortalamaDoluluk,
                SeferDoluluklari = seferDoluluklari,
                GuzergahDoluluklari = guzergahDoluluklari
            };
        }

        public async Task<MusteriAnaliziViewModel> GetMusteriAnaliziAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            // Varsayılan tarih aralığı: Son 30 gün
            var basla = baslangicTarihi ?? DateTime.Now.AddDays(-30);
            var bitir = bitisTarihi ?? DateTime.Now;
            
            var toplamMusteriSayisi = await _context.Users.AsNoTracking().CountAsync();
            var yeniMusteriSayisi = toplamMusteriSayisi; // Yaklaşık değer

            // Aktif müşteri: Son 30 gün içinde bilet alan
            var aktifMusteriSayisi = await _context.Biletler
                .AsNoTracking()
                .Where(b => b.BiletTarihi >= DateTime.Today.AddDays(-30))
                .Where(b => b.BiletDurumu != "İptal")
                .Select(b => b.YolcuID)
                .Distinct()
                .CountAsync();

            // Sık bilet alan müşteriler (3+ bilet)
            var sikBiletAlanlar = await _context.Biletler
                .AsNoTracking()
                .Where(b => b.BiletTarihi >= basla && b.BiletTarihi <= bitir)
                .Where(b => b.BiletDurumu != "İptal")
                .GroupBy(b => b.YolcuID)
                .Where(g => g.Count() >= 3)
                .CountAsync();

            var sikBiletAlanOrani = toplamMusteriSayisi > 0 ? (sikBiletAlanlar / (decimal)toplamMusteriSayisi) * 100 : 0;

            // En çok bilet alan müşteriler (mock data - gerçek implement ederken Yolcu entity'si kullanılacak)
            var enCokBiletAlanMusteri = await _context.Biletler
                .Include(b => b.Sefer)
                .AsNoTracking()
                .Where(b => b.BiletTarihi >= basla && b.BiletTarihi <= bitir)
                .Where(b => b.BiletDurumu != "İptal")
                .GroupBy(b => b.YolcuID)
                .Select(g => new TopMusteriModel
                {
                    YolcuID = g.Key,
                    Ad = "Müşteri",
                    Soyad = g.Key.ToString(),
                    Email = $"musteri{g.Key}@email.com",
                    BiletSayisi = g.Count(),
                    ToplamHarcama = g.Sum(b => b.Sefer.Fiyat)
                })
                .OrderByDescending(m => m.BiletSayisi)
                .Take(10)
                .ToListAsync();

            // Müşteri artış trendi (basit implementasyon)
            System.Diagnostics.Debug.WriteLine($"GetMusteriAnaliziAsync - Müşteri artış trendi çağrılıyor. Tarih aralığı: {basla:yyyy-MM-dd} - {bitir:yyyy-MM-dd}");
            var musteriArtisiTrendi = await GetMusteriArtisiTrendi(basla, bitir);
            System.Diagnostics.Debug.WriteLine($"GetMusteriAnaliziAsync - Dönen trend verisi count: {musteriArtisiTrendi?.Count ?? 0}");

            return new MusteriAnaliziViewModel
            {
                ToplamMusteriSayisi = toplamMusteriSayisi,
                YeniMusteriSayisi = Math.Min(yeniMusteriSayisi, 68), // Mock değer
                AktifMusteriSayisi = aktifMusteriSayisi,
                SikBiletAlanOrani = sikBiletAlanOrani,
                EnCokBiletAlanMusteri = enCokBiletAlanMusteri,
                MusteriArtisiTrendi = musteriArtisiTrendi
            };
        }

        public async Task<IptalIadeRaporuViewModel> GetIptalIadeRaporuAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            // Varsayılan tarih aralığı: Son 30 gün
            var basla = baslangicTarihi ?? DateTime.Now.AddDays(-30);
            var bitir = bitisTarihi ?? DateTime.Now;
            
            // Debug için önce iptal biletleri sayalım
            var tumIptalBiletler = await _context.Biletler.Where(b => b.BiletDurumu == "İptal").CountAsync();
            System.Diagnostics.Debug.WriteLine($"Toplam iptal edilen bilet sayısı: {tumIptalBiletler}");

            var iptalBiletlerQuery = _context.Biletler
                .Include(b => b.Sefer)
                .ThenInclude(s => s.Guzergah)
                .AsNoTracking()
                .Where(b => b.BiletDurumu == "İptal");
                // Geçici olarak tarih filtrelerini kaldırdım

            if (guzergahId.HasValue)
            {
                iptalBiletlerQuery = iptalBiletlerQuery.Where(b => b.Sefer.GuzergahID == guzergahId.Value);
            }

            var iptalBiletler = await iptalBiletlerQuery.ToListAsync();

            var iptalEdilenBiletSayisi = iptalBiletler.Count;
            var toplamIadeTutari = iptalBiletler.Sum(b => b.Sefer?.Fiyat ?? 0);

            System.Diagnostics.Debug.WriteLine($"İptal edilen bilet sayısı: {iptalEdilenBiletSayisi}");
            System.Diagnostics.Debug.WriteLine($"Toplam iade tutarı: {toplamIadeTutari}");

            // Toplam bilet sayısına göre iptal oranı (tarih filtresi olmadan)
            var toplamBiletSayisi = await _context.Biletler
                .AsNoTracking()
                .CountAsync(); // Geçici olarak tarih filtresi yok

            var iptalOrani = toplamBiletSayisi > 0 ? (iptalEdilenBiletSayisi / (decimal)toplamBiletSayisi) * 100 : 0;

            // İptal nedenleri (mock data)
            var iptalNedenleri = new List<IptalNedenModel>
            {
                new IptalNedenModel { Neden = "Müşteri İsteği", Sayi = iptalEdilenBiletSayisi / 2, Oran = 50 },
                new IptalNedenModel { Neden = "Sefer İptali", Sayi = iptalEdilenBiletSayisi / 3, Oran = 33 },
                new IptalNedenModel { Neden = "Diğer", Sayi = iptalEdilenBiletSayisi / 6, Oran = 17 }
            };

            // İptal trendi
            var iptalTrendi = await GetIptalTrendi(basla, bitir, guzergahId);

            return new IptalIadeRaporuViewModel
            {
                IptalEdilenBiletSayisi = iptalEdilenBiletSayisi,
                IptalOrani = iptalOrani,
                ToplamIadeTutari = toplamIadeTutari,
                IptalNedenleri = iptalNedenleri,
                IptalTrendi = iptalTrendi
            };
        }

        // Yardımcı metotlar
        private decimal CalculateDolulukOrani(int seferID, int satilanKoltuk)
        {
            var sefer = _context.Seferler
                .Include(s => s.Otobus)
                .AsNoTracking()
                .FirstOrDefault(s => s.SeferID == seferID);

            var toplamKoltuk = sefer?.Otobus?.KoltukSayısı ?? 40;
            return toplamKoltuk > 0 ? (satilanKoltuk / (decimal)toplamKoltuk) * 100 : 0;
        }

        private async Task<decimal> CalculateOrtalamaDoluluk(List<OtobusBiletSistemi.Core.Entities.Sefer> seferler)
        {
            if (!seferler.Any()) return 0;

            var dolulukOranlari = seferler.Select(s => CalculateDolulukOrani(s.SeferID, 
                s.Biletler?.Count(b => b.BiletDurumu != "İptal") ?? 0)).ToList();

            return dolulukOranlari.Any() ? dolulukOranlari.Average() : 0;
        }

        private async Task<List<TrendModel>> GetSatisTrendi(DateTime baslangicTarihi, DateTime bitisTarihi, int? guzergahId)
        {
            var gunler = new List<TrendModel>();
            var currentDate = baslangicTarihi.Date;

            System.Diagnostics.Debug.WriteLine($"GetSatisTrendi çağrıldı - Başlangıç: {baslangicTarihi:yyyy-MM-dd}, Bitiş: {bitisTarihi:yyyy-MM-dd}");

            // Tüm biletleri tek seferde çek (tarih filtresi daha esnek)
            var biletlerQuery = _context.Biletler
                .Include(b => b.Sefer)
                .AsNoTracking()
                .Where(b => b.BiletDurumu != "İptal");

            if (guzergahId.HasValue)
            {
                biletlerQuery = biletlerQuery.Where(b => b.Sefer.GuzergahID == guzergahId.Value);
            }

            var tumBiletler = await biletlerQuery.ToListAsync();
            
            System.Diagnostics.Debug.WriteLine($"GetSatisTrendi - Toplam çekilen bilet sayısı: {tumBiletler.Count}");
            
            // BiletTarihi veya Sefer.Tarih kullan ve tarih aralığına göre filtrele
            var biletler = tumBiletler.Where(b =>
            {
                var kullanilacakTarih = b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date;
                return kullanilacakTarih.HasValue && 
                       kullanilacakTarih.Value >= baslangicTarihi.Date && 
                       kullanilacakTarih.Value <= bitisTarihi.Date;
            }).ToList();
            
            System.Diagnostics.Debug.WriteLine($"GetSatisTrendi - Tarih aralığındaki biletler: {biletler.Count}");

            // Günlük gruplayıp trend oluştur
            var gunlukSatislar = biletler
                .GroupBy(b => (b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date).Value)
                .ToDictionary(g => g.Key, g => g.Count());
                
            System.Diagnostics.Debug.WriteLine($"GetSatisTrendi - Günlük satış grubu sayısı: {gunlukSatislar.Count}");

            while (currentDate <= bitisTarihi.Date)
            {
                var gunlukSatis = gunlukSatislar.ContainsKey(currentDate) ? gunlukSatislar[currentDate] : 0;

                gunler.Add(new TrendModel
                {
                    Tarih = currentDate,
                    Deger = gunlukSatis,
                    Label = currentDate.ToString("dd/MM")
                });

                currentDate = currentDate.AddDays(1);
            }

            // Debug için trend verilerini logla
            System.Diagnostics.Debug.WriteLine($"Trend verileri oluşturuldu. Toplam gün sayısı: {gunler.Count}");
            foreach(var gun in gunler.Take(10))
            {
                System.Diagnostics.Debug.WriteLine($"Tarih: {gun.Tarih:dd/MM/yyyy}, Satış: {gun.Deger}");
            }

            // Tarihe göre sırala (eski tarihten yeniye)
            return gunler.OrderBy(g => g.Tarih).ToList();
        }

        private async Task<List<TrendModel>> GetGelirTrendi(DateTime baslangicTarihi, DateTime bitisTarihi, int? guzergahId)
        {
            var gunler = new List<TrendModel>();
            var currentDate = baslangicTarihi.Date;

            System.Diagnostics.Debug.WriteLine($"GetGelirTrendi çağrıldı - Başlangıç: {baslangicTarihi:yyyy-MM-dd}, Bitiş: {bitisTarihi:yyyy-MM-dd}");

            // Tüm biletleri tek seferde çek (tarih filtresi daha esnek)
            var biletlerQuery = _context.Biletler
                .Include(b => b.Sefer)
                .AsNoTracking()
                .Where(b => b.BiletDurumu != "İptal");

            if (guzergahId.HasValue)
            {
                biletlerQuery = biletlerQuery.Where(b => b.Sefer.GuzergahID == guzergahId.Value);
            }

            var tumBiletler = await biletlerQuery.ToListAsync();
            
            System.Diagnostics.Debug.WriteLine($"GetGelirTrendi - Toplam çekilen bilet sayısı: {tumBiletler.Count}");
            
            // BiletTarihi veya Sefer.Tarih kullan ve tarih aralığına göre filtrele
            var biletler = tumBiletler.Where(b =>
            {
                var kullanilacakTarih = b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date;
                return kullanilacakTarih.HasValue && 
                       kullanilacakTarih.Value >= baslangicTarihi.Date && 
                       kullanilacakTarih.Value <= bitisTarihi.Date;
            }).ToList();
            
            System.Diagnostics.Debug.WriteLine($"GetGelirTrendi - Tarih aralığındaki biletler: {biletler.Count}");

            // Günlük gruplayıp gelir trendi oluştur
            var gunlukGelirler = biletler
                .GroupBy(b => (b.BiletTarihi?.Date ?? b.Sefer?.Tarih.Date).Value)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.Sefer?.Fiyat ?? 0));
                
            System.Diagnostics.Debug.WriteLine($"GetGelirTrendi - Günlük gelir grubu sayısı: {gunlukGelirler.Count}");

            while (currentDate <= bitisTarihi.Date)
            {
                var gunlukGelir = gunlukGelirler.ContainsKey(currentDate) ? gunlukGelirler[currentDate] : 0;

                gunler.Add(new TrendModel
                {
                    Tarih = currentDate,
                    Deger = gunlukGelir,
                    Label = currentDate.ToString("dd/MM")
                });

                currentDate = currentDate.AddDays(1);
            }

            // Debug için gelir trend verilerini logla
            System.Diagnostics.Debug.WriteLine($"Gelir trend verileri oluşturuldu. Toplam gün sayısı: {gunler.Count}");
            foreach(var gun in gunler.Take(10))
            {
                System.Diagnostics.Debug.WriteLine($"Tarih: {gun.Tarih:dd/MM/yyyy}, Gelir: ₺{gun.Deger}");
            }

            // Tarihe göre sırala (eski tarihten yeniye)
            return gunler.OrderBy(g => g.Tarih).ToList();
        }

        private async Task<List<TrendModel>> GetMusteriArtisiTrendi(DateTime baslangicTarihi, DateTime bitisTarihi)
        {
            var gunler = new List<TrendModel>();
            var currentDate = baslangicTarihi.Date;

            System.Diagnostics.Debug.WriteLine($"GetMusteriArtisiTrendi çağrıldı - Başlangıç: {baslangicTarihi:yyyy-MM-dd}, Bitiş: {bitisTarihi:yyyy-MM-dd}");

            // Önce tüm yolcuları çekip durumu kontrol edelim
            var tumYolcular = await _context.Users.AsNoTracking().ToListAsync();
            System.Diagnostics.Debug.WriteLine($"Veritabanında toplam yolcu sayısı: {tumYolcular.Count}");
            
            var createDateOlanlar = tumYolcular.Where(u => u.CreateDate.HasValue).ToList();
            System.Diagnostics.Debug.WriteLine($"CreateDate'i olan yolcu sayısı: {createDateOlanlar.Count}");
            
            var emailOnaylilar = tumYolcular.Where(u => u.EmailConfirmed == true).ToList();
            System.Diagnostics.Debug.WriteLine($"Email'i onaylanmış yolcu sayısı: {emailOnaylilar.Count}");

            // CreateDate'i olan kayıtları tarihe göre logla
            foreach(var yolcu in createDateOlanlar.Take(5))
            {
                System.Diagnostics.Debug.WriteLine($"Yolcu {yolcu.Id}: CreateDate = {yolcu.CreateDate}, EmailConfirmed = {yolcu.EmailConfirmed}");
            }

            // YOLCU tablosundan CREATEDATE'e göre gerçek müşteri artış trendini çek
            // EmailConfirmed filtresi geçici olarak kaldırıldı - test için
            var yolcularQuery = _context.Users
                .AsNoTracking()
                .Where(u => u.CreateDate.HasValue); // CREATEDATE'i olan kayıtlar
                //.Where(u => u.EmailConfirmed == true); // Email'i onaylanmış hesaplar - geçici kapalı

            var yolcular = await yolcularQuery.ToListAsync();
            
            System.Diagnostics.Debug.WriteLine($"GetMusteriArtisiTrendi - CreateDate'i olan toplam yolcu sayısı: {yolcular.Count}");

            // Tarih aralığında olanları filtrele
            var tarihAraligindakiler = yolcular
                .Where(u => u.CreateDate.Value.Date >= baslangicTarihi.Date && u.CreateDate.Value.Date <= bitisTarihi.Date)
                .ToList();
                
            System.Diagnostics.Debug.WriteLine($"GetMusteriArtisiTrendi - Tarih aralığındaki yolcu sayısı: {tarihAraligindakiler.Count}");

            // Günlük gruplayıp trend oluştur
            var gunlukYeniMusteriler = tarihAraligindakiler
                .GroupBy(y => y.CreateDate.Value.Date)
                .ToDictionary(g => g.Key, g => g.Count());
                
            System.Diagnostics.Debug.WriteLine($"GetMusteriArtisiTrendi - Günlük müşteri grubu sayısı: {gunlukYeniMusteriler.Count}");

            // Gerçek veri ile trend oluştur
            while (currentDate <= bitisTarihi.Date)
            {
                var gunlukYeniMusteri = gunlukYeniMusteriler.ContainsKey(currentDate) ? gunlukYeniMusteriler[currentDate] : 0;

                gunler.Add(new TrendModel
                {
                    Tarih = currentDate,
                    Deger = gunlukYeniMusteri,
                    Label = currentDate.ToString("dd/MM")
                });

                currentDate = currentDate.AddDays(1);
            }

            // Debug için müşteri trend verilerini logla
            System.Diagnostics.Debug.WriteLine($"Müşteri trend verileri oluşturuldu. Toplam gün sayısı: {gunler.Count}");
            var toplamYeniMusteri = gunler.Sum(g => g.Deger);
            System.Diagnostics.Debug.WriteLine($"Toplam yeni müşteri sayısı: {toplamYeniMusteri}");
            
            foreach(var gun in gunler.Take(10))
            {
                System.Diagnostics.Debug.WriteLine($"Tarih: {gun.Tarih:dd/MM/yyyy}, Yeni Müşteri: {gun.Deger}");
            }
            
            // Eğer hiç data yoksa console'a uyarı yaz
            if (toplamYeniMusteri == 0)
            {
                System.Diagnostics.Debug.WriteLine("UYARI: Müşteri trend verisi boş!");
                System.Diagnostics.Debug.WriteLine($"Arama yapılan tarih aralığı: {baslangicTarihi:yyyy-MM-dd} - {bitisTarihi:yyyy-MM-dd}");
                System.Diagnostics.Debug.WriteLine($"CreateDate'i olan yolcu sayısı: {createDateOlanlar.Count}");
                System.Diagnostics.Debug.WriteLine($"Email onaylanmış yolcu sayısı: {emailOnaylilar.Count}");
            }

            // Tarihe göre sırala (eski tarihten yeniye)
            return gunler.OrderBy(g => g.Tarih).ToList();
        }

        private async Task<List<TrendModel>> GetIptalTrendi(DateTime baslangicTarihi, DateTime bitisTarihi, int? guzergahId)
        {
            var gunler = new List<TrendModel>();
            var currentDate = baslangicTarihi.Date;

            while (currentDate <= bitisTarihi.Date)
            {
                var gunlukIptal = await _context.Biletler
                    .Include(b => b.Sefer)
                    .AsNoTracking()
                    .Where(b => b.BiletTarihi.HasValue && b.BiletTarihi.Value.Date == currentDate)
                    .Where(b => b.BiletDurumu == "İptal")
                    .Where(b => !guzergahId.HasValue || b.Sefer.GuzergahID == guzergahId.Value)
                    .CountAsync();

                gunler.Add(new TrendModel
                {
                    Tarih = currentDate,
                    Deger = gunlukIptal,
                    Label = currentDate.ToString("dd/MM")
                });

                currentDate = currentDate.AddDays(1);
            }

            return gunler;
        }

        public async Task<List<ZamanBazliSatisModel>> GetSaatlikSatisAsync(DateTime tarih, int? guzergahId)
        {
            var biletlerQuery = _context.Biletler
                .Include(b => b.Sefer)
                .ThenInclude(s => s.Guzergah)
                .AsNoTracking()
                .Where(b => b.BiletTarihi.HasValue && b.BiletTarihi.Value.Date == tarih.Date);

            if (guzergahId.HasValue)
            {
                biletlerQuery = biletlerQuery.Where(b => b.Sefer.GuzergahID == guzergahId.Value);
            }

            var biletler = await biletlerQuery.ToListAsync();

            var saatlikSatis = biletler
                .GroupBy(b => b.BiletTarihi.Value.Hour)
                .Select(g => new ZamanBazliSatisModel
                {
                    Saat = g.Key,
                    BiletSayisi = g.Count(),
                    Gelir = g.Sum(b => b.Sefer?.Fiyat ?? 0)
                })
                .OrderBy(s => s.Saat)
                .ToList();

            return saatlikSatis;
        }
    }
} 