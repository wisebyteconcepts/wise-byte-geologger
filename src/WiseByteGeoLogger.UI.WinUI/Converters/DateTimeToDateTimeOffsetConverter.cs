

using Microsoft.UI.Xaml.Data;

namespace WiseByteGeoLogger.UI.WinUI.Converters;

public class DateTimeToDateTimeOffsetConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is DateTime dateTime)
        {
            return new DateTimeOffset(dateTime);
        }
        return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if(value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.DateTime;
        }
        return value;
    }
}
