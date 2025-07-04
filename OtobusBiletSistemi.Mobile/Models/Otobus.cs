using System.Collections.Generic;

namespace OtobusBiletSistemi.Mobile.Models
{
    public class Otobus
    {
        public int OtobusID { get; set; }
        public string Plaka { get; set; } = string.Empty;
        public string OtobusTipi { get; set; } = string.Empty;
        public int KoltukSayısı { get; set; }
        
        // Mobile UI için ek özellikler
        public string OtobusInfo => $"{Plaka} - {OtobusTipi}";
        public List<Koltuk>? Koltuklar { get; set; }
        
        // Koltuk durumu için
        public int BosKoltukSayisi { get; set; }
        public int DoluKoltukSayisi => KoltukSayısı - BosKoltukSayisi;
    }
}
