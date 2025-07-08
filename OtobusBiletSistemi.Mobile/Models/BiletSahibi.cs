using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel;
using System.Linq;

namespace OtobusBiletSistemi.Mobile.Models
{
    public partial class BiletSahibi : ObservableObject
    {
        [ObservableProperty]
        private string koltukNo = "";

        [ObservableProperty]
        private int koltukId;

        [ObservableProperty]
        private string ad = "";

        [ObservableProperty]
        private string soyad = "";

        [ObservableProperty]
        private string tcKimlikNo = "";

        [ObservableProperty]
        private string cinsiyet = "";

        [ObservableProperty]
        private string telefon = "";

        [ObservableProperty]
        private string email = "";

        public bool IsValid => !string.IsNullOrWhiteSpace(Ad) &&
                               !string.IsNullOrWhiteSpace(Soyad) &&
                               !string.IsNullOrWhiteSpace(TcKimlikNo) &&
                               TcKimlikNo.Length == 11 &&
                               !string.IsNullOrWhiteSpace(Cinsiyet) &&
                               !string.IsNullOrWhiteSpace(Telefon) &&
                               !string.IsNullOrWhiteSpace(Email);

        public Style? ErkekButtonStyle => Cinsiyet == "Erkek" ? 
            (Style?)Application.Current?.Resources?["PrimaryButtonStyle"] : 
            (Style?)Application.Current?.Resources?["SecondaryButtonStyle"];
        public Style? KadinButtonStyle => Cinsiyet == "Kadın" ? 
            (Style?)Application.Current?.Resources?["PrimaryButtonStyle"] : 
            (Style?)Application.Current?.Resources?["SecondaryButtonStyle"];
        
        public BiletSahibi()
        {
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Ad) ||
                    e.PropertyName == nameof(Soyad) ||
                    e.PropertyName == nameof(TcKimlikNo) ||
                    e.PropertyName == nameof(Cinsiyet) ||
                    e.PropertyName == nameof(Telefon) ||
                    e.PropertyName == nameof(Email))
                {
                    OnPropertyChanged(nameof(IsValid));
                }
            };
        }
        
        [RelayCommand]
        private void SetCinsiyet(string gender)
        {
            Cinsiyet = gender;
            OnPropertyChanged(nameof(ErkekButtonStyle));
            OnPropertyChanged(nameof(KadinButtonStyle));
        }
    }
} 