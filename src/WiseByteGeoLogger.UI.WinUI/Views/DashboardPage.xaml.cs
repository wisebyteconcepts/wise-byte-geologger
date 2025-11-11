using Microsoft.UI.Xaml.Controls;

using WiseByteGeoLogger.UI.WinUI.ViewModels;

namespace WiseByteGeoLogger.UI.WinUI.Views;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel
    {
        get;
    }

    public DashboardPage()
    {
        ViewModel = App.GetService<DashboardViewModel>();
        InitializeComponent();
    }
}
