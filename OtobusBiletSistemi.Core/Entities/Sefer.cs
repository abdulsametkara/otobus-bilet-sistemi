using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtobusBiletSistemi.Core.Entities
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

        // Navigation Properties
        public virtual Otobus? Otobus { get; set; }
        public virtual Guzergah? Guzergah { get; set; }
        public virtual ICollection<Bilet>? Biletler { get; set; }
    }
}
