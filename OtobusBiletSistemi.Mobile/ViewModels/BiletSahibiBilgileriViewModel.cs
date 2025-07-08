using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtobusBiletSistemi.Mobile.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace OtobusBiletSistemi.Mobile.ViewModels
{
    [QueryProperty(nameof(SeferId), "SeferId")]
    [QueryProperty(nameof(SecilenKoltukIds), "SecilenKoltukIds")]
    [QueryProperty(nameof(SecilenKoltukNos), "SecilenKoltukNos")]
    [QueryProperty(nameof(ToplamFiyat), "ToplamFiyat")]
    [QueryProperty(nameof(KalkisYeri), "KalkisYeri")]
    [QueryProperty(nameof(VarisYeri), "VarisYeri")]
    [QueryProperty(nameof(SeferTarihi), "SeferTarihi")]
    [QueryProperty(nameof(KalkisSaati), "KalkisSaati")]
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
        private string kalkisYeri = "";

        [ObservableProperty]
        private string varisYeri = "";

        [ObservableProperty]
        private string seferTarihi = "";

        [ObservableProperty]
        private string kalkisSaati = "";

        [ObservableProperty]
        private ObservableCollection<BiletSahibi> biletSahipleri = new();

        [ObservableProperty]
        private int mevcutIndex = 0;
        
        [ObservableProperty]
        private BiletSahibi? mevcutBiletSahibi;

        public bool CanGoNext => MevcutIndex < BiletSahipleri.Count - 1;
        public bool CanGoPrevious => MevcutIndex > 0;
        public bool IsLastPassenger => MevcutIndex == BiletSahipleri.Count - 1;
        public bool ShowPaymentButton => IsLastPassenger && mevcutBiletSahibi?.IsValid == true;
        public bool IsSinglePassenger => BiletSahipleri.Count == 1;
        public bool HasMultiplePassengers => BiletSahipleri.Count > 1;
        public string SayfaBilgisi => $"{MevcutIndex + 1} / {BiletSahipleri.Count}";
        public string CurrentPassengerInfo => $"{MevcutIndex + 1}/{BiletSahipleri.Count}";
        public string NextButtonText => IsLastPassenger ? "Ödemeye Geç" : "Sonraki Yolcu";
        
        public BiletSahibiBilgileriViewModel()
        {
        }

        partial void OnSecilenKoltukNosChanged(List<string> value)
        {
            if (value?.Any() == true)
            {
                biletSahipleri.Clear();
                for (int i = 0; i < value.Count; i++)
                {
                    var biletSahibi = new BiletSahibi
                    {
                        KoltukNo = value[i],
                        KoltukId = SecilenKoltukIds[i]
                    };
                    biletSahipleri.Add(biletSahibi);
                }
                UpdateMevcutBiletSahibi();
                OnPropertyChanged(nameof(SayfaBilgisi));
                OnPropertyChanged(nameof(CurrentPassengerInfo));
                OnPropertyChanged(nameof(IsSinglePassenger));
                OnPropertyChanged(nameof(HasMultiplePassengers));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(IsLastPassenger));
                OnPropertyChanged(nameof(ShowPaymentButton));
            }
        }
        
        partial void OnMevcutIndexChanged(int value)
        {
            UpdateMevcutBiletSahibi();
            OnPropertyChanged(nameof(CurrentPassengerInfo));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(IsLastPassenger));
            OnPropertyChanged(nameof(ShowPaymentButton));
        }
        
        private void UpdateMevcutBiletSahibi()
        {
            mevcutBiletSahibi = BiletSahipleri.Count > MevcutIndex ? BiletSahipleri[MevcutIndex] : null;
        }
        
        [RelayCommand]
        private void SonrakiBiletSahibi()
        {
            if (CanGoNext)
            {
                MevcutIndex++;
            }
        }

        [RelayCommand]
        private void OncekiBiletSahibi()
        {
            if (CanGoPrevious)
            {
                MevcutIndex--;
            }
        }

        [RelayCommand]
        private async Task GeriAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task OdemeYapAsync()
        {
            try
            {
                // Mevcut yolcunun bilgilerini kontrol et
                if (mevcutBiletSahibi == null)
                {
                    await Shell.Current.DisplayAlert("Hata", "Yolcu bilgileri bulunamadı.", "Tamam");
                    return;
                }

                // Mevcut yolcunun bilgilerini kontrol et
                if (!mevcutBiletSahibi.IsValid)
                {
                    await Shell.Current.DisplayAlert("Eksik Bilgi", "Lütfen tüm zorunlu alanları doldurun.", "Tamam");
                    return;
                }

                // Tüm yolcuların bilgilerini kontrol et
                if (!BiletSahipleri.All(b => b.IsValid))
                {
                    await Shell.Current.DisplayAlert("Eksik Bilgi", "Lütfen tüm yolcuların bilgilerini eksiksiz doldurun.", "Tamam");
                    return;
                }

                Debug.WriteLine("✅ Validasyon başarılı, ödeme sayfasına yönlendiriliyor...");
                
                // Ödeme sayfasına sadece temel bilgileri geçir - complex object serialization'dan kaçın
                var parameters = new Dictionary<string, object>
                {
                    ["SeferId"] = SeferId,
                    ["BiletSayisi"] = BiletSahipleri.Count,
                    ["ToplamFiyat"] = ToplamFiyat,
                    ["KalkisYeri"] = KalkisYeri ?? "",
                    ["VarisYeri"] = VarisYeri ?? "",
                    ["SeferTarihi"] = SeferTarihi ?? "",
                    ["KalkisSaati"] = KalkisSaati ?? ""
                };

                Debug.WriteLine($"📋 Navigation parametreleri:");
                Debug.WriteLine($"   SeferId: {SeferId}");
                Debug.WriteLine($"   BiletSayisi: {BiletSahipleri.Count}");
                Debug.WriteLine($"   ToplamFiyat: {ToplamFiyat}");
                Debug.WriteLine($"   KalkisYeri: {KalkisYeri}");
                Debug.WriteLine($"   VarisYeri: {VarisYeri}");

                await Shell.Current.GoToAsync("OdemePage", parameters);
                Debug.WriteLine("✅ Navigation başarılı!");
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ödeme sayfasına geçiş hatası: {ex.Message}");
                Debug.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Hata", $"Bir hata oluştu: {ex.Message}\n\nLütfen tekrar deneyin.", "Tamam");
            }
        }
    }
} 