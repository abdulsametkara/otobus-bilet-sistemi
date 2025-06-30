using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtobusBiletSistemi.Core.Entities
{
    public class Yolcu
    {
        public int YolcuID { get; set; }
        public string TCNo { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public string TelNo { get; set; }
        public string Email { get; set; }
        
        public string FullName => $"{Ad} {Soyad}";
    }
}
