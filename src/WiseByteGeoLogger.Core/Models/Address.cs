using Newtonsoft.Json;

namespace WiseByteGeoLogger.Core.Models;

public class Address
{
    [JsonProperty("town")]
    public string Town { get; set; } = string.Empty;

    [JsonProperty("county")]
    public string County { get; set; } = string.Empty;

    [JsonProperty("state_district")]
    public string StateDistrict { get; set; } = string.Empty;

    [JsonProperty("state")]
    public string State { get; set; } = string.Empty;

    [JsonProperty("postcode")]
    public string Postcode { get; set; } = string.Empty;

    [JsonProperty("country")]
    public string Country { get; set; } = string.Empty;

    [JsonProperty("country_code")]
    public string CountryCode { get; set; } = string.Empty;
}