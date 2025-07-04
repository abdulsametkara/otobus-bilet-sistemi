using System.Diagnostics;

namespace OtobusBiletSistemi.Mobile;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		Debug.WriteLine("OtobusBiletSistemi.Mobile uygulaması başlatılıyor...");
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		Debug.WriteLine("Ana pencere oluşturuluyor...");
		var window = new Window(new AppShell());
		Debug.WriteLine("AppShell oluşturuldu ve pencere ayarlandı.");
		return window;
	}
	
	protected override void OnStart()
	{
		Debug.WriteLine("Uygulama OnStart çağrıldı");
		base.OnStart();
	}
	
	protected override void OnResume()
	{
		Debug.WriteLine("Uygulama OnResume çağrıldı");
		base.OnResume();
	}
	
	protected override void OnSleep()
	{
		Debug.WriteLine("Uygulama OnSleep çağrıldı");
		base.OnSleep();
	}
}