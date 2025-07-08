using OtobusBiletSistemi.Mobile.ViewModels;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace OtobusBiletSistemi.Mobile.Views;

public partial class KoltukSecimPage : ContentPage
{
    private KoltukSecimViewModel _viewModel;

    public KoltukSecimPage(KoltukSecimViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        Debug.WriteLine("🚌 KoltukSecimPage constructor çağrıldı");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine($"🚌 KoltukSecimPage - OnAppearing (Manual Navigation Mode)");
    }

    private void OnSeatClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("🎯 OnSeatClicked çağrıldı!");
        
        if (sender is not Button button)
        {
            Debug.WriteLine("❌ Sender Button değil!");
            return;
        }
        
        if (_viewModel == null)
        {
            Debug.WriteLine("❌ ViewModel bulunamadı!");
            return;
        }
        
        // Button'ın BindingContext'inden koltuk bilgisini al
        var rowContext = button.BindingContext as KoltukRow;
        if (rowContext == null)
        {
            Debug.WriteLine("❌ Row context bulunamadı!");
            return;
        }
        
        // Hangi koltuk tıklandığını belirle
        KoltukViewModel? koltukVm = null;
        
        // Button'ın Grid.Column pozisyonuna göre belirle
        if (button.StyleId == "A" || (button.Parent is Grid grid && Grid.GetColumn(button) == 0))
            koltukVm = rowContext.SeatA;
        else if (button.StyleId == "B" || (button.Parent is Grid grid2 && Grid.GetColumn(button) == 1))
            koltukVm = rowContext.SeatB;
        else if (button.StyleId == "C" || (button.Parent is Grid grid3 && Grid.GetColumn(button) == 3))
            koltukVm = rowContext.SeatC;
        else if (button.StyleId == "D" || (button.Parent is Grid grid4 && Grid.GetColumn(button) == 4))
            koltukVm = rowContext.SeatD;
        
        if (koltukVm == null)
        {
            Debug.WriteLine("❌ Koltuk ViewModel bulunamadı!");
            return;
        }
        
        Debug.WriteLine($"✅ Koltuk bulundu: {koltukVm.KoltukNo}, Tıklama işleniyor...");
        
        // ViewModel'deki SelectKoltukCommand'ı çağır
        _viewModel.SelectKoltukCommand.Execute(koltukVm);
    }
} 