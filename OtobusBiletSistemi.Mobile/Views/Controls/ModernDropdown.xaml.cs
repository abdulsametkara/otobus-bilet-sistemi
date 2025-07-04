using System.Collections;
using Microsoft.Maui.Controls;

namespace OtobusBiletSistemi.Mobile.Views.Controls;

public partial class ModernDropdown : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ModernDropdown), "Seçiniz");
    
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(ModernDropdown), null);
    
    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(ModernDropdown), null,
            propertyChanged: OnSelectedItemChanged);
    
    public static readonly BindableProperty SelectedTextProperty =
        BindableProperty.Create(nameof(SelectedText), typeof(string), typeof(ModernDropdown), "");

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public string SelectedText
    {
        get => (string)GetValue(SelectedTextProperty);
        set => SetValue(SelectedTextProperty, value);
    }

    public event EventHandler<object>? SelectedItemChanged;

    public ModernDropdown()
    {
        InitializeComponent();
        SelectedText = Title;
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernDropdown dropdown)
        {
            dropdown.SelectedText = newValue?.ToString() ?? dropdown.Title;
            if (newValue != null)
                dropdown.SelectedItemChanged?.Invoke(dropdown, newValue);
        }
    }

    private void OnDropdownTapped(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = true;
    }

    private void OnOverlayTapped(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = false;
    }

    private void OnClosePopup(object sender, EventArgs e)
    {
        PopupOverlay.IsVisible = false;
    }

    private void OnItemSelected(object sender, EventArgs e)
    {
        if (sender is Grid grid && grid.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tapGesture)
        {
            var selectedItem = tapGesture.CommandParameter;
            if (selectedItem != null)
            {
                SelectedItem = selectedItem;
                PopupOverlay.IsVisible = false;
            }
        }
    }
} 