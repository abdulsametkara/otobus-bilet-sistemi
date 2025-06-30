using System.ComponentModel.DataAnnotations;

namespace OtobusBiletSistemi.Web.Models
{
    public class SeferAramaViewModel
    {
        [Required(ErrorMessage = "Nereden alanı zorunludur")]
        [Display(Name = "Nereden")]
        public string Nereden { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nereye alanı zorunludur")]
        [Display(Name = "Nereye")]
        public string Nereye { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tarih alanı zorunludur")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        public DateTime Tarih { get; set; } = DateTime.Today;

        [Display(Name = "Yolcu Sayısı")]
        [Range(1, 10, ErrorMessage = "Yolcu sayısı 1-10 arasında olmalıdır")]
        public int YolcuSayisi { get; set; } = 1;
    }
} 
 
 
 
 