using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OtobusBiletSistemi.Core.Data;
using OtobusBiletSistemi.Web.Models;
using OtobusBiletSistemi.Web.Services;
using System.Text.Json;

namespace OtobusBiletSistemi.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class RaporController : Controller
    {
        private readonly IRaporService _raporService;
        private readonly AppDbContext _context;

        public RaporController(IRaporService raporService, AppDbContext context)
        {
            _raporService = raporService;
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            ViewData["Title"] = "Raporlar ve Analitik";
            
            if (!baslangicTarihi.HasValue)
                baslangicTarihi = DateTime.Now.AddDays(-15);
            if (!bitisTarihi.HasValue)
                bitisTarihi = DateTime.Now.AddDays(15);

            var dashboardData = await _raporService.GetDashboardDataAsync(baslangicTarihi, bitisTarihi);
            
            // Güzergah listesi için
            ViewBag.Guzergahlar = await _context.Guzergahlar
                .Select(g => new SelectListItem 
                { 
                    Value = g.GuzergahID.ToString(), 
                    Text = g.Nereden + " - " + g.Nereye 
                })
                .ToListAsync();

            ViewBag.BaslangicTarihi = baslangicTarihi.Value.ToString("yyyy-MM-dd");
            ViewBag.BitisTarihi = bitisTarihi.Value.ToString("yyyy-MM-dd");

            return View(dashboardData);
        }

        public async Task<IActionResult> SatisRaporu(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            ViewData["Title"] = "Satış Raporu";
            
            if (!baslangicTarihi.HasValue)
                baslangicTarihi = DateTime.Now.AddDays(-15);
            if (!bitisTarihi.HasValue)
                bitisTarihi = DateTime.Now.AddDays(15);

            var satisRaporu = await _raporService.GetSatisRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
            
            ViewBag.Guzergahlar = await _context.Guzergahlar
                .Select(g => new SelectListItem 
                { 
                    Value = g.GuzergahID.ToString(), 
                    Text = g.Nereden + " - " + g.Nereye 
                })
                .ToListAsync();

            // Seçili güzergah adını bul
            if (guzergahId.HasValue)
            {
                var seciliGuzergah = await _context.Guzergahlar
                    .Where(g => g.GuzergahID == guzergahId.Value)
                    .Select(g => g.Nereden + " - " + g.Nereye)
                    .FirstOrDefaultAsync();
                ViewBag.SeciliGuzergahAdi = seciliGuzergah;
            }
            else
            {
                ViewBag.SeciliGuzergahAdi = null;
            }

            ViewBag.BaslangicTarihi = baslangicTarihi.Value.ToString("dd.MM.yyyy");
            ViewBag.BitisTarihi = bitisTarihi.Value.ToString("dd.MM.yyyy");
            ViewBag.SeciliGuzergah = guzergahId;

            return View(satisRaporu);
        }

        public async Task<IActionResult> GelirRaporu(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            ViewData["Title"] = "Gelir Raporu";
            
            if (!baslangicTarihi.HasValue)
                baslangicTarihi = DateTime.Now.AddDays(-15);
            if (!bitisTarihi.HasValue)
                bitisTarihi = DateTime.Now.AddDays(15);

            var gelirRaporu = await _raporService.GetGelirRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
            
            ViewBag.Guzergahlar = await _context.Guzergahlar
                .Select(g => new SelectListItem 
                { 
                    Value = g.GuzergahID.ToString(), 
                    Text = g.Nereden + " - " + g.Nereye 
                })
                .ToListAsync();

            // Seçili güzergah adını bul
            if (guzergahId.HasValue)
            {
                var seciliGuzergah = await _context.Guzergahlar
                    .Where(g => g.GuzergahID == guzergahId.Value)
                    .Select(g => g.Nereden + " - " + g.Nereye)
                    .FirstOrDefaultAsync();
                ViewBag.SeciliGuzergahAdi = seciliGuzergah;
            }
            else
            {
                ViewBag.SeciliGuzergahAdi = null;
            }

            ViewBag.BaslangicTarihi = baslangicTarihi.Value.ToString("dd.MM.yyyy");
            ViewBag.BitisTarihi = bitisTarihi.Value.ToString("dd.MM.yyyy");
            ViewBag.SeciliGuzergah = guzergahId;

            return View(gelirRaporu);
        }

        public async Task<IActionResult> SeferPerformans(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            ViewData["Title"] = "Sefer Performansı";
            
            if (!baslangicTarihi.HasValue)
                baslangicTarihi = DateTime.Now.AddDays(-30);
            if (!bitisTarihi.HasValue)
                bitisTarihi = DateTime.Now;

            var seferPerformans = await _raporService.GetSeferPerformansAsync(baslangicTarihi, bitisTarihi, guzergahId);
            
            ViewBag.Guzergahlar = await _context.Guzergahlar
                .Select(g => new SelectListItem 
                { 
                    Value = g.GuzergahID.ToString(), 
                    Text = g.Nereden + " - " + g.Nereye 
                })
                .ToListAsync();

            ViewBag.BaslangicTarihi = baslangicTarihi.Value.ToString("yyyy-MM-dd");
            ViewBag.BitisTarihi = bitisTarihi.Value.ToString("yyyy-MM-dd");
            ViewBag.SeciliGuzergah = guzergahId;

            return View(seferPerformans);
        }

        public async Task<IActionResult> MusteriAnalizi(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            ViewData["Title"] = "Müşteri Analizi";
            
            if (!baslangicTarihi.HasValue)
                baslangicTarihi = DateTime.Now.AddDays(-30);
            if (!bitisTarihi.HasValue)
                bitisTarihi = DateTime.Now;

            var musteriAnalizi = await _raporService.GetMusteriAnaliziAsync(baslangicTarihi, bitisTarihi);
            
            ViewBag.BaslangicTarihi = baslangicTarihi.Value.ToString("yyyy-MM-dd");
            ViewBag.BitisTarihi = bitisTarihi.Value.ToString("yyyy-MM-dd");

            return View(musteriAnalizi);
        }

        public async Task<IActionResult> IptalIadeRaporu(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            ViewData["Title"] = "İptal ve İade Raporu";
            
            if (!baslangicTarihi.HasValue)
                baslangicTarihi = DateTime.Now.AddDays(-30);
            if (!bitisTarihi.HasValue)
                bitisTarihi = DateTime.Now;

            var iptalIadeRaporu = await _raporService.GetIptalIadeRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
            
            ViewBag.Guzergahlar = await _context.Guzergahlar
                .Select(g => new SelectListItem 
                { 
                    Value = g.GuzergahID.ToString(), 
                    Text = g.Nereden + " - " + g.Nereye 
                })
                .ToListAsync();

            ViewBag.BaslangicTarihi = baslangicTarihi.Value.ToString("yyyy-MM-dd");
            ViewBag.BitisTarihi = bitisTarihi.Value.ToString("yyyy-MM-dd");
            ViewBag.SeciliGuzergah = guzergahId;

            return View(iptalIadeRaporu);
        }

        #region API Endpoints

        [HttpGet]
        public async Task<IActionResult> GetDashboardData(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null)
        {
            try
            {
                var data = await _raporService.GetDashboardDataAsync(baslangicTarihi, bitisTarihi);
                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSatisRaporuData(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            try
            {
                var data = await _raporService.GetSatisRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGelirRaporuData(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            if (!baslangicTarihi.HasValue)
                baslangicTarihi = new DateTime(2025, 6, 1);
            if (!bitisTarihi.HasValue)
                bitisTarihi = new DateTime(2025, 7, 31);

            var gelirRaporu = await _raporService.GetGelirRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
            return Json(gelirRaporu);
        }

        [HttpGet]
        public async Task<IActionResult> GetSaatlikSatisData(DateTime tarih, int? guzergahId = null)
        {
            try
            {
                var saatlikSatis = await _raporService.GetSaatlikSatisAsync(tarih, guzergahId);
                return Json(new { success = true, data = saatlikSatis });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Saatlik satış verileri alınamadı: " + ex.Message });
            }
        }

        #endregion

        #region Export Methods

        [HttpPost]
        public async Task<IActionResult> ExportToJson(string raporTipi, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            try
            {
                object data = null;
                string fileName = "";

                switch (raporTipi.ToLower())
                {
                    case "dashboard":
                        data = await _raporService.GetDashboardDataAsync(baslangicTarihi, bitisTarihi);
                        fileName = "dashboard-raporu.json";
                        break;
                    case "satis":
                        data = await _raporService.GetSatisRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
                        fileName = "satis-raporu.json";
                        break;
                    case "gelir":
                        data = await _raporService.GetGelirRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
                        fileName = "gelir-raporu.json";
                        break;
                    case "sefer":
                        data = await _raporService.GetSeferPerformansAsync(baslangicTarihi, bitisTarihi, guzergahId);
                        fileName = "sefer-performans-raporu.json";
                        break;
                    case "musteri":
                        data = await _raporService.GetMusteriAnaliziAsync(baslangicTarihi, bitisTarihi);
                        fileName = "musteri-analizi-raporu.json";
                        break;
                    case "iptal":
                        data = await _raporService.GetIptalIadeRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
                        fileName = "iptal-iade-raporu.json";
                        break;
                    default:
                        return BadRequest("Geçersiz rapor tipi");
                }

                var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = null
                });
                
                return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportToExcel(string raporTipi, DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null)
        {
            try
            {
                object data = null;
                string fileName = "";
                string sheetName = "";

                switch (raporTipi.ToLower())
                {
                    case "dashboard":
                        data = await _raporService.GetDashboardDataAsync(baslangicTarihi, bitisTarihi);
                        fileName = "dashboard-raporu.xlsx";
                        sheetName = "Dashboard";
                        break;
                    case "satis":
                        data = await _raporService.GetSatisRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
                        fileName = "satis-raporu.xlsx";
                        sheetName = "Satış Raporu";
                        break;
                    case "gelir":
                        data = await _raporService.GetGelirRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
                        fileName = "gelir-raporu.xlsx";
                        sheetName = "Gelir Raporu";
                        break;
                    case "sefer":
                        data = await _raporService.GetSeferPerformansAsync(baslangicTarihi, bitisTarihi, guzergahId);
                        fileName = "sefer-performans-raporu.xlsx";
                        sheetName = "Sefer Performans";
                        break;
                    case "musteri":
                        data = await _raporService.GetMusteriAnaliziAsync(baslangicTarihi, bitisTarihi);
                        fileName = "musteri-analizi-raporu.xlsx";
                        sheetName = "Müşteri Analizi";
                        break;
                    case "iptal":
                        data = await _raporService.GetIptalIadeRaporuAsync(baslangicTarihi, bitisTarihi, guzergahId);
                        fileName = "iptal-iade-raporu.xlsx";
                        sheetName = "İptal İade Raporu";
                        break;
                    default:
                        return BadRequest("Geçersiz rapor tipi");
                }

                // Excel uyumlu CSV format oluştur (UTF-8 BOM ile)
                var csvContent = GenerateCSVContent(data, raporTipi);
                
                // UTF-8 BOM ekle (Excel için Türkçe karakter desteği)
                var bom = new byte[] { 0xEF, 0xBB, 0xBF };
                var contentBytes = System.Text.Encoding.UTF8.GetBytes(csvContent);
                var bytes = new byte[bom.Length + contentBytes.Length];
                Array.Copy(bom, 0, bytes, 0, bom.Length);
                Array.Copy(contentBytes, 0, bytes, bom.Length, contentBytes.Length);
                
                return File(bytes, "text/csv; charset=utf-8", fileName.Replace(".xlsx", ".csv"));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        private string GenerateCSVContent(object data, string raporTipi)
        {
            var csv = new System.Text.StringBuilder();
            
            try
            {
                switch (raporTipi.ToLower())
                {
                    case "satis":
                        if (data is SatisRaporuViewModel satisData)
                        {
                            csv.AppendLine("SATIŞ RAPORU");
                            csv.AppendLine($"Rapor Tarihi;{DateTime.Now:dd.MM.yyyy HH:mm}");
                            csv.AppendLine("");
                            csv.AppendLine("ÖZET BİLGİLER");
                            csv.AppendLine($"Toplam Satış;{satisData.ToplamSatis}");
                            csv.AppendLine($"Günlük Ortalama;{satisData.GunlukOrtalama:F1}");
                            csv.AppendLine($"Haftalık Satış;{satisData.HaftalikSatis}");
                            csv.AppendLine($"Aylık Satış;{satisData.AylikSatis}");
                            csv.AppendLine($"Artış Oranı;%{satisData.ArtisOrani:F1}");
                            csv.AppendLine("");
                            
                            if (satisData.GuzergahBazliSatis?.Any() == true)
                            {
                                csv.AppendLine("GÜZERGAH BAZLI SATIŞ");
                                csv.AppendLine("Güzergah;Bilet Sayısı;Oran (%)");
                                foreach (var item in satisData.GuzergahBazliSatis)
                                {
                                    csv.AppendLine($"{item.GuzergahAdi};{item.BiletSayisi};{item.YuzdeOrani:F1}");
                                }
                            }
                        }
                        break;
                        
                    case "gelir":
                        if (data is GelirRaporuViewModel gelirData)
                        {
                            csv.AppendLine("GELİR RAPORU");
                            csv.AppendLine($"Rapor Tarihi;{DateTime.Now:dd.MM.yyyy HH:mm}");
                            csv.AppendLine("");
                            csv.AppendLine("ÖZET BİLGİLER");
                            csv.AppendLine($"Toplam Gelir;{gelirData.ToplamGelir:N2} TL");
                            csv.AppendLine($"Günlük Gelir;{gelirData.GunlukGelir:N2} TL");
                            csv.AppendLine($"Haftalık Gelir;{gelirData.HaftalikGelir:N2} TL");
                            csv.AppendLine($"Aylık Gelir;{gelirData.AylikGelir:N2} TL");
                            csv.AppendLine($"Gelir Artış Oranı;%{gelirData.GelirArtisOrani:F1}");
                            csv.AppendLine("");
                            
                            if (gelirData.SeferBazliGelir?.Any() == true)
                            {
                                csv.AppendLine("SEFER BAZLI GELİR (İLK 10)");
                                csv.AppendLine("Sefer;Tarih;Gelir (TL);Bilet Sayısı;Doluluk Oranı (%)");
                                foreach (var item in gelirData.SeferBazliGelir.Take(10))
                                {
                                    csv.AppendLine($"{item.SeferAdi};{item.SeferTarihi:dd.MM.yyyy};{item.Gelir:N2};{item.BiletSayisi};{item.DolulukOrani:F1}");
                                }
                            }
                        }
                        break;
                        
                    case "musteri":
                        if (data is MusteriAnaliziViewModel musteriData)
                        {
                            csv.AppendLine("MÜŞTERİ ANALİZİ RAPORU");
                            csv.AppendLine($"Rapor Tarihi;{DateTime.Now:dd.MM.yyyy HH:mm}");
                            csv.AppendLine("");
                            csv.AppendLine("ÖZET BİLGİLER");
                            csv.AppendLine($"Toplam Müşteri;{musteriData.ToplamMusteriSayisi}");
                            csv.AppendLine($"Yeni Müşteri;{musteriData.YeniMusteriSayisi}");
                            csv.AppendLine($"Aktif Müşteri;{musteriData.AktifMusteriSayisi}");
                            csv.AppendLine($"Sık Bilet Alan Oranı;%{musteriData.SikBiletAlanOrani:F1}");
                            csv.AppendLine("");
                            
                            if (musteriData.EnCokBiletAlanMusteri?.Any() == true)
                            {
                                csv.AppendLine("EN ÇOK BİLET ALAN MÜŞTERİLER");
                                csv.AppendLine("Müşteri ID;Bilet Sayısı;Toplam Harcama (TL)");
                                foreach (var item in musteriData.EnCokBiletAlanMusteri)
                                {
                                    csv.AppendLine($"{item.YolcuID};{item.BiletSayisi};{item.ToplamHarcama:N2}");
                                }
                            }
                        }
                        break;
                        
                    case "sefer":
                        if (data is SeferPerformansViewModel seferData)
                        {
                            csv.AppendLine("SEFER PERFORMANSI RAPORU");
                            csv.AppendLine($"Rapor Tarihi;{DateTime.Now:dd.MM.yyyy HH:mm}");
                            csv.AppendLine("");
                            csv.AppendLine("ÖZET BİLGİLER");
                            csv.AppendLine($"Aktif Sefer Sayısı;{seferData.AktifSeferSayisi}");
                            csv.AppendLine($"Tamamlanan Sefer;{seferData.TamamlananSeferSayisi}");
                            csv.AppendLine($"İptal Edilen Sefer;{seferData.IptalEdilenSeferSayisi}");
                            csv.AppendLine($"Ortalama Doluluk Oranı;%{seferData.OrtalamaDolulukOrani:F1}");
                            csv.AppendLine($"Sefer İptal Oranı;%{seferData.SeferIptalOrani:F1}");
                            csv.AppendLine("");
                            
                            if (seferData.SeferDoluluklari?.Any() == true)
                            {
                                csv.AppendLine("SEFER DOLULUK DETAYLARI");
                                csv.AppendLine("Sefer Adı;Tarih;Toplam Koltuk;Satılan Koltuk;Doluluk (%)");
                                foreach (var item in seferData.SeferDoluluklari.Take(20))
                                {
                                    csv.AppendLine($"{item.SeferAdi};{item.SeferTarihi:dd.MM.yyyy HH:mm};{item.ToplamKoltuk};{item.SatilanKoltuk};{item.DolulukOrani:F1}");
                                }
                            }
                            
                            if (seferData.GuzergahDoluluklari?.Any() == true)
                            {
                                csv.AppendLine("");
                                csv.AppendLine("GÜZERGAH PERFORMANSI");
                                csv.AppendLine("Güzergah;Ortalama Doluluk (%);Toplam Sefer");
                                foreach (var item in seferData.GuzergahDoluluklari.Take(10))
                                {
                                    csv.AppendLine($"{item.GuzergahAdi};{item.OrtalamaDoluluk:F1};{item.ToplamSefer}");
                                }
                            }
                        }
                        break;
                        
                    case "iptal":
                        if (data is IptalIadeRaporuViewModel iptalData)
                        {
                            csv.AppendLine("İPTAL VE İADE RAPORU");
                            csv.AppendLine($"Rapor Tarihi;{DateTime.Now:dd.MM.yyyy HH:mm}");
                            csv.AppendLine("");
                            csv.AppendLine("ÖZET BİLGİLER");
                            csv.AppendLine($"İptal Edilen Bilet;{iptalData.IptalEdilenBiletSayisi}");
                            csv.AppendLine($"İptal Oranı;%{iptalData.IptalOrani:F1}");
                            csv.AppendLine($"Toplam İade Tutarı;{iptalData.ToplamIadeTutari:N2} TL");
                            csv.AppendLine("");
                            
                            csv.AppendLine("DURUM DEĞERLENDİRMESİ");
                            if (iptalData.IptalOrani <= 5)
                                csv.AppendLine("İptal Oranı Durumu;Mükemmel");
                            else if (iptalData.IptalOrani <= 10)
                                csv.AppendLine("İptal Oranı Durumu;İyi");
                            else if (iptalData.IptalOrani <= 15)
                                csv.AppendLine("İptal Oranı Durumu;Orta");
                            else
                                csv.AppendLine("İptal Oranı Durumu;Yüksek - İnceleme Gerekli");
                        }
                        break;
                        
                    default:
                        csv.AppendLine("RAPOR VERİSİ");
                        csv.AppendLine($"Rapor Tarihi;{DateTime.Now:dd.MM.yyyy HH:mm}");
                        csv.AppendLine($"Rapor Tipi;{raporTipi}");
                        break;
                }
            }
            catch (Exception ex)
            {
                csv.AppendLine($"Hata oluştu: {ex.Message}");
            }
            
            return csv.ToString();
        }

        #endregion
    }
} 
 