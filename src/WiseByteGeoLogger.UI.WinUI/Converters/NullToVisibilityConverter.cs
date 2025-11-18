using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace WiseByteGeoLogger.UI.WinUI.Converters;

internal class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var reverse = false;

        // Try to parse the 'parameter' to determine if we need to reverse
        if (parameter is string paramStr && bool.TryParse(paramStr, out var parsed))
        {
            reverse = parsed;
        }

        var isVisible = value != null;

        // Reverse if needed
        if (reverse)
        {
            isVisible = !isVisible;
        }

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
