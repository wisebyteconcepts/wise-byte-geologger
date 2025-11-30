using WiseByteGeoLogger.Core.Models;

namespace WiseByteGeoLogger.Core.Contracts;

public interface IImageManipulationService
{
    string StampImage(string backgroundPath, Stamp stamp, string outputFileName);
}