using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Input;

using ColorCode.Compilation.Languages;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using InventoryPro.UI.WinUI.Helpers;

using Microsoft.UI.Xaml;

using UpdaterPro.Core;
using UpdaterPro.Core.Models;

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

    [ObservableProperty]
    private string currentUpdateOperation = string.Empty;

    [ObservableProperty]
    private double updateProgress = 0;

    [ObservableProperty]
    private string updateStatus = "";

    [ObservableProperty]
    private bool isWorkingForUpdates, isUpdating;

    [ObservableProperty]
    private CancellationTokenSource? cts;


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

    [RelayCommand]
    internal async Task CheckForUpdate()
    {

        try
        {
    
            const string UpdateBaseUrlOrPath = "https://www.dropbox.com/scl/fi/9h3aqcy2jt1fl18gxw3r6/WiseByteGeoLogger_UpdateBase.json?rlkey=9mzqctw8xk44ye1ysl2t7fxdo&st=ujtu7xtk&dl=1";

    // you can use your own json file url here
    var updater = new UpdaterManager(UpdateBaseUrlOrPath);

            Cts = new CancellationTokenSource();
            IsWorkingForUpdates = true;
            CurrentUpdateOperation = "Checking for updates";

            var progress = new Progress<UpdateProgress>(p =>
            {
                UpdateProgress = p.Progress * 100; // Set your progress bar
                UpdateStatus = p.StatusMessage; // Update your UI or log
            });

            // Check for updates
            var updates = await updater.CheckForUpdate(Cts.Token, progress);

            if (!(updates?.Count > 0))
            {
                IsWorkingForUpdates = false;
                await App.MainWindow.ShowMessageDialogAsync("You are running the latest version of the application", "Check For Update");
                return;
            }

            var latestUpdate = updates.FirstOrDefault() ?? throw new Exception("Latest update not found");

            var features = string.Join(Environment.NewLine, latestUpdate.ReleaseNote?.Select(note => $"- {note}") ?? Array.Empty<string>());

            // update available
            var reply = await App.MainWindow.ConfirmAsync(
                $"An update is available for {latestUpdate.ApplicationName}.\n" +
                $"Available Version: {latestUpdate.UpdatedVersion}\n" +
                (features.Length > 0 ? $"Release Notes:\n{features}\n\n" : "") +
                "Do you want to restart the application to update to the latest version?",
                "Update Available", "Yes", "No");

            CurrentUpdateOperation = "Updating";

            // start update
            if (reply == true)
            {
                IsUpdating = true;
                await UpdaterManager.UpdateAsync(updates, Cts.Token, progress);
            }
        }
        catch (Exception ex)
        {
            await App.MainWindow.ShowMessageDialogAsync($"Error updating application: {ex.Message}");
        }

        IsWorkingForUpdates = false;
        IsUpdating = false;



    }

    [RelayCommand]
    private void CancelCheckForUpdate()
    {
        Cts?.Cancel();
        CurrentUpdateOperation = "Cancelling";
        UpdateStatus = "Operation is being cancelled...";
    }


    [RelayCommand]
    private static async Task GoToSourceCode()
    {
        try
        {
            _ = await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/wisebyteconcepts/wise-byte-geologger"));
        }
        catch (Exception ex)
        {
            await App.MainWindow.ShowMessageDialogAsync(ex.Message, "Error opening source page");
        }
    }


    [RelayCommand]
    private static async Task GoToLocationIq()
    {
        try
        {
            const string locationIqUrl = "https://my.locationiq.com/dashboard?ref=locationiq#accesstoken";

            _ = await Windows.System.Launcher.LaunchUriAsync(new Uri(locationIqUrl));
        }
        catch (Exception ex)
        {
            await App.MainWindow.ShowMessageDialogAsync(ex.Message, "Error Opening API Key Page");
        }
    }



}
