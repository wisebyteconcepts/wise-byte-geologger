using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.UI.Xaml;

using Windows.ApplicationModel;

using WiseByteGeoLogger.UI.WinUI.Contracts;
using WiseByteGeoLogger.UI.WinUI.Helpers;

using static WiseByteGeoLogger.UI.WinUI.Helpers.DialogHelper;

namespace WiseByteGeoLogger.UI.WinUI.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService localSettingsService;
    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    public ICommand SwitchThemeCommand
    {
        get;
    }

    [ObservableProperty]
    private string aPIKey = string.Empty;

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        this.localSettingsService = localSettingsService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        _ = GetAPIKey();
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }


    [RelayCommand]
    private async Task SaveAPIKey()
    {
        try
        {
            if (string.IsNullOrEmpty(APIKey))
            {
                throw new Exception("API Key cannot be empty.");
            }

            await localSettingsService.SaveSettingAsync(nameof(APIKey), APIKey);

        }
        catch (Exception ex)
        {
            await MessageDialogAsync(ex.Message, "Error Saving API Key");
        }
    }

    private async Task GetAPIKey()
    {
        try
        {
            var key = await localSettingsService.ReadSettingAsync<string>(nameof(APIKey));
            if (!string.IsNullOrEmpty(key))
            {
                APIKey = key;
            }
        }
        catch (Exception ex)
        {
            await MessageDialogAsync(ex.Message, "Error Saving API Key");
        }
    }
}
