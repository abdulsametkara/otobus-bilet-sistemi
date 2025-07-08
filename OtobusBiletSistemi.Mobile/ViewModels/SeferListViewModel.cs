using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using OtobusBiletSistemi.Mobile.Models;
using OtobusBiletSistemi.Mobile.Services;
using OtobusBiletSistemi.Mobile.Views;
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
        private string searchMessage = "Sefer aramak için yukardaki formu kullanın";

        [ObservableProperty]
        private ObservableCollection<string> kalkisYerleri = new();

        [ObservableProperty]
        private ObservableCollection<string> varisYerleri = new();

        [ObservableProperty]
        private int yolcuSayisi = 1;

        [ObservableProperty]
        private ObservableCollection<int> yolcuSayisiListesi = new();

        // Modern UI için ek property'ler
        [ObservableProperty]
        private bool isSearchSectionVisible = false;

        [ObservableProperty]
        private string greetingMessage = "Günaydın,";

        [ObservableProperty]
        private string userNameDisplay = "Değerli Yolcu";

        [ObservableProperty]
        private ObservableCollection<string> populerGuzergahlar = new();

        [ObservableProperty]
        private ObservableCollection<Sefer> sonArananSeferler = new();

        // Web tasarımına uygun ek property'ler
        public bool HasResults => !IsLoading && Seferler.Any();
        public bool HasNoResults => !IsLoading && !Seferler.Any() && !string.IsNullOrEmpty(SearchMessage);
        public bool HasSearchMessage => !string.IsNullOrEmpty(SearchMessage) && !IsLoading;
        public bool HasValidationErrors => string.IsNullOrEmpty(KalkisYeri) || string.IsNullOrEmpty(VarisYeri) || KalkisYeri == VarisYeri;

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
                
                // Kişiselleştirilmiş karşılama ayarla
                SetGreetingMessage();
                
                // Database verilerini yükle
                await LoadSehirlerAsync();
                await LoadPopulerGuzergahlarAsync();
                await LoadSonArananSeferlerAsync();
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

        private void SetGreetingMessage()
        {
            var now = DateTime.Now;
            if (now.Hour < 12)
                GreetingMessage = "Günaydın,";
            else if (now.Hour < 18)
                GreetingMessage = "İyi günler,";
            else
                GreetingMessage = "İyi akşamlar,";
        }

        private async Task LoadPopulerGuzergahlarAsync()
        {
            try
            {
                Debug.WriteLine("LoadPopulerGuzergahlarAsync başlatıldı");
                
                var populerler = await _apiService.GetPopulerGuzergahlarAsync();
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    PopulerGuzergahlar.Clear();
                    foreach (var guzergah in populerler)
                    {
                        PopulerGuzergahlar.Add(guzergah);
                    }
                });
                
                Debug.WriteLine($"✅ {populerler.Count} popüler güzergah yüklendi");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ LoadPopulerGuzergahlarAsync Hatası: {ex.Message}");
            }
        }

        private async Task LoadSonArananSeferlerAsync()
        {
            try
            {
                Debug.WriteLine("LoadSonArananSeferlerAsync başlatıldı");
                
                var sonSeferler = await _apiService.GetSonArananSeferlerAsync(3);
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SonArananSeferler.Clear();
                    foreach (var sefer in sonSeferler)
                    {
                        SonArananSeferler.Add(sefer);
                    }
                });
                
                Debug.WriteLine($"✅ {sonSeferler.Count} son sefer yüklendi");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ LoadSonArananSeferlerAsync Hatası: {ex.Message}");
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

        partial void OnKalkisYeriChanged(string value)
        {
            OnPropertyChanged(nameof(HasValidationErrors));
        }

        partial void OnVarisYeriChanged(string value)
        {
            OnPropertyChanged(nameof(HasValidationErrors));
        }

        // Modern UI Commands
        [RelayCommand]
        private async Task QuickSearchAsync(string destinations)
        {
            try
            {
                if (string.IsNullOrEmpty(destinations)) return;

                // Parse destinations (format: "From,To")
                var parts = destinations.Split(',');
                if (parts.Length == 2)
                {
                    KalkisYeri = parts[0].Trim();
                    VarisYeri = parts[1].Trim();
                    
                    // Perform search
                    await SearchSeferlerAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"QuickSearch Hatası: {ex.Message}");
                await Shell.Current.DisplayAlert("Hata", "Hızlı arama sırasında hata oluştu.", "Tamam");
            }
        }

        [RelayCommand]
        private void ShowSearch()
        {
            IsSearchSectionVisible = !IsSearchSectionVisible;
            Debug.WriteLine($"Arama bölümü görünürlüğü: {IsSearchSectionVisible}");
        }

        [RelayCommand]
        private void SwapLocations()
        {
            var temp = KalkisYeri;
            KalkisYeri = VarisYeri;
            VarisYeri = temp;
            Debug.WriteLine($"Yerler değiştirildi: {KalkisYeri} <-> {VarisYeri}");
        }

        [RelayCommand]
        private async Task SeferAraAsync()
        {
            await SearchSeferlerAsync();
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
                
                // Database'den benzersiz şehir listelerini çek
                var kalkisYerleriSet = await _apiService.GetUniqueKalkisYerleriAsync();
                var varisYerleriSet = await _apiService.GetUniqueVarisYerleriAsync();
                
                Debug.WriteLine($"✅ Database'den {kalkisYerleriSet.Count} kalkış yeri alındı");
                Debug.WriteLine($"✅ Database'den {varisYerleriSet.Count} varış yeri alındı");
                
                if (kalkisYerleriSet == null || kalkisYerleriSet.Count == 0)
                {
                    Debug.WriteLine("❌ HATA: Database'den kalkış yerleri bulunamadı!");
                    throw new InvalidOperationException("Kalkış yeri verisi bulunamadı!");
                }
                
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
                Debug.WriteLine("🔄 LoadSeferlerAsync başlıyor...");

                var seferler = await _apiService.GetSeferlerAsync();
                Debug.WriteLine($"📡 API'den {seferler.Count} sefer alındı");
                
                // İlk 3 seferi detaylı kontrol et
                for (int i = 0; i < Math.Min(3, seferler.Count); i++)
                {
                    var sefer = seferler[i];
                    Debug.WriteLine($"🎫 Sefer {i+1}:");
                    Debug.WriteLine($"   - SeferID: {sefer.SeferID} (Type: {sefer.SeferID.GetType()})");
                    Debug.WriteLine($"   - Fiyat: {sefer.Fiyat}");
                    Debug.WriteLine($"   - Tarih: {sefer.Tarih}");
                    Debug.WriteLine($"   - Saat: '{sefer.Saat}'");
                    Debug.WriteLine($"   - GuzergahID: {sefer.GuzergahID}");
                    Debug.WriteLine($"   - OtobusID: {sefer.OtobusID}");
                }
                
                Seferler.Clear();
                foreach (var sefer in seferler)
                {
                    Debug.WriteLine($"➕ Listeye eklenen sefer: ID={sefer.SeferID}, Fiyat={sefer.Fiyat}");
                    Seferler.Add(sefer);
                }

                Debug.WriteLine($"✅ {Seferler.Count} sefer UI'a yüklendi");

                if (!Seferler.Any())
                {
                    SearchMessage = "Henüz sistemde sefer bulunmuyor";
                }
                else
                {
                    SearchMessage = "";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ LoadSeferlerAsync Hatası: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                SearchMessage = "Seferler yüklenirken hata oluştu";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SearchSeferlerAsync()
        {
            try
            {
                Debug.WriteLine($"🔍 SearchSeferlerAsync başlatıldı: {KalkisYeri} → {VarisYeri}, Tarih: {Tarih:dd.MM.yyyy}");
                
                IsLoading = true;
                SearchMessage = "";
                
                // Arama kriterleri kontrolü
                if (string.IsNullOrEmpty(KalkisYeri) || string.IsNullOrEmpty(VarisYeri))
                {
                    SearchMessage = "Lütfen kalkış ve varış yerlerini seçin";
                    IsLoading = false;
                    return;
                }
                
                if (KalkisYeri == VarisYeri)
                {
                    SearchMessage = "Kalkış ve varış yerleri aynı olamaz";
                    IsLoading = false;
                    return;
                }
                
                // ApiService ile gelişmiş sefer arama
                var seferler = await _apiService.SearchAdvancedSeferlerAsync(KalkisYeri, VarisYeri, Tarih, YolcuSayisi);
                Debug.WriteLine($"✅ {seferler.Count} sefer bulundu");
                
                // Sonuçları UI'a yükle
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Seferler.Clear();
                    foreach (var sefer in seferler)
                    {
                        Seferler.Add(sefer);
                    }
                    
                    if (!Seferler.Any())
                    {
                        SearchMessage = $"{KalkisYeri} → {VarisYeri} güzergahında {Tarih:dd.MM.yyyy} tarihinde sefer bulunamadı";
                    }
                    else
                    {
                        SearchMessage = "";
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ SearchSeferlerAsync Hatası: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                
                SearchMessage = "Sefer arama sırasında hata oluştu: " + ex.Message;
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
            SearchMessage = "Sefer aramak için yukarıdaki formu doldurun";
            
            Seferler.Clear();
            
            Debug.WriteLine("✅ Arama formu temizlendi");
        }

        [RelayCommand]
        public async Task TestNavigationAsync()
        {
            try
            {
                Debug.WriteLine("🧪 Test navigasyonu başlatılıyor...");
                
                // Test verisi ile koltuk seçim sayfasına git
                var testSefer = new Sefer
                {
                    SeferID = 1,
                    Tarih = DateTime.Today,
                    Saat = "09:00",
                    Fiyat = 50.00m,
                    GuzergahID = 1,
                    OtobusID = 1
                };
                
                await SeferSecAsync(testSefer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Test navigasyonu hatası: {ex.Message}");
                await Shell.Current.DisplayAlert("Test Hatası", ex.Message, "Tamam");
            }
        }

        [RelayCommand]
        public async Task SeferSecAsync(Sefer sefer)
        {
            try
            {
                Debug.WriteLine($"🚀 SeferSecAsync BAŞLADI");
                
                if (sefer == null) 
                {
                    Debug.WriteLine("❌ Sefer parametresi null!");
                    await Shell.Current.DisplayAlert("Hata", "Sefer bilgisi bulunamadı!", "Tamam");
                    return;
                }
                
                // Detaylı sefer kontrolü
                Debug.WriteLine($"🔍 Sefer Object Details:");
                Debug.WriteLine($"   - Object Type: {sefer.GetType().Name}");
                Debug.WriteLine($"   - SeferID: {sefer.SeferID} (Type: {sefer.SeferID.GetType()})");
                Debug.WriteLine($"   - GuzergahID: {sefer.GuzergahID}");
                Debug.WriteLine($"   - OtobusID: {sefer.OtobusID}");
                Debug.WriteLine($"   - Fiyat: {sefer.Fiyat} TL");
                Debug.WriteLine($"   - Tarih: {sefer.Tarih:yyyy-MM-dd}");
                Debug.WriteLine($"   - Saat: '{sefer.Saat}'");
                Debug.WriteLine($"   - YolcuSayisi: {YolcuSayisi}");
                
                // SeferID kontrolü
                if (sefer.SeferID <= 0)
                {
                    Debug.WriteLine($"❌ FATAL: Geçersiz SeferID: {sefer.SeferID}");
                    Debug.WriteLine($"❌ Sefer objesi bozuk olabilir!");
                    
                    // Tüm seferleri kontrol et
                    var tumSeferler = await _apiService.GetSeferlerAsync();
                    Debug.WriteLine($"🔍 Sistemdeki toplam sefer sayısı: {tumSeferler.Count}");
                    if (tumSeferler.Any())
                    {
                        var ilkSefer = tumSeferler.First();
                        Debug.WriteLine($"🔍 İlk sefer örneği: ID={ilkSefer.SeferID}, Fiyat={ilkSefer.Fiyat}");
                    }
                    
                    await Shell.Current.DisplayAlert("Kritik Hata", 
                        $"Sefer ID geçersiz: {sefer.SeferID}\nLütfen farklı bir sefer deneyin.", "Tamam");
                    return;
                }

                Debug.WriteLine($"✅ SeferID geçerli: {sefer.SeferID}");

                // Navigation başlıyor
                try
                {
                    Debug.WriteLine($"🚀 Navigation başlıyor...");
                    Debug.WriteLine($"📊 FINAL PARAMETERS: SeferID={sefer.SeferID}, YolcuSayisi={YolcuSayisi}");
                    
                    // ViewModel'i oluştur VE hemen verileri yükle
                    var koltukViewModel = new KoltukSecimViewModel(_apiService);
                    
                    // ÖNCE verileri yükle, SONRA sayfayı aç
                    Debug.WriteLine($"📡 Veri yükleme başlıyor - SeferID: {sefer.SeferID}");
                    await koltukViewModel.LoadKoltukDataAsync(sefer.SeferID, YolcuSayisi);
                    Debug.WriteLine($"✅ Veriler yüklendi");
                    
                    // Şimdi sayfayı oluştur ve aç
                    var koltukPage = new KoltukSecimPage(koltukViewModel);
                    Debug.WriteLine($"🏗️ Sayfa oluşturuldu");
                    
                    await Shell.Current.Navigation.PushModalAsync(koltukPage);
                    Debug.WriteLine($"✅ Navigation tamamlandı");
                    
                    return;
                }
                catch (Exception directEx)
                {
                    Debug.WriteLine($"❌ Navigation Hatası: {directEx.Message}");
                    Debug.WriteLine($"❌ InnerException: {directEx.InnerException?.Message}");
                    Debug.WriteLine($"❌ Stack trace: {directEx.StackTrace}");
                    
                    await Shell.Current.DisplayAlert(
                        "Navigation Hatası", 
                        $"Koltuk seçim sayfası açılamadı:\n{directEx.Message}", 
                        "Tamam");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ SeferSecAsync FATAL ERROR: {ex.Message}");
                Debug.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                
                await Shell.Current.DisplayAlert(
                    "Sistem Hatası", 
                    $"Beklenmeyen bir hata oluştu:\n{ex.Message}", 
                    "Tamam");
            }
        }
    }
}