using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OtobusBiletSistemi.Mobile.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace OtobusBiletSistemi.Mobile.ViewModels
{
    [QueryProperty(nameof(SeferId), "SeferId")]
    [QueryProperty(nameof(BiletSayisi), "BiletSayisi")]
    [QueryProperty(nameof(ToplamFiyat), "ToplamFiyat")]
    [QueryProperty(nameof(KalkisYeri), "KalkisYeri")]
    [QueryProperty(nameof(VarisYeri), "VarisYeri")]
    [QueryProperty(nameof(SeferTarihi), "SeferTarihi")]
    [QueryProperty(nameof(KalkisSaati), "KalkisSaati")]
    public partial class OdemeViewModel : ObservableObject
    {
        [ObservableProperty]
        private int seferId;
        
        [ObservableProperty]
        private int biletSayisi;

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
        private string varisSaati = "";

        // Bilet sahipleri listesi
        [ObservableProperty]
        private ObservableCollection<BiletSahibi> biletSahipleri = new();

        // Ödeme form alanları
        [ObservableProperty]
        private string selectedPaymentMethod = "CreditCard";

        [ObservableProperty]
        private string cardNumber = "";

        [ObservableProperty]
        private string cardHolder = "";

        [ObservableProperty]
        private string expiryDate = "";

        [ObservableProperty]
        private string cvv = "";

        [ObservableProperty]
        private string mobileNumber = "";

        public bool IsCardSelected => SelectedPaymentMethod == "CreditCard";
        public bool IsMobileSelected => SelectedPaymentMethod == "Mobile";

        public OdemeViewModel()
        {
            // Test verileri ekle
            InitializeTestData();
        }

        private void InitializeTestData()
        {
            // Örnek bilet sahipleri
            biletSahipleri.Add(new BiletSahibi
            {
                KoltukNo = "1A",
                KoltukId = 1,
                Ad = "Ahmet",
                Soyad = "Yılmaz",
                TcKimlikNo = "12345678901",
                Cinsiyet = "Erkek",
                Telefon = "05123456789",
                Email = "ahmet@example.com"
            });

            if (BiletSayisi > 1)
            {
                biletSahipleri.Add(new BiletSahibi
                {
                    KoltukNo = "1B",
                    KoltukId = 2,
                    Ad = "Fatma",
                    Soyad = "Yılmaz",
                    TcKimlikNo = "10987654321",
                    Cinsiyet = "Kadın",
                    Telefon = "05987654321",
                    Email = "fatma@example.com"
                });
            }
        }

        partial void OnBiletSayisiChanged(int value)
        {
            // Bilet sayısı değiştiğinde test verilerini güncelle
            biletSahipleri.Clear();
            InitializeTestData();
        }

        partial void OnKalkisSaatiChanged(string value)
        {
            UpdateVarisSaati();
        }

        partial void OnSeferTarihiChanged(string value)
        {
            UpdateVarisSaati();
        }

        private void UpdateVarisSaati()
        {
            try
            {
                if (!string.IsNullOrEmpty(KalkisSaati))
                {
                    if (TimeSpan.TryParse(KalkisSaati, out var kalkisTime))
                    {
                        // Ortalama 2 saatlik yolculuk süresi varsayalım
                        var varisTime = kalkisTime.Add(TimeSpan.FromHours(2));
                        varisSaati = varisTime.ToString(@"hh\:mm");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"VarisSaati hesaplanırken hata: {ex.Message}");
            }
        }

        partial void OnSelectedPaymentMethodChanged(string value)
        {
            OnPropertyChanged(nameof(IsCardSelected));
            OnPropertyChanged(nameof(IsMobileSelected));
        }

        [RelayCommand]
        private void SelectPaymentMethod(string method)
        {
            SelectedPaymentMethod = method;
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
                // Ödeme yöntemi doğrulaması
                if (IsCardSelected)
                {
                    if (!ValidateCardPayment())
                        return;
                }
                else if (IsMobileSelected)
                {
                    if (!ValidateMobilePayment())
                        return;
                }

                // Loading göster
                await Shell.Current.DisplayAlert("Ödeme İşlemi", "Ödeme işleminiz gerçekleştiriliyor...", "Tamam");

                // API çağrısı simülasyonu
                await Task.Delay(2000);

                // Başarılı ödeme
                await Shell.Current.DisplayAlert(
                    "Ödeme Başarılı!", 
                    $"🎉 Ödemeniz başarıyla tamamlandı!\n\n" +
                    $"Bilet Detayları:\n" +
                    $"• {KalkisYeri} → {VarisYeri}\n" +
                    $"• {SeferTarihi} - {KalkisSaati}\n" +
                    $"• {BiletSayisi} Yolcu\n" +
                    $"• Toplam: {ToplamFiyat:C}\n\n" +
                    $"Biletiniz e-posta adresinize gönderilecektir.",
                    "Tamam");

                // Ana sayfaya dön
                await Shell.Current.GoToAsync("//SeferListPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ödeme hatası: {ex.Message}");
                await Shell.Current.DisplayAlert("Hata", "Ödeme işlemi sırasında bir hata oluştu. Lütfen tekrar deneyin.", "Tamam");
            }
        }

        private bool ValidateCardPayment()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(CardNumber) || CardNumber.Replace(" ", "").Length < 16)
                errors.Add("Geçerli bir kart numarası giriniz");

            if (string.IsNullOrWhiteSpace(CardHolder))
                errors.Add("Kart sahibi adını giriniz");

            if (string.IsNullOrWhiteSpace(ExpiryDate) || !Regex.IsMatch(ExpiryDate, @"^\d{2}/\d{2}$"))
                errors.Add("Geçerli bir son kullanma tarihi giriniz (MM/YY)");

            if (string.IsNullOrWhiteSpace(Cvv) || Cvv.Length != 3)
                errors.Add("Geçerli bir CVV kodu giriniz");

            if (errors.Any())
            {
                var message = "Lütfen aşağıdaki alanları kontrol ediniz:\n• " + string.Join("\n• ", errors);
                Shell.Current.DisplayAlert("Eksik Bilgi", message, "Tamam");
                return false;
            }

            return true;
        }

        private bool ValidateMobilePayment()
        {
            if (string.IsNullOrWhiteSpace(MobileNumber) || MobileNumber.Length != 11 || !MobileNumber.StartsWith("05"))
            {
                Shell.Current.DisplayAlert("Hata", "Geçerli bir cep telefonu numarası giriniz (05xxxxxxxxx)", "Tamam");
                return false;
            }

            return true;
        }
    }
} 