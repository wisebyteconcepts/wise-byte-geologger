using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using WiseByteGeoLogger.Core.Contracts;
using WiseByteGeoLogger.Core.Models;
using WiseByteGeoLogger.UI.WinUI.Contracts;
using WiseByteGeoLogger.UI.WinUI.Helpers;
using WiseByteGeoLogger.UI.WinUI.Services;

using static WiseByteGeoLogger.UI.WinUI.Helpers.DialogHelper;

namespace WiseByteGeoLogger.UI.WinUI.ViewModels;

public partial class StamperViewModel : ObservableObject
{
    private readonly ILocationService locationService;
    private readonly INavigationService navigationService;
    private readonly ILocalSettingsService localSettingsService;
    [ObservableProperty]
    private string latitude = string.Empty;

    [ObservableProperty]
    private string longitude = string.Empty;

    [ObservableProperty]
    private string staticMapPath = string.Empty;


    [ObservableProperty]
    private ObservableCollection<Stamp> stamps = [];

    [ObservableProperty]
    private string? _outputDirectory;

    [ObservableProperty]
    private Stamp? selectedStamp;


    [ObservableProperty]
    private DateTimeOffset date = DateTime.Now;

    [ObservableProperty]
    private TimeSpan time = DateTime.Now.TimeOfDay;

    public StamperViewModel(ILocationService locationService, INavigationService navigationService, ILocalSettingsService localSettingsService)
    {
        this.locationService = locationService;
        this.navigationService = navigationService;
        this.localSettingsService = localSettingsService;

        _ = InitializeAsync();

    }

    internal async Task InitializeAsync()
    {
        try
        {
            var apikey = await localSettingsService.ReadSettingAsync<string>("APIKey");
            if (string.IsNullOrEmpty(apikey))
            {
                await MessageDialogAsync("No API Key found from settings. Please set it in Settings page.", "API Key is empty");
                navigationService.NavigateTo(typeof(SettingsViewModel).FullName!);
                return;
            }

            locationService.SetAPIKey(apikey);
        }
        catch (Exception)
        {
            await MessageDialogAsync("Failed to load API Key from settings. Please set it in Settings page.", "Error");
        }
    }

    [RelayCommand]
    private async Task GetLocation()
    {
        if (string.IsNullOrEmpty(Latitude) || string.IsNullOrEmpty(Longitude)) { return; }
        if (!double.TryParse(Latitude, out var lat) || !double.TryParse(Longitude, out var lon)) { return; }


        var location = await locationService.GetLocationAsync(lat, lon);
        StaticMapPath = await locationService.GetSatelliteImagePath(lat, lon, 12);

        await App.MainWindow.ShowMessageDialogAsync($"Location: {location.DisplayName}", "Location Retrieved");

    }

    [RelayCommand]
    private async Task SelectImages()
    {
        try
        {
            if (!double.TryParse(Latitude, out var lat) || !double.TryParse(Longitude, out var lon))
            {
                await MessageDialogAsync("Please enter valid latitude and longitude values.", "Invalid Input");
                return;
            }

            var files = await FilePickerHelper.SelectFiles([".png", ".jpg", ".jpeg", ".bmp", ".gif"]);

            if (files?.Count > 0)
            {
                foreach (var file in files)
                {
                    Stamps.Add(new Stamp
                    {
                        BackgroundImagePath = file.Path,
                        Location = new Location
                        {
                            Latitude = lat,
                            Longitude = lon
                        }
                    });
                }
            }

        }
        catch (Exception ex)
        {
            await MessageDialogAsync(ex.Message, "Error Selecting Images");
        }
    }

    [RelayCommand]
    private void RemoveSelectedStamp(Stamp stamp)
    {
        if (stamp != null) { Stamps.Remove(stamp); }
    }
    [RelayCommand]
    private void ClearAll()
    {
        Stamps.Clear();
    }


    [RelayCommand]
    internal async Task GoBack()
    {
        try
        {
            navigationService.GoBack();
        }
        catch (Exception ex)
        {
            await MessageDialogAsync($"Error navigating back: {ex.Message}");
        }
    }
}
