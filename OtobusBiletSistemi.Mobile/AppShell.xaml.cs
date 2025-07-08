using OtobusBiletSistemi.Mobile.Views;

namespace OtobusBiletSistemi.Mobile;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register navigation routes
		Routing.RegisterRoute("koltuklarpage", typeof(KoltukSecimPage));
		Routing.RegisterRoute("KoltukSecimPage", typeof(KoltukSecimPage));
		Routing.RegisterRoute("BiletSahibiBilgileriPage", typeof(BiletSahibiBilgileriPage));
		Routing.RegisterRoute("OdemePage", typeof(OdemePage));
	}
}
