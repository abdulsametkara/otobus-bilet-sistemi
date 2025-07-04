using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtobusBiletSistemi.Mobile.Models;
using OtobusBiletSistemi.Mobile.Services;
using System.Diagnostics;

namespace OtobusBiletSistemi.Mobile.ViewModels;

public partial class KoltukSecimViewModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private int seferId;

    [ObservableProperty]
    private int yolcuSayisi = 1;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private Sefer? sefer;

    [ObservableProperty]
    private Otobus? otobus;

    [ObservableProperty]
    private Guzergah? guzergah;

    [ObservableProperty]
    private ObservableCollection<KoltukRow> koltukRows = new();

    [ObservableProperty]
    private ObservableCollection<Koltuk> secilenKoltuklar = new();

    [ObservableProperty]
    private List<int> doluKoltuklar = new();

    // Computed Properties
    public bool HasSelectedSeats => SecilenKoltuklar.Count > 0;
    public decimal ToplamFiyat => SecilenKoltuklar.Count * (Sefer?.Fiyat ?? 0);
    public bool CanContinue => SecilenKoltuklar.Count == YolcuSayisi;
    public string DevamButtonText => CanContinue ? "Devam Et" : $"{SecilenKoltuklar.Count}/{YolcuSayisi} Koltuk Seçili";
    public string SelectionHint => YolcuSayisi == 1 ? "Bir koltuk seçmelisiniz" : $"{YolcuSayisi} koltuk seçmelisiniz";
    
    // Header Properties
    public string KalkisYeri => Guzergah?.KalkisYeri ?? "";
    public string VarisYeri => Guzergah?.VarisYeri ?? "";
    public TimeSpan KalkisSaati => TimeSpan.TryParse(Sefer?.Saat, out var time) ? time : TimeSpan.Zero;
    public TimeSpan VarisSaati => KalkisSaati.Add(TimeSpan.FromHours(2)); 
    public DateTime SeferTarihi => Sefer?.Tarih ?? DateTime.Now;

    public KoltukSecimViewModel(IApiService apiService)
    {
        _apiService = apiService;
        Debug.WriteLine("🚌 KoltukSecimViewModel oluşturuldu");
    }

    public async Task LoadKoltukDataAsync(int seferId, int yolcuSayisi)
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            SeferId = seferId;
            YolcuSayisi = yolcuSayisi;
            
            Debug.WriteLine($"🚌 Veri yükleme başlıyor - SeferId: {seferId}, YolcuSayisi: {yolcuSayisi}");

            // 1. Sefer bilgilerini yükle
            await LoadSeferInfo();
            
            // 2. Koltuk düzenini oluştur
            await LoadKoltukLayout();

            Debug.WriteLine("✅ Tüm veriler başarıyla yüklendi");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Veri yükleme hatası: {ex.Message}");
            Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            ErrorMessage = $"Veri yükleme hatası: {ex.Message}";
            
            // Kullanıcıya detaylı hata göster
            await Shell.Current.DisplayAlert("Database Hatası", 
                $"Veritabanından veri alınırken hata oluştu:\n\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Tamam");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadSeferInfo()
    {
        Debug.WriteLine($"🔍 Sefer bilgileri yükleniyor... Aranan SeferID: {SeferId}");
        
        // Sefer bilgisi
        var seferler = await _apiService.GetSeferlerAsync();
        Debug.WriteLine($"📋 Toplam {seferler.Count} sefer alındı");
        
        // İlk 5 seferi listele
        for (int i = 0; i < Math.Min(5, seferler.Count); i++)
        {
            Debug.WriteLine($"   Sefer {i+1}: ID={seferler[i].SeferID}, Güzergah={seferler[i].GuzergahID}");
        }
        
        Sefer = seferler.FirstOrDefault(s => s.SeferID == SeferId);
        
        if (Sefer == null)
        {
            Debug.WriteLine($"❌ Sefer bulunamadı! Aranan ID: {SeferId}");
            Debug.WriteLine($"❌ Mevcut Sefer ID'leri: {string.Join(", ", seferler.Select(s => s.SeferID).Take(10))}");
            throw new Exception($"Sefer bulunamadı (ID: {SeferId})");
        }
        
        Debug.WriteLine($"✅ Sefer bulundu: {Sefer.SeferID} - Fiyat: {Sefer.Fiyat} TL - Saat: {Sefer.Saat} - Tarih: {Sefer.Tarih}");

        // Güzergah bilgisi
        var guzergahlar = await _apiService.GetGuzergahlarAsync();
        Guzergah = guzergahlar.FirstOrDefault(g => g.GuzergahID == Sefer.GuzergahID);
        
        if (Guzergah != null)
        {
            Debug.WriteLine($"✅ Güzergah: {Guzergah.KalkisYeri} → {Guzergah.VarisYeri}");
        }
        else
        {
            Debug.WriteLine($"❌ Güzergah bulunamadı! Aranan ID: {Sefer.GuzergahID}");
        }

        // Otobüs bilgisi
        var otobusler = await _apiService.GetOtobuslerAsync();
        Otobus = otobusler.FirstOrDefault(o => o.OtobusID == Sefer.OtobusID);
        
        if (Otobus != null)
        {
            Debug.WriteLine($"✅ Otobüs: {Otobus.Plaka} (Kapasite: {Otobus.KoltukSayısı})");
            
            // Koltukların mevcut olduğundan emin ol
            Debug.WriteLine($"🔧 Otobüs için koltuklar kontrol ediliyor...");
            bool koltuklarOlusturuldu = await _apiService.EnsureKoltuklarExistAsync(Otobus.OtobusID, Otobus.KoltukSayısı);
            
            if (koltuklarOlusturuldu)
            {
                Debug.WriteLine($"✅ Koltuklar hazır!");
            }
            else
            {
                Debug.WriteLine($"❌ Koltuk oluşturma başarısız!");
            }
        }
        else
        {
            Debug.WriteLine($"❌ Otobüs bulunamadı! Aranan ID: {Sefer.OtobusID}");
        }

        // Header bilgileri için UI güncellemesi
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(KalkisYeri));
            OnPropertyChanged(nameof(VarisYeri));
            OnPropertyChanged(nameof(KalkisSaati));
            OnPropertyChanged(nameof(VarisSaati));
            OnPropertyChanged(nameof(SeferTarihi));
            OnPropertyChanged(nameof(ToplamFiyat));
        });
    }

    private async Task LoadKoltukLayout()
    {
        Debug.WriteLine("🪑 Koltuk düzeni oluşturuluyor...");
        
        if (Otobus == null) 
        {
            Debug.WriteLine("❌ HATA: Otobus bilgisi null! Koltuklar yüklenemez.");
            return;
        }

        Debug.WriteLine($"🚌 Otobüs bilgisi: ID={Otobus.OtobusID}, Plaka={Otobus.Plaka}");

        // Tüm koltukları yükle
        var tumKoltuklar = await _apiService.GetKoltuklerAsync();
        Debug.WriteLine($"📋 Toplam veritabanından {tumKoltuklar.Count} koltuk alındı");
        
        // İlk 10 koltuğu örnek olarak listele
        for (int i = 0; i < Math.Min(10, tumKoltuklar.Count); i++)
        {
            Debug.WriteLine($"   Koltuk {i+1}: ID={tumKoltuklar[i].KoltukID}, OtobusID={tumKoltuklar[i].OtobusID}, No={tumKoltuklar[i].KoltukNo}");
        }
        
        var otobusKoltuklari = tumKoltuklar.Where(k => k.OtobusID == Otobus.OtobusID).ToList();
        
        Debug.WriteLine($"📋 Bu otobüs için {otobusKoltuklari.Count} koltuk bulundu (OtobusID: {Otobus.OtobusID})");
        
        if (otobusKoltuklari.Count == 0)
        {
            Debug.WriteLine($"❌ SORUN: OtobusID {Otobus.OtobusID} için hiç koltuk bulunamadı!");
            Debug.WriteLine($"❌ Veritabanında mevcut otobüs ID'leri: {string.Join(", ", tumKoltuklar.Select(k => k.OtobusID).Distinct().Take(10))}");
        }
        
        // Koltuk numaralarını listele
        foreach (var koltuk in otobusKoltuklari.Take(5))
        {
            Debug.WriteLine($"   🪑 Koltuk: ID={koltuk.KoltukID}, No='{koltuk.KoltukNo}'");
        }

        // Dolu koltukları yükle  
        var tumBiletler = await _apiService.GetBiletlerAsync();
        Debug.WriteLine($"🎫 Toplam {tumBiletler.Count} bilet alındı");
        
        DoluKoltuklar = tumBiletler
            .Where(b => b.SeferID == SeferId)
            .Select(b => b.KoltukID)
            .ToList();
            
        Debug.WriteLine($"🔒 Bu sefer için {DoluKoltuklar.Count} koltuk dolu (SeferID: {SeferId})");
        Debug.WriteLine($"🔒 Dolu koltuk ID'leri: {string.Join(", ", DoluKoltuklar.Take(5))}");

        // Koltuk sıralarını oluştur (2+1 düzen)
        CreateKoltukRows(otobusKoltuklari);
    }

    private void CreateKoltukRows(List<Koltuk> koltuklar)
    {
        KoltukRows.Clear();
        
        Debug.WriteLine($"🔧 CreateKoltukRows başlıyor, {koltuklar.Count} koltuk var");
        
        if (koltuklar.Count == 0)
        {
            Debug.WriteLine("❌ Hiç koltuk yok, sıralar oluşturulamaz!");
            return;
        }
        
        // Koltuk numaralarını kontrol et
        foreach (var koltuk in koltuklar.Take(5))
        {
            var rowNum = GetRowNumber(koltuk.KoltukNo);
            var position = GetSeatPosition(koltuk.KoltukNo);
            Debug.WriteLine($"   🪑 Koltuk '{koltuk.KoltukNo}' -> Sıra: {rowNum}, Pozisyon: '{position}'");
        }
        
        // Koltuk numaralarına göre sırala ve grupla
        var siraGroups = koltuklar
            .OrderBy(k => k.KoltukNo)
            .GroupBy(k => GetRowNumber(k.KoltukNo))
            .OrderBy(g => g.Key);

        Debug.WriteLine($"📊 {siraGroups.Count()} farklı sıra grubu oluşturuldu");

        foreach (var siraGroup in siraGroups)
        {
            var row = new KoltukRow { RowNumber = siraGroup.Key };
            Debug.WriteLine($"🏗️ Sıra {siraGroup.Key} oluşturuluyor, {siraGroup.Count()} koltuk var");
            
            foreach (var koltuk in siraGroup.OrderBy(k => k.KoltukNo))
            {
                var koltukVm = new KoltukViewModel
                {
                    Koltuk = koltuk,
                    IsOccupied = DoluKoltuklar.Contains(koltuk.KoltukID),
                    IsSelected = false
                };
                
                // 2+1 düzene göre yerleştir
                var position = GetSeatPosition(koltuk.KoltukNo);
                Debug.WriteLine($"      🪑 '{koltuk.KoltukNo}' pozisyonu: '{position}', Dolu: {koltukVm.IsOccupied}");
                
                if (position == "A") 
                {
                    row.SeatA = koltukVm;
                    Debug.WriteLine($"         -> A pozisyonuna yerleştirildi");
                }
                else if (position == "B") 
                {
                    row.SeatB = koltukVm;
                    Debug.WriteLine($"         -> B pozisyonuna yerleştirildi");
                }
                else if (position == "C") 
                {
                    row.SeatC = koltukVm;
                    Debug.WriteLine($"         -> C pozisyonuna yerleştirildi");
                }
                else
                {
                    Debug.WriteLine($"         ❌ Bilinmeyen pozisyon: '{position}' - koltuk yerleştirilemedi!");
                }
            }
            
            // Sıranın dolu olup olmadığını kontrol et
            var seatCount = (row.SeatA != null ? 1 : 0) + (row.SeatB != null ? 1 : 0) + (row.SeatC != null ? 1 : 0);
            Debug.WriteLine($"      ✅ Sıra {row.RowNumber} tamamlandı: {seatCount} koltuk yerleştirildi");
            
            KoltukRows.Add(row);
        }
        
        Debug.WriteLine($"✅ {KoltukRows.Count} sıra oluşturuldu, UI güncellenecek");
        
        // KoltukRows değiştiğini UI'a bildir - Main thread'de çalıştır
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(KoltukRows));
            Debug.WriteLine($"📱 UI thread'de KoltukRows güncellendi");
        });
    }

    private int GetRowNumber(string koltukNo)
    {
        // "1A", "2B" gibi formattan sıra numarasını çıkar
        var numPart = new string(koltukNo.Where(char.IsDigit).ToArray());
        return int.TryParse(numPart, out int row) ? row : 1;
    }

    private string GetSeatPosition(string koltukNo)
    {
        // "1A", "2B" gibi formattan harf kısmını çıkar  
        var letterPart = new string(koltukNo.Where(char.IsLetter).ToArray());
        return letterPart.ToUpper();
    }

    [RelayCommand]
    private async Task SelectKoltukAsync(KoltukViewModel koltukVm)
    {
        if (koltukVm?.Koltuk == null || koltukVm.IsOccupied) return;

        Debug.WriteLine($"🔘 Koltuk seçimi: {koltukVm.KoltukNo}, Mevcut durum: {(koltukVm.IsSelected ? "Seçili" : "Boş")}");

        if (koltukVm.IsSelected)
        {
            // Seçimi kaldır
            koltukVm.IsSelected = false;
            SecilenKoltuklar.Remove(koltukVm.Koltuk);
            Debug.WriteLine($"✅ Koltuk seçimi kaldırıldı: {koltukVm.KoltukNo}");
        }
        else
        {
            // Yeni seçim
            if (SecilenKoltuklar.Count >= YolcuSayisi)
            {
                await Shell.Current.DisplayAlert("Uyarı", 
                    $"En fazla {YolcuSayisi} koltuk seçebilirsiniz!", "Tamam");
                return;
            }
            
            koltukVm.IsSelected = true;
            SecilenKoltuklar.Add(koltukVm.Koltuk);
            Debug.WriteLine($"✅ Koltuk seçildi: {koltukVm.KoltukNo}");
        }

        // Force UI update için KoltukRows'u yeniden tetikle
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(KoltukRows));
            OnPropertyChanged(nameof(HasSelectedSeats));
            OnPropertyChanged(nameof(ToplamFiyat));
            OnPropertyChanged(nameof(CanContinue));
            OnPropertyChanged(nameof(DevamButtonText));
        });

        Debug.WriteLine($"📊 Toplam seçilen: {SecilenKoltuklar.Count}/{YolcuSayisi}");
    }

    [RelayCommand]
    private async Task DevamEtAsync()
    {
        if (!CanContinue)
        {
            await Shell.Current.DisplayAlert("Uyarı", SelectionHint, "Tamam");
            return;
        }

        // Devam işlemi - örneğin ödeme sayfasına git
        await Shell.Current.DisplayAlert("Başarılı", 
            $"{SecilenKoltuklar.Count} koltuk seçildi!\nToplam: {ToplamFiyat:C} TL", "Tamam");
    }

    [RelayCommand]
    private async Task GeriAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}

// Helper Classes
public class KoltukRow
{
    public int RowNumber { get; set; }
    public KoltukViewModel? SeatA { get; set; }
    public KoltukViewModel? SeatB { get; set; }
    public KoltukViewModel? SeatC { get; set; }
}

public partial class KoltukViewModel : ObservableObject
{
    public Koltuk? Koltuk { get; set; }
    
    [ObservableProperty]
    private bool isSelected;
    
    [ObservableProperty]
    private bool isOccupied;
    
    public string KoltukNo => Koltuk?.KoltukNo ?? "";
    public bool IsAvailable => !IsOccupied;
    public Color BackgroundColor => IsOccupied ? Colors.Red : IsSelected ? Colors.Blue : Colors.LightGreen;
    public Color TextColor => IsOccupied || IsSelected ? Colors.White : Colors.Black;

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(BackgroundColor));
        OnPropertyChanged(nameof(TextColor));
    }

    partial void OnIsOccupiedChanged(bool value)
    {
        OnPropertyChanged(nameof(BackgroundColor));
        OnPropertyChanged(nameof(TextColor));
        OnPropertyChanged(nameof(IsAvailable));
    }
} 