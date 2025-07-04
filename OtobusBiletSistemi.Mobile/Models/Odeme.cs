using System;

namespace OtobusBiletSistemi.Mobile.Models
{
    public class Odeme
    {
        public int OdemeID { get; set; }
        public int YolcuID { get; set; }
        public DateTime OdemeTarihi { get; set; }
        public decimal OdemeTutari { get; set; }
        public string OdemeYontemi { get; set; } = string.Empty;
        public string OdemeDurumu { get; set; } = string.Empty;
        public string KartSahibiAdi { get; set; } = string.Empty;
        public string KartNumarasi { get; set; } = string.Empty;
        public int BiletSayisi { get; set; }
        
        // Mobile UI için ek özellikler
        public string FormattedTutar => $"{OdemeTutari:C}";
        public string FormattedTarih => OdemeTarihi.ToString("dd.MM.yyyy HH:mm");
        public bool BasariliMi => OdemeDurumu == "Onaylandı";
        
        // Backward compatibility (Web projesi ile uyumluluk için)
        public decimal ToplamTutar 
        { 
            get => OdemeTutari; 
            set => OdemeTutari = value; 
        }
        public string OdemeTipi 
        { 
            get => OdemeYontemi; 
            set => OdemeYontemi = value; 
        }
        public string OnayDurumu 
        { 
            get => OdemeDurumu; 
            set => OdemeDurumu = value; 
        }
    }
}
