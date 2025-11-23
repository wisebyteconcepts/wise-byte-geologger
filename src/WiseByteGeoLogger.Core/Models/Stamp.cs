namespace WiseByteGeoLogger.Core.Models;

public class Stamp
{
    public Location Location
    {
        get; set;
    } = new Location();

    public string GPSMapImagePath
    {
        get; set;
    } = string.Empty;
    public string? BackgroundImagePath
    {
        get; set;
    }

    public DateTime DateTime
    {
        get; set;
    } = DateTime.Now;


}
