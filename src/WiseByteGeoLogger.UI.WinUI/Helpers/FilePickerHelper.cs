using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage.Pickers;

using Windows.Storage;

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
            openPicker.FileTypeFilter.Add(fileType);
        }

        return await openPicker.PickMultipleFilesAsync();
    }
}
