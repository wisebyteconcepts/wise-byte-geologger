using WiseByteGeoLogger.Core.Models;

namespace WiseByteGeoLogger.Core.Contracts;

public interface ILocationService
{
    string APIKey
    {
        get;
    }

    string StaticMapsSavePath
    {
        get;
    }


    void SetAPIKey(string apiKey);

    void SetStaticMapsSavePath(string path);

    Task<Location> GetLocationAsync(double latitude, double longitude);
    Task<string> GetSatelliteImagePath(double latitude, double longitude, int zoom = 8, int width = 600, int height = 600);

}
