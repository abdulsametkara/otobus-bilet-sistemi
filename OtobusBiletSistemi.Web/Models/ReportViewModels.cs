using System.ComponentModel.DataAnnotations;

namespace OtobusBiletSistemi.Web.Models
{
    public class DashboardViewModel
    {
        // Satış Özeti
        public int ToplamSatis { get; set; }
        public int BuAySatis { get; set; }
        
        // Gelir Özeti
        public decimal ToplamGelir { get; set; }
        public decimal BuAyGelir { get; set; }
        
        // Sefer Performansı
        public decimal OrtalamaDoluluk { get; set; }
        public int AktifSeferSayisi { get; set; }
        
        // Müşteri Analizi
        public int ToplamMusteriSayisi { get; set; }
        public int YeniMusteriler { get; set; }
        
        // İptal/İade
        public int IptalSayisi { get; set; }
        public decimal IptalOrani { get; set; }
        public decimal IadeTutari { get; set; }
        public decimal IadeOrani { get; set; }
        
        // Detaylı Raporlar
        public SatisRaporuViewModel SatisRaporu { get; set; } = new();
        public GelirRaporuViewModel GelirRaporu { get; set; } = new();
        public SeferPerformansViewModel SeferPerformans { get; set; } = new();
        public MusteriAnaliziViewModel MusteriAnalizi { get; set; } = new();
        public IptalIadeRaporuViewModel IptalIadeRaporu { get; set; } = new();
        
        public DateTime BaslangicTarihi { get; set; } = DateTime.Today.AddDays(-30);
        public DateTime BitisTarihi { get; set; } = DateTime.Today;
    }

    public class SatisRaporuViewModel
    {
        public int ToplamSatis { get; set; }
        public int ToplamBiletSatisi { get; set; }
        public int GunlukSatis { get; set; }
        public decimal GunlukOrtalama { get; set; }
        public int HaftalikSatis { get; set; }
        public decimal HaftalikOrtalama { get; set; }
        public int AylikSatis { get; set; }
        public decimal ArtisOrani { get; set; }
        public decimal SatisArtisOrani { get; set; }
        public decimal OrtalamaBiletFiyati { get; set; }
        public List<GuzergahSatisModel> GuzergahBazliSatis { get; set; } = new();
        public List<TrendModel> SatisTrendi { get; set; } = new();
        public List<ZamanBazliSatisModel> SaatlikSatis { get; set; } = new();
    }

    public class GelirRaporuViewModel
    {
        public decimal ToplamGelir { get; set; }
        public decimal GunlukGelir { get; set; }
        public decimal HaftalikGelir { get; set; }
        public decimal AylikGelir { get; set; }
        public decimal OrtalamaBiletFiyati { get; set; }
        public decimal GelirArtisOrani { get; set; }
        public List<SeferGelirModel> SeferBazliGelir { get; set; } = new();
        public List<TrendModel> GelirTrendi { get; set; } = new();
    }

    public class SeferPerformansViewModel
    {
        public int AktifSeferSayisi { get; set; }
        public int TamamlananSeferSayisi { get; set; }
        public int IptalEdilenSeferSayisi { get; set; }
        public decimal OrtalamaDolulukOrani { get; set; }
        public decimal SeferIptalOrani { get; set; }
        public List<SeferDolulukModel> SeferDoluluklari { get; set; } = new();
        public List<GuzergahDolulukModel> GuzergahDoluluklari { get; set; } = new();
    }

    public class MusteriAnaliziViewModel
    {
        public int ToplamMusteriSayisi { get; set; }
        public int YeniMusteriSayisi { get; set; }
        public int AktifMusteriSayisi { get; set; }
        public decimal SikBiletAlanOrani { get; set; }
        public List<TopMusteriModel> EnCokBiletAlanMusteri { get; set; } = new();
        public List<TrendModel> MusteriArtisiTrendi { get; set; } = new();
    }

    public class IptalIadeRaporuViewModel
    {
        public int IptalEdilenBiletSayisi { get; set; }
        public decimal IptalOrani { get; set; }
        public decimal ToplamIadeTutari { get; set; }
        public List<IptalNedenModel> IptalNedenleri { get; set; } = new();
        public List<TrendModel> IptalTrendi { get; set; } = new();
    }

    // Yardımcı Modeller
    public class GuzergahSatisModel
    {
        public string GuzergahAdi { get; set; } = string.Empty;
        public int BiletSayisi { get; set; }
        public int SatisSayisi { get; set; }
        public decimal Gelir { get; set; }
        public decimal Oran { get; set; }
        public decimal YuzdeOrani { get; set; }
    }

    public class TrendModel
    {
        public DateTime Tarih { get; set; }
        public decimal Deger { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class ZamanBazliSatisModel
    {
        public int Saat { get; set; }
        public int BiletSayisi { get; set; }
        public decimal Gelir { get; set; }
    }

    public class SeferGelirModel
    {
        public int SeferID { get; set; }
        public string SeferAdi { get; set; } = string.Empty;
        public DateTime SeferTarihi { get; set; }
        public decimal Gelir { get; set; }
        public int BiletSayisi { get; set; }
        public decimal DolulukOrani { get; set; }
    }

    public class SeferDolulukModel
    {
        public int SeferID { get; set; }
        public string SeferAdi { get; set; } = string.Empty;
        public DateTime SeferTarihi { get; set; }
        public int ToplamKoltuk { get; set; }
        public int SatilanKoltuk { get; set; }
        public decimal DolulukOrani { get; set; }
    }

    public class GuzergahDolulukModel
    {
        public int GuzergahID { get; set; }
        public string GuzergahAdi { get; set; } = string.Empty;
        public decimal OrtalamaDoluluk { get; set; }
        public int ToplamSefer { get; set; }
    }

    public class TopMusteriModel
    {
        public int YolcuID { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int BiletSayisi { get; set; }
        public decimal ToplamHarcama { get; set; }
    }

    public class IptalNedenModel
    {
        public string Neden { get; set; } = string.Empty;
        public int Sayi { get; set; }
        public decimal Oran { get; set; }
    }

    public class RaporFiltresiModel
    {
        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BaslangicTarihi { get; set; } = DateTime.Today.AddDays(-30);

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BitisTarihi { get; set; } = DateTime.Today;

        [Display(Name = "Güzergah")]
        public int? GuzergahID { get; set; }

        [Display(Name = "Rapor Türü")]
        public string RaporTuru { get; set; } = "Tümü";
    }

    public class ExcelExportModel
    {
        public string RaporAdi { get; set; } = string.Empty;
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
        public List<Dictionary<string, object>> Data { get; set; } = new();
    }
} 