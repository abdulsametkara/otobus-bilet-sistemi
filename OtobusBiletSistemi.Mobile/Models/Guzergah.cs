using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtobusBiletSistemi.Mobile.Models
{
    public class Guzergah
    {
        public int GuzergahID { get; set; }
        public string KalkisYeri { get; set; } = string.Empty;
        public string VarisYeri { get; set; } = string.Empty;
        public int Mesafe { get; set; }
        
        // Mobile UI için ek özellikler
        public string GuzergahAdi => $"{KalkisYeri} - {VarisYeri}";
        public string MesafeText => $"{Mesafe} km";
        
        // Backward compatibility
        public string Nereden => KalkisYeri;
        public string Nereye => VarisYeri;
    }
}
