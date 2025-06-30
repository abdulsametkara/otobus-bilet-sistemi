using OtobusBiletSistemi.Core.Entities;

namespace OtobusBiletSistemi.Web.Models
{
    public class BiletDetayViewModel
    {
        public Bilet? Bilet { get; set; }
        public Sefer? Sefer { get; set; }
        public Koltuk? Koltuk { get; set; }
        public Otobus? Otobus { get; set; }
        public Guzergah? Guzergah { get; set; }

        // Computed properties
        public string BiletDurumuRenk
        {
            get
            {
                return Bilet?.BiletDurumu switch
                {
                    "Aktif" => "success",
                    "İptal" => "danger",
                    "Kullanıldı" => "secondary",
                    _ => "warning"
                };
            }
        }

        public string BiletDurumuIcon
        {
            get
            {
                return Bilet?.BiletDurumu switch
                {
                    "Aktif" => "fas fa-check-circle",
                    "İptal" => "fas fa-times-circle",
                    "Kullanıldı" => "fas fa-history",
                    _ => "fas fa-question-circle"
                };
            }
        }

        public string SeferTarihi => Sefer?.Tarih.ToString("dd.MM.yyyy") ?? "";
        public string SeferSaati => Sefer?.Saat ?? "";
        
        public string KalkisTerminali 
        { 
            get 
            {
                if (Guzergah?.Nereden == null) return "Başlangıç";
                
                return Guzergah.Nereden switch
                {
                    "Ankara" => "Ankara Otogarı",
                    "İstanbul" => "İstanbul Büyük Otogarı",
                    _ => $"{Guzergah.Nereden} Terminali"
                };
            }
        }
        
        public string VarisTerminali 
        { 
            get 
            {
                if (Guzergah?.Nereye == null) return "Varış";
                
                return Guzergah.Nereye switch
                {
                    "Ankara" => "Ankara Otogarı",
                    "İstanbul" => "İstanbul Büyük Otogarı",
                    _ => $"{Guzergah.Nereye} Terminali"
                };
            }
        }
    }
} 