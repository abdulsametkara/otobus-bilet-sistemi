using OtobusBiletSistemi.Core.Entities;

namespace OtobusBiletSistemi.Web.Models
{
    public class SeferDetayViewModel
    {
        public Sefer? Sefer { get; set; }
        public Otobus? Otobus { get; set; }
        public Guzergah? Guzergah { get; set; }
        public int ToplamKoltukSayisi { get; set; }
        public int DoluKoltukSayisi { get; set; }
        public int BosKoltukSayisi => ToplamKoltukSayisi - DoluKoltukSayisi;
        public decimal BiletFiyati { get; set; } = 120; // Şimdilik sabit, sonra dinamik yapacağız
        
        public string KalkisSaati => Sefer?.Saat ?? "";
        public string SeferTarihi => Sefer?.Tarih.ToShortDateString() ?? "";
    }
} 
 
 
 
 