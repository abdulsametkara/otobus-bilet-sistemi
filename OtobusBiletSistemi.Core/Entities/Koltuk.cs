using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtobusBiletSistemi.Core.Entities
{
    public class Koltuk
    {
        public int KoltukID { get; set; }
        public int OtobusID { get; set; }
        public string KoltukNo { get; set; }
    }
}
