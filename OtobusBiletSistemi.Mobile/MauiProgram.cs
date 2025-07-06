using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using OtobusBiletSistemi.Mobile.Services;
using OtobusBiletSistemi.Mobile.ViewModels;
using OtobusBiletSistemi.Mobile.Views;
using System.Diagnostics;
using System.Globalization;

namespace OtobusBiletSistemi.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        var cultureInfo = new CultureInfo("tr-TR");
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // Oracle bağlantısı için sadece logging
        Debug.WriteLine("🔧 MauiProgram konfigürasyonu başlatılıyor...");

        // ApiService - Artık DI gerektirmiyor
        builder.Services.AddTransient<IApiService, ApiService>();

        // ViewModels
        builder.Services.AddTransient<SeferListViewModel>();
        builder.Services.AddTransient<KoltukSecimViewModel>();
        builder.Services.AddTransient<BiletSahibiBilgileriViewModel>();

        // Views
        builder.Services.AddTransient<SeferListPage>();
        builder.Services.AddTransient<KoltukSecimPage>();
        builder.Services.AddTransient<BiletSahibiBilgileriPage>();

        Debug.WriteLine("✅ MauiProgram konfigürasyonu tamamlandı");

        return builder.Build();
	}
}
