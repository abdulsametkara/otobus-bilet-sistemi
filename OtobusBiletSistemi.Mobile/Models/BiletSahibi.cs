using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OtobusBiletSistemi.Mobile.Models;

public partial class BiletSahibi : ObservableObject
{
    [ObservableProperty]
    private string ad = string.Empty;

    [ObservableProperty]
    private string soyad = string.Empty;

    [ObservableProperty]
    private string tcKimlikNo = string.Empty;

    [ObservableProperty]
    private string telefon = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private DateTime dogumTarihi = DateTime.Now.AddYears(-25);

    [ObservableProperty]
    private string cinsiyet = "Erkek";

    // Validation properties
    public bool IsValid => 
        !string.IsNullOrWhiteSpace(Ad) &&
        !string.IsNullOrWhiteSpace(Soyad) &&
        IsTcKimlikValid();

    public bool IsTcKimlikValid()
    {
        if (string.IsNullOrWhiteSpace(TcKimlikNo) || TcKimlikNo.Length != 11)
            return false;

        if (!TcKimlikNo.All(char.IsDigit))
            return false;

        // TC Kimlik algoritması kontrolü
        int[] digits = TcKimlikNo.Select(c => int.Parse(c.ToString())).ToArray();
        
        // İlk hane 0 olamaz
        if (digits[0] == 0)
            return false;

        // 10. hane kontrolü
        int sum1 = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        int sum2 = digits[1] + digits[3] + digits[5] + digits[7];
        int check10 = ((sum1 * 7) - sum2) % 10;
        
        if (check10 != digits[9])
            return false;

        // 11. hane kontrolü
        int totalSum = digits.Take(10).Sum();
        int check11 = totalSum % 10;
        
        return check11 == digits[10];
    }

    public string TcKimlikHataMesaji
    {
        get
        {
            if (string.IsNullOrWhiteSpace(TcKimlikNo))
                return "TC Kimlik No boş olamaz";
            if (TcKimlikNo.Length != 11)
                return "TC Kimlik No 11 haneli olmalıdır";
            if (!TcKimlikNo.All(char.IsDigit))
                return "TC Kimlik No sadece rakam içermelidir";
            if (!IsTcKimlikValid())
                return "Geçersiz TC Kimlik No";
            return string.Empty;
        }
    }

    // Koltuk bilgisi
    public string KoltukNo { get; set; } = string.Empty;
    public int KoltukId { get; set; }

    // Display properties
    public string TamAd => $"{Ad} {Soyad}";
    public string KisaBilgi => $"{KoltukNo} - {TamAd}";

    partial void OnAdChanged(string value)
    {
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(TamAd));
        OnPropertyChanged(nameof(KisaBilgi));
    }

    partial void OnSoyadChanged(string value)
    {
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(TamAd));
        OnPropertyChanged(nameof(KisaBilgi));
    }

    partial void OnTcKimlikNoChanged(string value)
    {
        OnPropertyChanged(nameof(IsValid));
        OnPropertyChanged(nameof(IsTcKimlikValid));
        OnPropertyChanged(nameof(TcKimlikHataMesaji));
    }

    partial void OnTelefonChanged(string value)
    {
        OnPropertyChanged(nameof(IsValid));
    }

    partial void OnEmailChanged(string value)
    {
        OnPropertyChanged(nameof(IsValid));
    }
} 