using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtobusBiletSistemi.Mobile.Models
{
    public class Koltuk
    {
        public int KoltukID { get; set; }
        public int OtobusID { get; set; }
        public string KoltukNo { get; set; } = string.Empty;
        
        // Mobile için ek özellikler
        public bool Dolu { get; set; }
        public string Durum => Dolu ? "Dolu" : "Boş";
        public bool SeciliMi { get; set; } // UI için
    }
}
