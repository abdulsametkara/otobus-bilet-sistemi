using OtobusBiletSistemi.Mobile.ViewModels;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace OtobusBiletSistemi.Mobile.Views;

public partial class KoltukSecimPage : ContentPage, IQueryAttributable
{
    private KoltukSecimViewModel _viewModel;
    private int _seferId = 0;
    private int _yolcuSayisi = 1;
    private bool _parametersReceived = false;
    private bool _pageAppeared = false;

    public KoltukSecimPage(KoltukSecimViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        Debug.WriteLine("🚌 KoltukSecimPage constructor çağrıldı");
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Debug.WriteLine("🚌 ApplyQueryAttributes çağrıldı");
        
        if (query.ContainsKey("seferId"))
        {
            var seferIdStr = query["seferId"].ToString();
            Debug.WriteLine($"🚌 SeferId parameter alındı: '{seferIdStr}'");
            
            if (int.TryParse(seferIdStr, out int seferId))
            {
                _seferId = seferId;
                Debug.WriteLine($"🚌 SeferId parse edildi: {_seferId}");
            }
            else
            {
                Debug.WriteLine($"❌ SeferId parse edilemedi: '{seferIdStr}'");
            }
        }
        
        if (query.ContainsKey("yolcuSayisi"))
        {
            var yolcuSayisiStr = query["yolcuSayisi"].ToString();
            Debug.WriteLine($"🚌 YolcuSayisi parameter alındı: '{yolcuSayisiStr}'");
            
            if (int.TryParse(yolcuSayisiStr, out int yolcuSayisi))
            {
                _yolcuSayisi = yolcuSayisi;
                Debug.WriteLine($"🚌 YolcuSayisi parse edildi: {_yolcuSayisi}");
            }
            else
            {
                Debug.WriteLine($"❌ YolcuSayisi parse edilemedi: '{yolcuSayisiStr}'");
            }
        }
        
        _parametersReceived = true;
        Debug.WriteLine($"🚌 Final parameters - SeferId: {_seferId}, YolcuSayisi: {_yolcuSayisi}");
        
        // Eğer sayfa zaten appeared olduysa, hemen yükle
        if (_pageAppeared)
        {
            Debug.WriteLine("🚌 Sayfa zaten appeared, veri yükleniyor...");
            _ = LoadDataAsync();
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _pageAppeared = true;
        
        Debug.WriteLine($"🚌 KoltukSecimPage - OnAppearing başladı");
        
        // Parametrelerin gelmesini bekle (maksimum 3 saniye)
        await WaitForParametersAsync();
        
        if (_parametersReceived && _seferId > 0)
        {
            Debug.WriteLine($"🚌 Veri yükleme başlıyor - SeferId: {_seferId}, YolcuSayisi: {_yolcuSayisi}");
            await LoadDataAsync();
        }
        else
        {
            Debug.WriteLine($"❌ Parametreler eksik veya geçersiz! SeferId: {_seferId}, YolcuSayisi: {_yolcuSayisi}, ParametersReceived: {_parametersReceived}");
            await DisplayAlert("Hata", $"Parametreler eksik veya geçersiz!\nSeferId: {_seferId}\nYolcuSayisi: {_yolcuSayisi}", "Tamam");
        }
    }

    private async Task WaitForParametersAsync()
    {
        int attempts = 0;
        int maxAttempts = 30; // 3 saniye (30 x 100ms)
        
        while (!_parametersReceived && attempts < maxAttempts)
        {
            await Task.Delay(100); // 100ms bekle
            attempts++;
            
            if (attempts % 10 == 0) // Her saniyede bir log
            {
                Debug.WriteLine($"⏳ Parametreler bekleniyor... ({attempts * 100}ms)");
            }
        }
        
        if (_parametersReceived)
        {
            Debug.WriteLine($"✅ Parametreler alındı! ({attempts * 100}ms sonra)");
        }
        else
        {
            Debug.WriteLine($"⚠️ Parametreler {maxAttempts * 100}ms sonra hala gelmedi!");
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            Debug.WriteLine($"🔄 LoadDataAsync başlıyor - SeferId: {_seferId}, YolcuSayisi: {_yolcuSayisi}");
            await _viewModel.LoadKoltukDataAsync(_seferId, _yolcuSayisi);
            Debug.WriteLine($"✅ LoadDataAsync tamamlandı");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ LoadDataAsync hatası: {ex.Message}");
            await DisplayAlert("Hata", $"Veri yükleme hatası: {ex.Message}", "Tamam");
        }
    }
} 