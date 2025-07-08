using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtobusBiletSistemi.Mobile.Models;
using OtobusBiletSistemi.Mobile.Services;
using System.Diagnostics;
using System.Linq;

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
    public string DevamButtonText => CanContinue ? "✅ Bilgileri Doldur" : $"{SecilenKoltuklar.Count}/{YolcuSayisi} Koltuk Seçili";
    public string OzetMetni => $"Koltuklar: {string.Join(", ", SecilenKoltuklar.Select(k => k.KoltukNo))}";
    public string SelectionHint => YolcuSayisi == 1 ? "Koltuk seçebilirsiniz" : $"{YolcuSayisi} koltuk seçmelisiniz";
    
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
            
            Debug.WriteLine($"🚌 LoadKoltukDataAsync başlıyor - SeferId: {seferId}, YolcuSayisi: {yolcuSayisi}");

            if (seferId <= 0)
            {
                throw new ArgumentException($"Geçersiz SeferId: {seferId}");
            }

            // 1. Sefer bilgilerini yükle
            await LoadSeferInfo();
            
            // 2. Koltuk düzenini oluştur
            await LoadKoltukLayout();

            Debug.WriteLine("✅ Tüm veriler başarıyla yüklendi");
            
            // UI güncellemelerini Main Thread'de yap
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnPropertyChanged(nameof(SeferId));
                OnPropertyChanged(nameof(YolcuSayisi));
                OnPropertyChanged(nameof(KoltukRows));
                OnPropertyChanged(nameof(DevamButtonText));
                OnPropertyChanged(nameof(SelectionHint));
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Veri yükleme hatası: {ex.Message}");
            Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            ErrorMessage = $"Parametreler eksik veya geçersiz!\nSeferId: {seferId}\nYolcuSayisi: {yolcuSayisi}";
            
            // Ana thread'de hata göster
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Shell.Current.DisplayAlert("Veri Yükleme Hatası", 
                    $"Veriler yüklenirken hata oluştu:\n\n{ex.Message}", "Tamam");
            });
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
        
        // Koltuk numaralarını detaylı kontrol et
        Debug.WriteLine($"🧪 İlk 10 koltuk için detaylı analiz:");
        foreach (var koltuk in koltuklar.Take(10))
        {
            var rowNum = GetRowNumber(koltuk.KoltukNo);
            var position = GetSeatPosition(koltuk.KoltukNo);
            Debug.WriteLine($"   🪑 Koltuk '{koltuk.KoltukNo}' -> ID: {koltuk.KoltukID}, Sıra: {rowNum}, Pozisyon: '{position}'");
            
            // Özel kontrol: 10'lu koltuklar
            if (koltuk.KoltukNo.Contains("10"))
            {
                Debug.WriteLine($"      🔍 10'lu koltuk tespit edildi: {koltuk.KoltukNo}");
                var digits = new string(koltuk.KoltukNo.Where(char.IsDigit).ToArray());
                var letters = new string(koltuk.KoltukNo.Where(char.IsLetter).ToArray());
                Debug.WriteLine($"         Digits: '{digits}', Letters: '{letters}'");
            }
        }
        
        // Koltuk numaralarına göre sırala ve grupla
        var siraGroups = koltuklar
            .OrderBy(k => GetRowNumber(k.KoltukNo))
            .ThenBy(k => GetSeatPosition(k.KoltukNo))
            .GroupBy(k => GetRowNumber(k.KoltukNo))
            .OrderBy(g => g.Key);

        Debug.WriteLine($"📊 {siraGroups.Count()} farklı sıra grubu oluşturuldu");

        foreach (var siraGroup in siraGroups)
        {
            var row = new KoltukRow { RowNumber = siraGroup.Key };
            Debug.WriteLine($"🏗️ Sıra {siraGroup.Key} oluşturuluyor, {siraGroup.Count()} koltuk var");
            
            foreach (var koltuk in siraGroup.OrderBy(k => k.KoltukNo))
            {
                var isDolu = DoluKoltuklar.Contains(koltuk.KoltukID);
                var koltukVm = new KoltukViewModel
                {
                    Koltuk = koltuk,
                    IsOccupied = isDolu,
                    IsSelected = false
                };
                
                // 2+2 düzene göre yerleştir
                var position = GetSeatPosition(koltuk.KoltukNo);
                Debug.WriteLine($"      🪑 '{koltuk.KoltukNo}' (ID: {koltuk.KoltukID}) pozisyonu: '{position}', Dolu: {koltukVm.IsOccupied}");
                
                // ÖZEL KONTROL: 3A ve 6B koltukları için debug
                if (koltuk.KoltukNo == "3A" || koltuk.KoltukNo == "6B")
                {
                    Debug.WriteLine($"🔍 ÖZEL KONTROL: Koltuk {koltuk.KoltukNo} (ID: {koltuk.KoltukID})");
                    Debug.WriteLine($"   - Dolu koltuk listesinde var mı? {DoluKoltuklar.Contains(koltuk.KoltukID)}");
                    Debug.WriteLine($"   - Dolu koltuk listesi: [{string.Join(", ", DoluKoltuklar)}]");
                    Debug.WriteLine($"   - ViewModel IsOccupied: {koltukVm.IsOccupied}");
                }
                
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
                else if (position == "D") 
                {
                    row.SeatD = koltukVm;
                    Debug.WriteLine($"         -> D pozisyonuna yerleştirildi");
                }
                else
                {
                    Debug.WriteLine($"         ❌ Bilinmeyen pozisyon: '{position}' - koltuk yerleştirilemedi!");
                }
            }
            
            // Sıranın dolu olup olmadığını kontrol et
            var seatCount = (row.SeatA != null ? 1 : 0) + (row.SeatB != null ? 1 : 0) + (row.SeatC != null ? 1 : 0) + (row.SeatD != null ? 1 : 0);
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
        // "1A", "2B", "10A" gibi formattan sıra numarasını çıkar
        var numPart = new string(koltukNo.Where(char.IsDigit).ToArray());
        Debug.WriteLine($"🔢 GetRowNumber: '{koltukNo}' -> NumPart: '{numPart}'");
        
        if (int.TryParse(numPart, out int row))
        {
            Debug.WriteLine($"   ✅ Parsed row: {row}");
            return row;
        }
        else
        {
            Debug.WriteLine($"   ❌ Parse failed, returning 1");
            return 1;
        }
    }

    private string GetSeatPosition(string koltukNo)
    {
        // "1A", "2B", "10A" gibi formattan harf kısmını çıkar  
        var letterPart = new string(koltukNo.Where(char.IsLetter).ToArray());
        Debug.WriteLine($"🔤 GetSeatPosition: '{koltukNo}' -> LetterPart: '{letterPart}'");
        return letterPart.ToUpper();
    }

    [RelayCommand]
    private void SelectKoltuk(KoltukViewModel koltukVm)
    {
        Debug.WriteLine($"🎯 SelectKoltukAsync çağrıldı!");
        
        if (koltukVm == null || koltukVm.Koltuk == null) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (koltukVm.IsSelected)
            {
                koltukVm.IsSelected = false;
                var koltukToRemove = SecilenKoltuklar.FirstOrDefault(k => k.KoltukID == koltukVm.Koltuk.KoltukID);
                if (koltukToRemove != null)
                {
                    SecilenKoltuklar.Remove(koltukToRemove);
                }
            }
            else
            {
                if (SecilenKoltuklar.Count < YolcuSayisi)
                {
                    koltukVm.IsSelected = true;
                    SecilenKoltuklar.Add(koltukVm.Koltuk);
                }
                else
                {
                    Shell.Current.DisplayAlert("Uyarı", $"En fazla {YolcuSayisi} koltuk seçebilirsiniz.", "Tamam");
                }
            }

            // Update all computed properties
            OnPropertyChanged(nameof(HasSelectedSeats));
            OnPropertyChanged(nameof(ToplamFiyat));
            OnPropertyChanged(nameof(CanContinue));
            OnPropertyChanged(nameof(DevamButtonText));
            OnPropertyChanged(nameof(OzetMetni));
        });
    }

    [RelayCommand]
    private async Task DevamEtAsync()
    {
        if (!CanContinue)
        {
            var message = SecilenKoltuklar.Count == 0 ? 
                $"Lütfen {YolcuSayisi} adet koltuk seçin." : 
                $"Toplam {YolcuSayisi} koltuk seçmelisiniz. Şu an {SecilenKoltuklar.Count} koltuk seçili.";
            await Shell.Current.DisplayAlert("Koltuk Seçimi", message, "Tamam");
            return;
        }

        // Seçilen koltukların ID'lerini al
        var secilenKoltukIds = SecilenKoltuklar.Select(k => k.KoltukID).ToList();
        var secilenKoltukNos = SecilenKoltuklar.Select(k => k.KoltukNo).ToList();
        
        // Bilet sahibi bilgileri sayfasına git
        var parametreler = new Dictionary<string, object>
        {
            ["SeferId"] = SeferId,
            ["SecilenKoltukIds"] = secilenKoltukIds,
            ["SecilenKoltukNos"] = secilenKoltukNos,
            ["ToplamFiyat"] = ToplamFiyat,
            ["KalkisYeri"] = KalkisYeri,
            ["VarisYeri"] = VarisYeri,
            ["SeferTarihi"] = SeferTarihi.ToString("yyyy-MM-dd"),
            ["KalkisSaati"] = KalkisSaati.ToString(@"hh\:mm")
        };

        await Shell.Current.GoToAsync("BiletSahibiBilgileriPage", parametreler);
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
    public KoltukViewModel? SeatD { get; set; }
}

public partial class KoltukViewModel : ObservableObject
{
    public Koltuk? Koltuk { get; set; }
    
    [ObservableProperty]
    private bool isSelected;
    
    [ObservableProperty]
    private bool isOccupied;

    public bool IsVisible => Koltuk != null;

    public string KoltukNo => Koltuk?.KoltukNo ?? "";
    
    // Debug için ek property
    public string DebugInfo 
    {
        get
        {
            if (Koltuk == null) return "NULL_KOLTUK";
            if (string.IsNullOrEmpty(Koltuk.KoltukNo)) return $"ID_{Koltuk.KoltukID}_EMPTY_NO";
            return Koltuk.KoltukNo; // Direkt koltuk numarasını göster
        }
    }
    
    // Accessibility için ContentDescription
    public string ContentDescription 
    {
        get
        {
            var state = IsOccupied ? "Dolu koltuk" : IsSelected ? "Seçili koltuk" : "Boş koltuk";
            return $"Koltuk {KoltukNo}, {state}";
        }
    }
    
    private int GetRowNumber()
    {
        if (string.IsNullOrEmpty(KoltukNo)) return 0;
        var numPart = new string(KoltukNo.Where(char.IsDigit).ToArray());
        return int.TryParse(numPart, out int row) ? row : 0;
    }
    
    private string GetPosition()
    {
        if (string.IsNullOrEmpty(KoltukNo)) return "";
        return new string(KoltukNo.Where(char.IsLetter).ToArray()).ToUpper();
    }
    public bool IsAvailable => !IsOccupied;
    
    // UI Color properties - Material Design 3 uyumlu
    public Color BackgroundColor => IsOccupied ? Color.FromArgb("#E0E0E0") : IsSelected ? Color.FromArgb("#1976D2") : Color.FromArgb("#E8F5E9");
    public Color TextColor => IsOccupied ? Color.FromArgb("#757575") : IsSelected ? Color.FromArgb("#FFFFFF") : Color.FromArgb("#2E7D32");

    partial void OnIsSelectedChanged(bool value)
    {
        Debug.WriteLine($"🔄 Koltuk {KoltukNo} seçim durumu değişti: {value}");
        
        // Ana thread'de UI güncellemelerini yap
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(TextColor));
            OnPropertyChanged(nameof(IsAvailable));
        });
    }

    partial void OnIsOccupiedChanged(bool value)
    {
        Debug.WriteLine($"🔒 Koltuk {KoltukNo} dolu durumu değişti: {value}");
        
        // Ana thread'de UI güncellemelerini yap
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OnPropertyChanged(nameof(BackgroundColor));
            OnPropertyChanged(nameof(TextColor));
            OnPropertyChanged(nameof(IsAvailable));
        });
    }
} 