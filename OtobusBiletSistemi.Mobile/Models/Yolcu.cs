using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtobusBiletSistemi.Mobile.Models
{
    public class YolcuUser
    {
        public int YolcuID { get; set; }
        public string TCNo { get; set; } = string.Empty;
        public string Ad { get; set; } = string.Empty;
        public string Soyad { get; set; } = string.Empty;
        public string TelNo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        public string FullName => $"{Ad} {Soyad}";
        
        // Mobile için ek özellikler
        public string? ProfilResmi { get; set; }
        public bool EmailOnaylandiMi { get; set; }
    }
}
