namespace WiseByteGeoLogger.UI.WinUI.Contracts;

public interface INavigationAware
{
    void OnNavigatedTo(object parameter);

    void OnNavigatedFrom();
}
