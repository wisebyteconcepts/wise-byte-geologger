using Microsoft.UI.Xaml.Controls;

using WiseByteGeoLogger.UI.WinUI.ViewModels;

namespace WiseByteGeoLogger.UI.WinUI.Views;

public sealed partial class StamperPage : Page
{
    public StamperViewModel ViewModel
    {
        get;
    }

    public StamperPage()
    {
        ViewModel = App.GetService<StamperViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
