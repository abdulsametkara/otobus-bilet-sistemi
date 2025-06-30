using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtobusBiletSistemi.Core.Entities
{
    public class Guzergah
    {
        public int GuzergahID { get; set; }
        public string Nereden { get; set; }
        public string Nereye { get; set; }  
        public int Mesafe { get; set; }
    }
}
