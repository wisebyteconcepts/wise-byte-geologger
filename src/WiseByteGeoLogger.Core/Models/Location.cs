using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace WiseByteGeoLogger.Core.Models;

public class Location
{
    [JsonProperty("lat")]
    public double Latitude
    {
        get; set;
    }

    [JsonProperty("lon")]
    public double Longitude
    {
        get; set;
    }

    [JsonProperty("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonProperty("address")]
    public Address Address { get; set; } = new Address();
}
