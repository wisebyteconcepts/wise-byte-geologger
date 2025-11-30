using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WiseByteGeoLogger.Core.Contracts;
using WiseByteGeoLogger.Core.Models;
using WiseByteGeoLogger.Core.Services;
using WiseByteGeoLogger.UI.WinUI.Contracts;
using WiseByteGeoLogger.UI.WinUI.Helpers;
using static WiseByteGeoLogger.UI.WinUI.Helpers.FilePickerHelper;
using WiseByteGeoLogger.UI.WinUI.Services;

using static WiseByteGeoLogger.UI.WinUI.Helpers.DialogHelper;

namespace WiseByteGeoLogger.UI.WinUI.ViewModels;

public partial class StamperViewModel : ObservableObject
{
    private readonly ILocationService locationService;
    private readonly IImageManipulationService imageManipulationService;
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

    public StamperViewModel(ILocationService locationService, IImageManipulationService imageManipulationService, INavigationService navigationService, ILocalSettingsService localSettingsService)
    {
        this.locationService = locationService;
        this.imageManipulationService = imageManipulationService;
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
            _ = double.TryParse(Latitude, out var lat);
            _ = double.TryParse(Longitude, out var lon);


            var files = await SelectFiles([".png", ".jpg", ".jpeg", ".bmp", ".gif"]);

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
                        },
                        DateTime = Date.Date + Time

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
    internal async Task BatchProcess()
    {
        // Select a directory

        if (string.IsNullOrWhiteSpace(OutputDirectory))
        {
            try
            {
                var folder = await SelectFolder();
                if (folder != null)
                {
                    OutputDirectory = folder.Path;

                    if (!Directory.Exists(OutputDirectory))
                    {
                        Directory.CreateDirectory(OutputDirectory);
                    }
                }
                else
                {
                    await App.MainWindow.ShowMessageDialogAsync("Output directory selection was cancelled.", "Operation Cancelled");
                    return;
                }
            }
            catch (Exception ex)
            {
                await App.MainWindow.ShowMessageDialogAsync(ex.Message);
            }
        }

        if (Stamps.Count < 1)
        {
            await App.MainWindow.ShowMessageDialogAsync("Load atleast one image to process", "No Image to Process");
            return;
        }

        foreach (var stampBackground in Stamps)
        {
            try
            {
                //Latitude = "26.187394";
                //Longitude = "91.563845";

                _ = double.TryParse(Latitude, out var lat);
                _ = double.TryParse(Longitude, out var lon);

                if (stampBackground.Location.Latitude == 0)
                {
                    if (lat == 0) { throw new Exception("No Latitute given"); }

                    stampBackground.Location.Latitude = lat;
                }

                if (stampBackground.Location.Longitude == 0)
                {
                    if (lon == 0) { throw new Exception("No Longitude given"); }

                    stampBackground.Location.Longitude = lon;
                }


                if (OutputDirectory == null) { throw new Exception("Output directory can not be null"); }
                if (stampBackground.BackgroundImagePath == null || string.IsNullOrWhiteSpace(stampBackground.BackgroundImagePath)) { throw new Exception("File name can not be null"); }


                var dateTime = stampBackground.DateTime;
                var outputFileName = GetUniqueFileName(Path.Combine(OutputDirectory, Path.GetFileName(stampBackground.BackgroundImagePath)), dateTime);

                var location = await locationService.GetLocationAsync(lat, lon);
                var address = location != null ? location.DisplayName : throw new Exception("Unable to retrieve address for the provided coordinates.");
                var sateliteImagePath = await locationService.GetSatelliteImagePath(lat, lon);
                //var sateliteImagePath = await LocationService.GetSatelliteImagePath(lat, lon, staticMapFile, 13);

                if (!File.Exists(sateliteImagePath))
                {
                    throw new Exception("Static Map image can not be found");
                }


                var stamp = new Stamp
                {
                    GPSMapImagePath = sateliteImagePath,
                    Location = location,
                    DateTime = dateTime
                };


                var file = imageManipulationService.StampImage(stampBackground.BackgroundImagePath, stamp, outputFileName);
                // PhotoMetadataService.UpdateMetadata(file, GetUniqueFileName(file), lat, lon, stamp.DateTime);

            }
            catch (Exception ex)
            {
                await App.MainWindow.ShowMessageDialogAsync(ex.Message);
            }
        }
    }


    private static string GetUniqueFileName(string path, DateTime dateTime)
    {
        // Get the file name without extension
        var fileName = $"GPSCamera_{dateTime:yyyyMMdd_hhmmtt}";
        // Get the file extension
        var extension = Path.GetExtension(path);
        // Get the directory path
        var directory = Path.GetDirectoryName(path);

        if (directory == null || string.IsNullOrWhiteSpace(directory)) { throw new Exception("Directory can not be null"); }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Generate the new file path with the counter
        var newPath = Path.Combine(directory, $"{fileName}{extension}");

        for (var counter = 1; File.Exists(newPath); counter++)
        {
            newPath = Path.Combine(directory, $"{fileName} ({counter}){extension}");
        }

        return newPath;
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
