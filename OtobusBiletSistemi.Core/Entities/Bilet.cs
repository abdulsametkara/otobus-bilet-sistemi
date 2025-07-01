using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using OtobusBiletSistemi.Core.Data;

namespace OtobusBiletSistemi.Core.Entities
{
    public class Bilet
    {
        public int BiletID { get; set; }
        public int SeferID { get; set; }
        public int KoltukID { get; set; }
        public int YolcuID { get; set; }
        public int? OdemeID { get; set; }
        public DateTime? BiletTarihi { get; set; }
        public string? BiletDurumu { get; set; }
        
        public string BiletNo => $"BLT-{BiletID:D6}";

        // Navigation Properties
        public virtual Sefer? Sefer { get; set; }
        public virtual Koltuk? Koltuk { get; set; }
        public virtual YolcuUser? Yolcu { get; set; }
        public virtual Odeme? Odeme { get; set; }
    }
}
