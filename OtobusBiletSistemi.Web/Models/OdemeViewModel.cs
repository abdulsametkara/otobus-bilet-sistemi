using System.ComponentModel.DataAnnotations;
using OtobusBiletSistemi.Core.Entities;

namespace OtobusBiletSistemi.Web.Models
{
    public class OdemeViewModel
    {
        // Sefer ve rezervasyon bilgileri
        public Sefer? Sefer { get; set; }
        public Otobus? Otobus { get; set; }
        public Guzergah? Guzergah { get; set; }
        public List<Koltuk> Koltuklar { get; set; } = new List<Koltuk>();
        public List<YolcuBilgisiModel> YolcuBilgileri { get; set; } = new List<YolcuBilgisiModel>();
        public decimal BiletFiyati { get; set; }

        // Ödeme bilgileri
        [Required(ErrorMessage = "Kart üzerindeki isim zorunludur")]
        [StringLength(100, ErrorMessage = "İsim en fazla 100 karakter olabilir")]
        [Display(Name = "Kart Üzerindeki İsim")]
        public string KartSahibiAdi { get; set; } = "";

        [Required(ErrorMessage = "Kart numarası zorunludur")]
        [Display(Name = "Kart Numarası")]
        public string KartNumarasi { get; set; } = "";
        
        // Kart numarası temizleme (boşlukları kaldır)
        public string CleanKartNumarasi => KartNumarasi?.Replace(" ", "").Replace("-", "") ?? "";

        [Required(ErrorMessage = "Son kullanma tarihi zorunludur")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "MM/YY formatında giriniz")]
        [Display(Name = "Son Kullanma Tarihi")]
        public string SonKullanmaTarihi { get; set; } = "";

        [Required(ErrorMessage = "CVV zorunludur")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV 3 haneli olmalıdır")]
        [Display(Name = "CVV")]
        public string CVV { get; set; } = "";

        // İletişim bilgileri
        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [RegularExpression(@"^0[5][0-9]{9}$", ErrorMessage = "05xxxxxxxxx formatında 11 haneli cep telefonu numarası giriniz")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = "";

        // Hesaplanan değerler
        public decimal ToplamFiyat => YolcuBilgileri.Count * BiletFiyati;
        public decimal KDV => 0m; // KDV kaldırıldı
        public decimal ToplamOdeme => ToplamFiyat; // KDV olmadan toplam
        
        public string SeferTarihi => Sefer?.Tarih.ToString("dd.MM.yyyy") ?? "";
        public string KalkisSaati => Sefer?.Saat ?? "";
        
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


 
 
 
 