using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtobusBiletSistemi.Core.Entities
{
    public class Otobus
    {
        public int OtobusID { get; set; }
        public string Plaka { get; set; }
        public string OtobusTipi { get; set; }
        public int KoltukSayısı { get; set; }
    }
}
