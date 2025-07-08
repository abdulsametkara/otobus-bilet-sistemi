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
        // Sefer işlemleri
        Task<List<Models.Sefer>> GetSeferlerAsync();
        Task<Models.Sefer?> GetSeferAsync(int seferID);
        Task<List<Models.Sefer>> SearchSeferlerAsync(string kalkis, string varis, DateTime tarih);
        Task<List<Models.Sefer>> SearchAdvancedSeferlerAsync(string kalkis, string varis, DateTime tarih, int yolcuSayisi);

        // Güzergah işlemleri ve şehir listeleri
        Task<List<Models.Guzergah>> GetGuzergahlarAsync();
        Task<List<string>> GetUniqueKalkisYerleriAsync();
        Task<List<string>> GetUniqueVarisYerleriAsync();
        Task<List<string>> GetPopulerGuzergahlarAsync();

        // Koltuk işlemleri
        Task<List<Models.Koltuk>> GetKoltuklerAsync();
        Task<List<Models.Koltuk>> GetKoltuklarAsync(int seferID);
        Task<Models.Koltuk?> GetKoltukAsync(int koltukID);
        Task<Models.Koltuk?> CreateKoltukAsync(Models.Koltuk koltuk);
        Task<bool> EnsureKoltuklarExistAsync(int otobusID, int koltukSayisi);
        Task<int> GetBosKoltukSayisiAsync(int seferID);
        Task<List<int>> GetDoluKoltukIdleriAsync(int seferID);

        // Bilet işlemleri
        Task<List<Models.Bilet>> GetBiletlerAsync();
        Task<List<Models.Bilet>> GetBiletlerByYolcuAsync(int yolcuID);
        Task<Models.Bilet?> GetBiletAsync(int biletID);
        Task<Models.Bilet?> CreateBiletAsync(Models.Bilet bilet);
        Task<bool> UpdateBiletAsync(Models.Bilet bilet);
        Task<bool> DeleteBiletAsync(int biletID);

        // Yolcu işlemleri
        Task<List<Models.YolcuUser>> GetYolcularAsync();
        Task<Models.YolcuUser?> GetYolcuAsync(int yolcuID);
        Task<Models.YolcuUser?> CreateYolcuAsync(Models.YolcuUser yolcu);

        // Ödeme işlemleri
        Task<Models.Odeme?> CreateOdemeAsync(Models.Odeme odeme);
        Task<Models.Odeme?> GetOdemeAsync(int odemeID);

        // Otobüs işlemleri
        Task<List<Models.Otobus>> GetOtobuslerAsync();
        Task<Models.Otobus?> GetOtobusAsync(int otobusID);

        // İstatistik ve analiz
        Task<List<Models.Guzergah>> GetTrendGuzergahlarAsync();
        Task<List<Models.Sefer>> GetSonArananSeferlerAsync(int limit = 5);

        // Generic API methods for flexible data access
        Task<T?> GetAsync<T>(string endpoint);
        Task<List<T>?> GetListAsync<T>(string endpoint);
    }
}