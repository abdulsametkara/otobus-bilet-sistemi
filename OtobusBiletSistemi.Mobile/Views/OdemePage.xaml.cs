using OtobusBiletSistemi.Mobile.ViewModels;

namespace OtobusBiletSistemi.Mobile.Views;

public partial class OdemePage : ContentPage
{
	public OdemePage(OdemeViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
} 