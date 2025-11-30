using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiseByteGeoLogger.UI.WinUI.Helpers;

public static class DialogHelper
{
    public static async Task MessageDialogAsync(string content, string title)
    {
        var window = App.MainWindow;
        await window.ShowMessageDialogAsync(content, title);
    }

    public static async Task MessageDialogAsync(string content)
    {
        var window = App.MainWindow;
        await window.ShowMessageDialogAsync(content);
    }
}
