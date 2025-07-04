using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using OtobusBiletSistemi.Mobile.Models;
using OtobusBiletSistemi.Mobile.Services;
using System.Linq;
using System.Diagnostics;

namespace OtobusBiletSistemi.Mobile.ViewModels
{
    public partial class SeferListViewModel : ObservableObject
    {
        private readonly IApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<Sefer> seferler = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string kalkisYeri = "";

        [ObservableProperty]
        private string varisYeri = "";

        [ObservableProperty]
        private DateTime tarih = DateTime.Today;

        [ObservableProperty]
        private string searchMessage = "Sefer aramak için yukarıdaki formu doldurun";

        [ObservableProperty]
        private ObservableCollection<string> kalkisYerleri = new();

        [ObservableProperty]
        private ObservableCollection<string> varisYerleri = new();

        [ObservableProperty]
        private int yolcuSayisi = 1;

        [ObservableProperty]
        private ObservableCollection<int> yolcuSayisiListesi = new();

        // Web tasarımına uygun ek property'ler
        public bool HasResults => !IsLoading && Seferler.Any();
        public bool HasNoResults => !IsLoading && !Seferler.Any() && !string.IsNullOrEmpty(SearchMessage);
        public bool HasSearchMessage => !string.IsNullOrEmpty(SearchMessage) && !IsLoading;

        public SeferListViewModel(IApiService apiService)
        {
            _apiService = apiService;
            Debug.WriteLine("SeferListViewModel oluşturuldu, ApiService enjekte edildi.");
            
            // Yolcu sayısı listesini başlat (1-10 kişi)
            for (int i = 1; i <= 10; i++)
            {
                YolcuSayisiListesi.Add(i);
            }
            
            // UI hazır olunca initialize yap
            Application.Current?.Dispatcher.Dispatch(async () => {
                await Task.Delay(300); // UI'ın tamamen yüklenmesi için kısa bir bekleme
                await InitializeAsync();
            });
        }

        // ViewModel initialize edildiğinde şehirleri yükle
        private async Task InitializeAsync()
        {
            try
            {
                Debug.WriteLine("SeferListViewModel - InitializeAsync başladı");
                await LoadSehirlerAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"HATA - InitializeAsync: {ex.Message}");
                Debug.WriteLine($"HATA DETAY: {ex.StackTrace}");
                
                await Shell.Current.DisplayAlert(
                    "Bağlantı Hatası", 
                    "Veritabanına bağlanırken hata oluştu. Lütfen daha sonra tekrar deneyin.", 
                    "Tamam");
            }
        }

        // Property'ler değiştiğinde dependent property'leri de güncelle
        partial void OnIsLoadingChanged(bool value)
        {
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(HasNoResults));
            OnPropertyChanged(nameof(HasSearchMessage));
        }

        partial void OnSeferlerChanged(ObservableCollection<Sefer> value)
        {
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(HasNoResults));
        }

        partial void OnSearchMessageChanged(string value)
        {
            OnPropertyChanged(nameof(HasSearchMessage));
            OnPropertyChanged(nameof(HasNoResults));
        }

        [RelayCommand]
        private async Task LoadSehirlerAsync()
        {
            try
            {
                Debug.WriteLine("🔄 LoadSehirlerAsync başlatıldı...");
                
                // ApiService kontrolü
                if (_apiService == null)
                {
                    Debug.WriteLine("❌ HATA: ApiService null!");
                    throw new InvalidOperationException("ApiService bulunamadı!");
                }
                
                // Veritabanından güzergahları çek
                var guzergahlar = await _apiService.GetGuzergahlarAsync();
                Debug.WriteLine($"✅ Veritabanından {guzergahlar.Count} güzergah alındı");
                
                if (guzergahlar == null || guzergahlar.Count == 0)
                {
                    Debug.WriteLine("❌ HATA: Veritabanından güzergah bulunamadı!");
                    throw new InvalidOperationException("Güzergah verisi bulunamadı!");
                }
                
                // Şehirleri ayrıştır ve sırala
                var kalkisYerleriSet = guzergahlar.Select(g => g.Nereden).Distinct().OrderBy(x => x).ToList();
                var varisYerleriSet = guzergahlar.Select(g => g.Nereye).Distinct().OrderBy(x => x).ToList();
                
                Debug.WriteLine($"📍 Kalkış şehirleri ({kalkisYerleriSet.Count}): {string.Join(", ", kalkisYerleriSet)}");
                Debug.WriteLine($"📍 Varış şehirleri ({varisYerleriSet.Count}): {string.Join(", ", varisYerleriSet)}");
                
                // UI thread'de güncellemeler yap
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Önce temizle
                    KalkisYerleri.Clear();
                    VarisYerleri.Clear();
                    
                    // Sonra şehirleri ekle
                    foreach (var yer in kalkisYerleriSet)
                    {
                        KalkisYerleri.Add(yer);
                    }
                    
                    foreach (var yer in varisYerleriSet)
                    {
                        VarisYerleri.Add(yer);
                    }
                    
                    Debug.WriteLine($"✅ Şehir listeleri UI'a yüklendi. Kalkış: {KalkisYerleri.Count}, Varış: {VarisYerleri.Count}");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ LoadSehirlerAsync Hatası: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                
                // Hata kullanıcıya bildir
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlert(
                        "Veri Yükleme Hatası", 
                        "Şehir bilgileri yüklenirken hata oluştu: " + ex.Message, 
                        "Tamam");
                });
            }
        }

        [RelayCommand]
        public async Task LoadSeferlerAsync()
        {
            try
            {
                IsLoading = true;
                SearchMessage = "";

                var seferler = await _apiService.GetSeferlerAsync();
                
                Seferler.Clear();
                foreach (var sefer in seferler)
                {
                    Seferler.Add(sefer);
                }

                if (!Seferler.Any())
                {
                    SearchMessage = "Henüz sistemde sefer bulunmuyor";
                }
            }
            catch (Exception ex)
            {
                SearchMessage = $"Seferler yüklenirken bir hata oluştu: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SearchSeferlerAsync()
        {
            if (string.IsNullOrWhiteSpace(KalkisYeri) || string.IsNullOrWhiteSpace(VarisYeri))
            {
                SearchMessage = "Lütfen kalkış ve varış şehirlerini seçiniz";
                return;
            }

            try
            {
                IsLoading = true;
                SearchMessage = "";

                // API'den tüm seferleri al
                var seferler = await _apiService.GetSeferlerAsync();
                
                // Güzergah bilgilerini al
                var guzergahlar = await _apiService.GetGuzergahlarAsync();
                
                // Seçilen şehirlere göre filtrele
                var filtrelenmisler = seferler.Where(s => 
                {
                    var guzergah = guzergahlar.FirstOrDefault(g => g.GuzergahID == s.GuzergahID);
                    return guzergah != null &&
                           guzergah.Nereden.Equals(KalkisYeri, StringComparison.OrdinalIgnoreCase) &&
                           guzergah.Nereye.Equals(VarisYeri, StringComparison.OrdinalIgnoreCase) &&
                           s.Tarih.Date == Tarih.Date;
                }).ToList();

                Seferler.Clear();
                foreach (var sefer in filtrelenmisler)
                {
                    Seferler.Add(sefer);
                }

                if (!Seferler.Any())
                {
                    SearchMessage = $"{KalkisYeri} - {VarisYeri} arası {Tarih:dd.MM.yyyy} tarihinde sefer bulunamadı";
                }
            }
            catch (Exception ex)
            {
                SearchMessage = $"Arama sırasında bir hata oluştu: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void ClearSearch()
        {
            KalkisYeri = "";
            VarisYeri = "";
            Tarih = DateTime.Today;
            YolcuSayisi = 1;
            Seferler.Clear();
            SearchMessage = "";
        }

        [RelayCommand]
        public async Task TestNavigationAsync()
        {
            try
            {
                // Dictionary ile test navigation
                var parameters = new Dictionary<string, object>
                {
                    ["seferId"] = "1",
                    ["yolcuSayisi"] = "2"
                };
                
                await Shell.Current.GoToAsync("//koltuklarpage", parameters);
                Debug.WriteLine("✅ Test navigation başarılı");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Test navigation hatası: {ex.Message}");
                await Shell.Current.DisplayAlert("Test Hatası", $"Navigation test hatası:\n{ex.Message}", "OK");
            }
        }

        [RelayCommand]
        public async Task SeferSecAsync(Sefer sefer)
        {
            if (sefer == null) 
            {
                Debug.WriteLine("❌ SeferSecAsync - sefer null!");
                await Shell.Current.DisplayAlert("Hata", "Sefer seçilmedi!", "Tamam");
                return;
            }

            Debug.WriteLine($"🚌 SeferSecAsync çağrıldı - SeferID: {sefer.SeferID}, YolcuSayisi: {YolcuSayisi}");

            if (YolcuSayisi <= 0 || YolcuSayisi > 10)
            {
                Debug.WriteLine($"❌ Geçersiz yolcu sayısı: {YolcuSayisi}");
                await Shell.Current.DisplayAlert("Uyarı", 
                    $"Geçersiz yolcu sayısı: {YolcuSayisi}\nLütfen 1-10 arası bir değer seçiniz", "Tamam");
                return;
            }

            try
            {
                // Dictionary ile navigation parameters geç
                var parameters = new Dictionary<string, object>
                {
                    ["seferId"] = sefer.SeferID.ToString(),
                    ["yolcuSayisi"] = YolcuSayisi.ToString()
                };
                
                Debug.WriteLine($"🚌 Navigation Parameters: seferId={sefer.SeferID}, yolcuSayisi={YolcuSayisi}");
                
                await Shell.Current.GoToAsync("//koltuklarpage", parameters);
                Debug.WriteLine("✅ Navigation başarılı");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Navigation hatası: {ex.Message}");
                // Hata mesajı göster
                await Shell.Current.DisplayAlert("Hata", 
                    $"Navigation hatası:\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}", "Tamam");
            }
        }
    }
}