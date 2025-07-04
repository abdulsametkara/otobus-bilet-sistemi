using OtobusBiletSistemi.Mobile.ViewModels;

namespace OtobusBiletSistemi.Mobile.Views;

public partial class SeferListPage : ContentPage
{
    public SeferListPage(SeferListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}