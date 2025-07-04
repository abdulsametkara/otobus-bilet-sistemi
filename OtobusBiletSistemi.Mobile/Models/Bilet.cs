using System;

namespace OtobusBiletSistemi.Mobile.Models
{
    public class Bilet
    {
        public int BiletID { get; set; }
        public int SeferID { get; set; }
        public int KoltukID { get; set; }
        public int YolcuID { get; set; }
        public int? OdemeID { get; set; }
        public DateTime? BiletTarihi { get; set; }
        public string? BiletDurumu { get; set; }
        
        public string BiletNo => $"BLT-{BiletID:D7}";

        public Sefer? Sefer { get; set; }
        public Koltuk? Koltuk { get; set; }
        public YolcuUser? Yolcu { get; set; }
        public Odeme? Odeme { get; set; }
    }
}
