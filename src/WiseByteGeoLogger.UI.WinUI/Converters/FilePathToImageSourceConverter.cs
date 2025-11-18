
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace WiseByteGeoLogger.UI.WinUI.Converters;

public class FilePathToImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string filePath && !string.IsNullOrEmpty(filePath))
        {
            try
            {
                var uri = new Uri(filePath);
                var bitmapImage = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(uri);
                return bitmapImage;
            }
            catch
            {
                // Handle invalid file path or URI format
                return null;
            }
        }
        return null;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
