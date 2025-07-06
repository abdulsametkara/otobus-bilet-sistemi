using OtobusBiletSistemi.Mobile.ViewModels;

namespace OtobusBiletSistemi.Mobile.Views;

public partial class BiletSahibiBilgileriPage : ContentPage
{
    public BiletSahibiBilgileriPage(BiletSahibiBilgileriViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
} 