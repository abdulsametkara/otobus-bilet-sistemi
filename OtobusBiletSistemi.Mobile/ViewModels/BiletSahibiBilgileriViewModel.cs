using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtobusBiletSistemi.Mobile.Models;
using System.Diagnostics;

namespace OtobusBiletSistemi.Mobile.ViewModels;

[QueryProperty(nameof(SeferId), nameof(SeferId))]
[QueryProperty(nameof(SecilenKoltukIds), nameof(SecilenKoltukIds))]
[QueryProperty(nameof(SecilenKoltukNos), nameof(SecilenKoltukNos))]
[QueryProperty(nameof(ToplamFiyat), nameof(ToplamFiyat))]
[QueryProperty(nameof(KalkisYeri), nameof(KalkisYeri))]
[QueryProperty(nameof(VarisYeri), nameof(VarisYeri))]
[QueryProperty(nameof(SeferTarihi), nameof(SeferTarihi))]
[QueryProperty(nameof(KalkisSaati), nameof(KalkisSaati))]
public partial class BiletSahibiBilgileriViewModel : ObservableObject
{
    [ObservableProperty]
    private int seferId;

    [ObservableProperty]
    private List<int> secilenKoltukIds = new();

    [ObservableProperty]
    private List<string> secilenKoltukNos = new();

    [ObservableProperty]
    private decimal toplamFiyat;

    [ObservableProperty]
    private string kalkisYeri = string.Empty;

    [ObservableProperty]
    private string varisYeri = string.Empty;

    [ObservableProperty]
    private string seferTarihi = string.Empty;

    [ObservableProperty]
    private string kalkisSaati = string.Empty;

    [ObservableProperty]
    private ObservableCollection<BiletSahibi> biletSahipleri = new();

    [ObservableProperty]
    private int mevcutIndex = 0;

    [ObservableProperty]
    private bool isLoading = false;

    // Computed Properties
    public BiletSahibi? MevcutBiletSahibi => 
        BiletSahipleri.Count > MevcutIndex ? BiletSahipleri[MevcutIndex] : null;

    public bool CanContinue => BiletSahipleri.All(b => b.IsValid);
    public bool CanGoNext => MevcutIndex < BiletSahipleri.Count - 1;
    public bool CanGoPrevious => MevcutIndex > 0;
    public bool IsLastPassenger => MevcutIndex == BiletSahipleri.Count - 1;
    public bool ShowPaymentButton => IsLastPassenger && MevcutBiletSahibi?.IsValid == true;
    public bool ShowNavigationButtons => BiletSahipleri.Count > 1;
    public string SayfaBilgisi => $"{MevcutIndex + 1} / {BiletSahipleri.Count}";
    public string BaslikMetni => $"Bilet Sahibi Bilgileri - Koltuk {MevcutBiletSahibi?.KoltukNo}";
    public string NextButtonText => IsLastPassenger ? "Tamamla" : "Sonraki";

    public BiletSahibiBilgileriViewModel()
    {
        Debug.WriteLine("🎫 BiletSahibiBilgileriViewModel oluşturuldu");
    }

    partial void OnSecilenKoltukNosChanged(List<string> value)
    {
        if (value?.Count > 0)
        {
            InitializeBiletSahipleri();
        }
    }

    private void InitializeBiletSahipleri()
    {
        Debug.WriteLine($"🎫 Bilet sahipleri başlatılıyor: {SecilenKoltukNos.Count} koltuk");

        BiletSahipleri.Clear();

        for (int i = 0; i < SecilenKoltukNos.Count; i++)
        {
            var biletSahibi = new BiletSahibi
            {
                KoltukNo = SecilenKoltukNos[i],
                KoltukId = SecilenKoltukIds[i]
            };

            BiletSahipleri.Add(biletSahibi);
            Debug.WriteLine($"   ✅ Bilet sahibi eklendi: Koltuk {biletSahibi.KoltukNo}");
        }

        // UI güncellemelerini tetikle
        OnPropertyChanged(nameof(MevcutBiletSahibi));
        OnPropertyChanged(nameof(SayfaBilgisi));
        OnPropertyChanged(nameof(BaslikMetni));
        OnPropertyChanged(nameof(CanGoNext));
        OnPropertyChanged(nameof(CanGoPrevious));
        OnPropertyChanged(nameof(IsLastPassenger));
        OnPropertyChanged(nameof(ShowPaymentButton));
        OnPropertyChanged(nameof(ShowNavigationButtons));
        OnPropertyChanged(nameof(NextButtonText));
    }

    [RelayCommand]
    private void SonrakiBiletSahibi()
    {
        if (CanGoNext)
        {
            MevcutIndex++;
            OnPropertyChanged(nameof(MevcutBiletSahibi));
            OnPropertyChanged(nameof(SayfaBilgisi));
            OnPropertyChanged(nameof(BaslikMetni));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(IsLastPassenger));
            OnPropertyChanged(nameof(ShowPaymentButton));
            OnPropertyChanged(nameof(NextButtonText));
        }
    }

    [RelayCommand]
    private void OncekiBiletSahibi()
    {
        if (CanGoPrevious)
        {
            MevcutIndex--;
            OnPropertyChanged(nameof(MevcutBiletSahibi));
            OnPropertyChanged(nameof(SayfaBilgisi));
            OnPropertyChanged(nameof(BaslikMetni));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(IsLastPassenger));
            OnPropertyChanged(nameof(ShowPaymentButton));
            OnPropertyChanged(nameof(NextButtonText));
        }
    }

    [RelayCommand]
    private async Task OdemeDevamEtAsync()
    {
        if (!CanContinue)
        {
            await Shell.Current.DisplayAlert("Uyarı", 
                "Lütfen tüm bilet sahiplerinin bilgilerini eksiksiz doldurun!", "Tamam");
            return;
        }

        // TODO: Ödeme sayfasına geç
        await Shell.Current.DisplayAlert("Bilgi", 
            $"Tüm bilgiler tamamlandı!\n" +
            $"Toplam: {ToplamFiyat:F0} TL\n" +
            $"Koltuklar: {string.Join(", ", SecilenKoltukNos)}", "Tamam");
    }

    [RelayCommand]
    private async Task GeriAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    partial void OnMevcutIndexChanged(int value)
    {
        OnPropertyChanged(nameof(CanContinue));
        OnPropertyChanged(nameof(IsLastPassenger));
        OnPropertyChanged(nameof(ShowPaymentButton));
        OnPropertyChanged(nameof(NextButtonText));
    }
} 