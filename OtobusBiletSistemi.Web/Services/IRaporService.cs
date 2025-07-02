using OtobusBiletSistemi.Web.Models;

namespace OtobusBiletSistemi.Web.Services
{
    public interface IRaporService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        Task<SatisRaporuViewModel> GetSatisRaporuAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null);
        Task<GelirRaporuViewModel> GetGelirRaporuAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null);
        Task<SeferPerformansViewModel> GetSeferPerformansAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null);
        Task<MusteriAnaliziViewModel> GetMusteriAnaliziAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null);
        Task<IptalIadeRaporuViewModel> GetIptalIadeRaporuAsync(DateTime? baslangicTarihi = null, DateTime? bitisTarihi = null, int? guzergahId = null);
        Task<List<ZamanBazliSatisModel>> GetSaatlikSatisAsync(DateTime tarih, int? guzergahId = null);
    }

    public interface IExcelExportService
    {
        Task<byte[]> ExportSatisRaporuAsync(SatisRaporuViewModel model);
        Task<byte[]> ExportGelirRaporuAsync(GelirRaporuViewModel model);
        Task<byte[]> ExportSeferPerformansAsync(SeferPerformansViewModel model);
        Task<byte[]> ExportMusteriAnaliziAsync(MusteriAnaliziViewModel model);
        Task<byte[]> ExportIptalIadeRaporuAsync(IptalIadeRaporuViewModel model);
    }

    public interface IPdfExportService
    {
        Task<byte[]> ExportSatisRaporuAsync(SatisRaporuViewModel model);
        Task<byte[]> ExportGelirRaporuAsync(GelirRaporuViewModel model);
        Task<byte[]> ExportSeferPerformansAsync(SeferPerformansViewModel model);
        Task<byte[]> ExportMusteriAnaliziAsync(MusteriAnaliziViewModel model);
        Task<byte[]> ExportIptalIadeRaporuAsync(IptalIadeRaporuViewModel model);
    }

    public interface IEmailReportService
    {
        Task SendWeeklyReportAsync(string recipientEmail);
        Task SendMonthlyReportAsync(string recipientEmail);
        Task SendCustomReportAsync(string recipientEmail, DashboardViewModel dashboardData);
    }
} 