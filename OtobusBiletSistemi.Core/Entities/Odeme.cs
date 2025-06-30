using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtobusBiletSistemi.Core.Entities
{
    public class Odeme
    {
        public int OdemeID { get; set; }
        public int YolcuID { get; set; }
        public DateTime OdemeTarihi { get; set; }
        public decimal OdemeTutari { get; set; }
        public string OdemeYontemi { get; set; } = "";
        public string OdemeDurumu { get; set; } = "";
        public string KartSahibiAdi { get; set; } = "";
        public string KartNumarasi { get; set; } = "";
        public int BiletSayisi { get; set; }
        
        // Backward compatibility
        public decimal ToplamTutar 
        { 
            get => OdemeTutari; 
            set => OdemeTutari = value; 
        }
        public string OdemeTipi 
        { 
            get => OdemeYontemi; 
            set => OdemeYontemi = value; 
        }
        public string OnayDurumu 
        { 
            get => OdemeDurumu; 
            set => OdemeDurumu = value; 
        }
    }
}
