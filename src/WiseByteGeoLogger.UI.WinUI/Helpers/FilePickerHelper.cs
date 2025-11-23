using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Pickers;

using static System.Net.Mime.MediaTypeNames;

namespace WiseByteGeoLogger.UI.WinUI.Helpers;

public static class FilePickerHelper
{
    public static async Task<IReadOnlyList<StorageFile>> SelectFiles(string[] fileTypes)
    {
        var openPicker = new FileOpenPicker();
        var window = App.MainWindow;
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, WinRT.Interop.WindowNative.GetWindowHandle(window));
        openPicker.SuggestedStartLocation = PickerLocationId.Desktop;

        foreach (var fileType in fileTypes)
        {
            if (string.IsNullOrWhiteSpace(fileType)) { continue; }
            if (!fileType.StartsWith(".")) { continue; }

            openPicker.FileTypeFilter.Add(fileType);
        }
     

        return await openPicker.PickMultipleFilesAsync();
    }

    public static async Task<StorageFolder> SelectFolder(PickerLocationId pickerLocationId = PickerLocationId.Desktop)
    {
        var openPicker = new FolderPicker();
        var window = App.MainWindow;
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, WinRT.Interop.WindowNative.GetWindowHandle(window));
        openPicker.SuggestedStartLocation = pickerLocationId;

        return await openPicker.PickSingleFolderAsync();
    }


}
