using System;
using System.Collections.Generic;

namespace OtobusBiletSistemi.Mobile.Models
{
    public class Sefer
    {
        public int SeferID { get; set; }
        public int OtobusID { get; set; }
        public int GuzergahID { get; set; }
        public DateTime Tarih { get; set; }
        public string Saat { get; set; } = string.Empty;
        public string Kalkis { get; set; } = string.Empty;
        public string Varis { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }

        // Navigation Properties (Mobile için DTO tarzı)
        public Otobus? Otobus { get; set; }
        public Guzergah? Guzergah { get; set; }
        public List<Bilet>? Biletler { get; set; }
        
        // Mobile için ek özellikler
        public int BosKoltukSayisi { get; set; }
        public bool UygunMu => BosKoltukSayisi > 0;

        // Web tasarımı tarzı formatting property'leri
        public string GuzergahText => Guzergah != null 
            ? $"{Guzergah.Nereden} - {Guzergah.Nereye}"
            : $"{Kalkis} - {Varis}";
        
        public string FormattedTarih => Tarih.ToString("dd.MM.yyyy");
        
        public string FormattedKalkisSaati => Saat;
        
        public string FormattedVarisSaati 
        {
            get
            {
                // Kalkış saatine yaklaşık yolculuk süresi ekle (örnek: 4 saat)
                if (TimeSpan.TryParse(Saat, out var kalkisSaati))
                {
                    var varisSaati = kalkisSaati.Add(TimeSpan.FromHours(4));
                    return varisSaati.ToString(@"hh\:mm");
                }
                return "--:--";
            }
        }
        
        public string FormattedFiyat => $"₺{Fiyat:N2}";
        
        public string BosKoltukText => $"{BosKoltukSayisi} Boş";
        
        public string OtobusInfo => Otobus?.Plaka ?? $"Otobüs #{OtobusID}";

        public string KalkisTerminal => $"{Kalkis}";

        public string VarisTerminal => $"{Varis}";
    }
}
