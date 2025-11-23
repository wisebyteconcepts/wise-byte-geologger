
using Newtonsoft.Json;

using WiseByteGeoLogger.Core.Contracts;
using WiseByteGeoLogger.Core.Models;

namespace WiseByteGeoLogger.Core.Services;

public class LocationIqLocationService : ILocationService
{
    public string APIKey
    {
        get; private set;
    } = string.Empty;

    public string StaticMapsSavePath
    {
        get; private set;
    } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "WiseByteGeoLogger", "StaticMaps");

    private static readonly string LocationIqBaseUrl = "https://us1.locationiq.com/v1/reverse";
    private static readonly string LocationIqImageBaseUrl = "https://maps.locationiq.com/v3/staticmap";

    public async Task<Location> GetLocationAsync(double latitude, double longitude)
    {

        if (string.IsNullOrEmpty(APIKey))
        {
            throw new InvalidOperationException("API Key is not set. Please set the API Key before making requests.");
        }

        using var client = new HttpClient();

        // Build the URL with query parameters
        var url = $"{LocationIqBaseUrl}?key={APIKey}&lat={latitude}&lon={longitude}&format=json&normalizeaddress=1";

        // Send the HTTP request to LocationIQ API
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();  // Throws an exception if the HTTP request fails

        // Read the JSON response
        var jsonResponse = await response.Content.ReadAsStringAsync();

        // Deserialize the JSON response to a LocationIQResponse object
        var location = JsonConvert.DeserializeObject<Location>(jsonResponse);

        return location ?? throw new Exception("Failed to parse LocationIQ response.");
    }
    public async Task<string> GetSatelliteImagePath(double latitude, double longitude, int zoom = 8, int width = 600, int height = 600)
    {
        if (string.IsNullOrEmpty(APIKey))
        {
            throw new InvalidOperationException("API Key is not set. Please set the API Key before making requests.");
        }

        // Ensure the save directory exists
        Directory.CreateDirectory(StaticMapsSavePath);

        using var client = new HttpClient();

        var marker = $"icon:large-red-cutout|{latitude},{longitude}";

        // Build the URL with query parameters
        var url = $"{LocationIqImageBaseUrl}?key={APIKey}&center={latitude},{longitude}&zoom={zoom}&size={width}x{height}&format=jpg&maptype=streets";

        // Send the HTTP request to LocationIQ Static Map API
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();  // Throws an exception if the HTTP request fails

        // Read the image content
        var imageBytes = await response.Content.ReadAsByteArrayAsync();

        // Save the image to disk
        //var fileName = Path.Combine(StaticMapsSavePath, "static_map.jpg");
        var filePath = EnsureUniqueFilePath(Path.Combine(StaticMapsSavePath, "static_map.jpg"));
        await File.WriteAllBytesAsync(filePath, imageBytes);

        return filePath;
    }

    private string EnsureUniqueFilePath(string fileName)
    {
        var directory = Path.GetDirectoryName(fileName)!;
        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        var finalPath = fileName;
        var counter = 1;

        while (File.Exists(finalPath))
        {
            finalPath = Path.Combine(directory, $"{name}_{counter}{ext}");
            counter++;
        }

        return finalPath;
    }
    public void SetAPIKey(string apiKey) => APIKey = apiKey;
    public void SetStaticMapsSavePath(string path) => StaticMapsSavePath = path;
}
