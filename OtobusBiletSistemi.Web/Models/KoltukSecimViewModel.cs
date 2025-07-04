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
        public int DoluKoltukSayisi => TumKoltuklar.Count - BosKoltukSayisi;
        public double DolulukOrani => TumKoltuklar.Count > 0 ? (double)DoluKoltukSayisi / TumKoltuklar.Count * 100 : 0;
        
        public decimal ToplamFiyat => BiletFiyati * YolcuSayisi;

        // Gelişmiş koltuk bilgileri
        public string DolulukDurumu
        {
            get
            {
                var oran = DolulukOrani;
                return oran switch
                {
                    < 30 => "Az Dolu",
                    < 70 => "Orta Dolu", 
                    < 90 => "Çok Dolu",
                    _ => "Neredeyse Dolu"
                };
            }
        }

        public string DolulukRengi
        {
            get
            {
                var oran = DolulukOrani;
                return oran switch
                {
                    < 30 => "success",
                    < 70 => "warning",
                    < 90 => "danger",
                    _ => "dark"
                };
            }
        }

        // Koltuk durumunu kontrol et
        public bool IsKoltukDolu(int koltukId)
        {
            return DoluKoltukIdleri.Contains(koltukId);
        }

        // Koltuk önerisi al (yan yana koltuklar)
        public List<Koltuk> GetKoltukOnerisi()
        {
            var oneriler = new List<Koltuk>();
            
            if (YolcuSayisi == 1)
            {
                // Tek koltuk için: cam kenarı veya koridora yakın
                var onerilenKoltuklar = BosKoltuklar
                    .Where(k => k.KoltukNo.EndsWith("A") || k.KoltukNo.EndsWith("C"))
                    .Take(3)
                    .ToList();
                
                oneriler.AddRange(onerilenKoltuklar);
            }
            else if (YolcuSayisi == 2)
            {
                // İki koltuk için: yan yana boş koltuklar
                var yanYanaKoltuklar = GetYanYanaKoltuklar(2);
                if (yanYanaKoltuklar.Any())
                {
                    oneriler.AddRange(yanYanaKoltuklar.First());
                }
            }
            else
            {
                // Çoklu koltuk için: aynı sırada veya yakın sıralarda
                var yakınKoltuklar = GetYakınKoltuklar(YolcuSayisi);
                oneriler.AddRange(yakınKoltuklar);
            }

            return oneriler.Take(YolcuSayisi).ToList();
        }

        // Yan yana boş koltukları bul
        public List<List<Koltuk>> GetYanYanaKoltuklar(int sayi)
        {
            var yanYanaGruplar = new List<List<Koltuk>>();
            var layout = GetKoltukLayout();

            foreach (var sira in layout)
            {
                var bosKoltuklar = sira.Where(k => !IsKoltukDolu(k.KoltukID)).ToList();
                
                for (int i = 0; i <= bosKoltuklar.Count - sayi; i++)
                {
                    var grup = bosKoltuklar.Skip(i).Take(sayi).ToList();
                    if (grup.Count == sayi && IsYanYana(grup))
                    {
                        yanYanaGruplar.Add(grup);
                    }
                }
            }

            return yanYanaGruplar.OrderBy(g => GetSiraNo(g.First().KoltukNo)).ToList();
        }

        // Yakın koltukları bul
        private List<Koltuk> GetYakınKoltuklar(int sayi)
        {
            var yakınKoltuklar = new List<Koltuk>();
            var layout = GetKoltukLayout();

            // Önce aynı sırada yakın koltukları ara
            foreach (var sira in layout)
            {
                var bosKoltuklar = sira.Where(k => !IsKoltukDolu(k.KoltukID)).ToList();
                if (bosKoltuklar.Count >= sayi)
                {
                    yakınKoltuklar.AddRange(bosKoltuklar.Take(sayi));
                    break;
                }
            }

            // Yeterli yoksa farklı sıralardan al
            if (yakınKoltuklar.Count < sayi)
            {
                yakınKoltuklar.Clear();
                foreach (var sira in layout)
                {
                    var bosKoltuklar = sira.Where(k => !IsKoltukDolu(k.KoltukID));
                    foreach (var koltuk in bosKoltuklar)
                    {
                        if (yakınKoltuklar.Count < sayi)
                            yakınKoltuklar.Add(koltuk);
                    }
                    if (yakınKoltuklar.Count >= sayi) break;
                }
            }

            return yakınKoltuklar;
        }

        // Koltukların yan yana olup olmadığını kontrol et
        private bool IsYanYana(List<Koltuk> koltuklar)
        {
            if (koltuklar.Count <= 1) return true;

            var harfler = koltuklar.Select(k => k.KoltukNo.Last()).OrderBy(h => h).ToList();
            var sıralar = koltuklar.Select(k => GetSiraNo(k.KoltukNo)).Distinct().ToList();

            // Aynı sırada olmalı
            if (sıralar.Count != 1) return false;

            // Harfler ardışık olmalı (A-B, B-C gibi)
            for (int i = 1; i < harfler.Count; i++)
            {
                if (harfler[i] - harfler[i - 1] != 1)
                    return false;
            }

            return true;
        }

        // Koltuk seçimi validasyonu
        public (bool IsValid, string ErrorMessage) ValidateKoltukSecimi(List<int> secilenKoltukIdleri)
        {
            if (secilenKoltukIdleri == null || !secilenKoltukIdleri.Any())
                return (false, "Lütfen en az bir koltuk seçin.");

            if (secilenKoltukIdleri.Count != YolcuSayisi)
                return (false, $"Lütfen {YolcuSayisi} koltuk seçin.");

            // Seçilen koltukların hepsi mevcut mu?
            var mevcutKoltukIdleri = TumKoltuklar.Select(k => k.KoltukID).ToHashSet();
            var gecersizKoltuklar = secilenKoltukIdleri.Where(id => !mevcutKoltukIdleri.Contains(id)).ToList();
            if (gecersizKoltuklar.Any())
                return (false, "Seçilen koltukların bazıları geçersiz.");

            // Seçilen koltuklar boş mu?
            var doluSecilenKoltuklar = secilenKoltukIdleri.Where(id => DoluKoltukIdleri.Contains(id)).ToList();
            if (doluSecilenKoltuklar.Any())
            {
                var doluKoltukNolari = TumKoltuklar
                    .Where(k => doluSecilenKoltuklar.Contains(k.KoltukID))
                    .Select(k => k.KoltukNo)
                    .ToList();
                return (false, $"Şu koltuklar zaten dolu: {string.Join(", ", doluKoltukNolari)}");
            }

            return (true, "");
        }

        // Koltuk seçimi için öneriler
        public List<KoltukOnerisi> GetKoltukOnerileri()
        {
            var oneriler = new List<KoltukOnerisi>();

            if (YolcuSayisi == 1)
            {
                // Tek yolcu için pencere kenarı öner
                var pencereKoltuklari = BosKoltuklar.Where(k => k.KoltukNo.EndsWith("A") || k.KoltukNo.EndsWith("C")).ToList();
                if (pencereKoltuklari.Any())
                {
                    oneriler.Add(new KoltukOnerisi
                    {
                        Başlık = "Pencere Kenarı",
                        Açıklama = "Manzara için ideal",
                        KoltukIdleri = pencereKoltuklari.Take(3).Select(k => k.KoltukID).ToList(),
                        Öncelik = 1
                    });
                }

                // Koridor kenarı öner
                var koridorKoltuklari = BosKoltuklar.Where(k => k.KoltukNo.EndsWith("B")).ToList();
                if (koridorKoltuklari.Any())
                {
                    oneriler.Add(new KoltukOnerisi
                    {
                        Başlık = "Koridor Kenarı",
                        Açıklama = "Hareket özgürlüğü için ideal",
                        KoltukIdleri = koridorKoltuklari.Take(3).Select(k => k.KoltukID).ToList(),
                        Öncelik = 2
                    });
                }
            }
            else if (YolcuSayisi == 2)
            {
                // İki yolcu için yan yana koltuklar öner
                var yanYanaGruplar = GetYanYanaKoltuklar(2);
                if (yanYanaGruplar.Any())
                {
                    oneriler.Add(new KoltukOnerisi
                    {
                        Başlık = "Yan Yana Koltuklar",
                        Açıklama = "Birlikte seyahat için ideal",
                        KoltukIdleri = yanYanaGruplar.First().Select(k => k.KoltukID).ToList(),
                        Öncelik = 1
                    });
                }
            }

            return oneriler.OrderBy(o => o.Öncelik).ToList();
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

    // Koltuk önerisi sınıfı
    public class KoltukOnerisi
    {
        public string Başlık { get; set; } = "";
        public string Açıklama { get; set; } = "";
        public List<int> KoltukIdleri { get; set; } = new List<int>();
        public int Öncelik { get; set; }
    }
} 
 
 
 
 