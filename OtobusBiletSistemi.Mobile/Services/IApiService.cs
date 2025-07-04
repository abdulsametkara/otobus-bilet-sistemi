using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OtobusBiletSistemi.Mobile.Models;

namespace OtobusBiletSistemi.Mobile.Services
{
    public interface IApiService
    {
        Task<List<Models.Sefer>> GetSeferlerAsync();
        Task<Models.Sefer?> GetSeferAsync(int seferID);
        Task<List<Models.Sefer>> SearchSeferlerAsync(string kalkis, string varis, DateTime tarih);

        Task<List<Models.Koltuk>> GetKoltuklerAsync();
        Task<List<Models.Koltuk>> GetKoltuklarAsync(int seferID);
        Task<Models.Koltuk?> GetKoltukAsync(int koltukID);
        Task<Models.Koltuk?> CreateKoltukAsync(Models.Koltuk koltuk);
        Task<bool> EnsureKoltuklarExistAsync(int otobusID, int koltukSayisi);

        Task<List<Models.Bilet>> GetBiletlerAsync();
        Task<List<Models.Bilet>> GetBiletlerByYolcuAsync(int yolcuID);
        Task<Models.Bilet?> GetBiletAsync(int biletID);
        Task<Models.Bilet?> CreateBiletAsync(Models.Bilet bilet);
        Task<bool> UpdateBiletAsync(Models.Bilet bilet);
        Task<bool> DeleteBiletAsync(int biletID);

        Task<List<Models.YolcuUser>> GetYolcularAsync();
        Task<Models.YolcuUser?> GetYolcuAsync(int yolcuID);
        Task<Models.YolcuUser?> CreateYolcuAsync(Models.YolcuUser yolcu);

        Task<Models.Odeme?> CreateOdemeAsync(Models.Odeme odeme);
        Task<Models.Odeme?> GetOdemeAsync(int odemeID);

        Task<List<Models.Guzergah>> GetGuzergahlarAsync();
        Task<List<Models.Otobus>> GetOtobuslerAsync();
        Task<Models.Otobus?> GetOtobusAsync(int otobusID);

        // Generic API methods for flexible data access
        Task<T?> GetAsync<T>(string endpoint);
        Task<List<T>?> GetListAsync<T>(string endpoint);
    }
}