
using Microsoft.UI.Xaml.Data;

namespace WiseByteGeoLogger.UI.WinUI.Converters;

public class DateTimeToTimeSpanConverter : IValueConverter
{
    DateTime actualDate;
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if(value is DateTime dateTime)
        {
            actualDate = dateTime;
            return dateTime.TimeOfDay;
        }
        return value;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        
        if(value is TimeSpan timeSpan)
        {
            return actualDate.Date.Add(timeSpan);
        }
        return value;
    }
}
