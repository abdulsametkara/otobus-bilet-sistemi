using System.ComponentModel.DataAnnotations;
using OtobusBiletSistemi.Core.Entities;

namespace OtobusBiletSistemi.Web.Models
{
    public class YolcuBilgileriViewModel
    {
        public Sefer? Sefer { get; set; }
        public Otobus? Otobus { get; set; }
        public Guzergah? Guzergah { get; set; }
        public List<Koltuk> Koltuklar { get; set; } = new List<Koltuk>();
        public List<YolcuBilgisiModel> YolcuBilgileri { get; set; } = new List<YolcuBilgisiModel>();
        public decimal BiletFiyati { get; set; }

        public decimal ToplamFiyat => YolcuBilgileri.Count * BiletFiyati;
        
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

    public class YolcuBilgisiModel
    {
        public int KoltukID { get; set; }
        public string KoltukNo { get; set; } = "";

        [Required(ErrorMessage = "TC Kimlik No zorunludur")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik No 11 haneli olmalıdır")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "TC Kimlik No sadece rakamlardan oluşmalıdır")]
        [Display(Name = "TC Kimlik No")]
        public string TCNo { get; set; } = "";

        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        [RegularExpression(@"^[a-zA-ZçğıöşüÇĞIİÖŞÜ0-9\s]+$", ErrorMessage = "Ad geçersiz karakterler içeriyor")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = "";

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        [RegularExpression(@"^[a-zA-ZçğıöşüÇĞIİÖŞÜ0-9\s]+$", ErrorMessage = "Soyad geçersiz karakterler içeriyor")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = "";

        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [RegularExpression(@"^0[5][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]$", ErrorMessage = "Geçerli bir cep telefonu numarası giriniz (05xxxxxxxxx)")]
        [Display(Name = "Telefon")]
        public string Telefon { get; set; } = "";

        [Required(ErrorMessage = "E-posta zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = "";

        [Display(Name = "Cinsiyet")]
        public string? Cinsiyet { get; set; }

        public string AdSoyad => $"{Ad} {Soyad}".Trim();
    }
} 