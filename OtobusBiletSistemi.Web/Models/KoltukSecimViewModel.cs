using OtobusBiletSistemi.Core.Entities;

namespace OtobusBiletSistemi.Web.Models
{
    public class KoltukSecimViewModel
    {
        public Sefer? Sefer { get; set; }
        public Otobus? Otobus { get; set; }
        public Guzergah? Guzergah { get; set; }
        public int YolcuSayisi { get; set; }
        public List<Koltuk> TumKoltuklar { get; set; } = new List<Koltuk>();
        public HashSet<int> DoluKoltukIdleri { get; set; } = new HashSet<int>();
        public decimal BiletFiyati { get; set; }

        // Helper property'ler
        public string SeferTarihi => Sefer?.Tarih.ToString("dd.MM.yyyy") ?? "";
        public string KalkisSaati => Sefer?.Saat ?? "";
        
        public List<Koltuk> BosKoltuklar => TumKoltuklar?.Where(k => !DoluKoltukIdleri.Contains(k.KoltukID)).ToList() ?? new List<Koltuk>();
        public int BosKoltukSayisi => BosKoltuklar.Count;
        
        public decimal ToplamFiyat => BiletFiyati * YolcuSayisi;

        // Koltuk durumunu kontrol et
        public bool IsKoltukDolu(int koltukId)
        {
            return DoluKoltukIdleri.Contains(koltukId);
        }

        // Koltuk gruplarını düzenle (A, B, C sütunları için)
        public List<List<Koltuk>> GetKoltukLayout()
        {
            var layout = new List<List<Koltuk>>();
            
            if (TumKoltuklar == null || !TumKoltuklar.Any())
                return layout;

            // Koltukları sıra numarasına göre grupla
            var siralar = TumKoltuklar
                .Where(k => !string.IsNullOrEmpty(k.KoltukNo))
                .GroupBy(k => GetSiraNo(k.KoltukNo))
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var sira in siralar)
            {
                var siraKoltuklari = sira.OrderBy(k => k.KoltukNo).ToList();
                layout.Add(siraKoltuklari);
            }

            return layout;
        }

        private int GetSiraNo(string koltukNo)
        {
            if (string.IsNullOrEmpty(koltukNo))
                return 0;

            // "1A", "2B" gibi formatlardan sıra numarasını çıkar
            var sayiKismi = "";
            foreach (char c in koltukNo)
            {
                if (char.IsDigit(c))
                    sayiKismi += c;
                else
                    break;
            }

            return int.TryParse(sayiKismi, out int sira) ? sira : 0;
        }
    }
} 
 
 
 
 