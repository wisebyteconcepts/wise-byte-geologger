using System.Globalization;

using ImageMagick;
using ImageMagick.Drawing;

using WiseByteGeoLogger.Core.Contracts;
using WiseByteGeoLogger.Core.Models;

namespace WiseByteGeoLogger.Core.Services;

public class ImageManipulationServiceMagick : IImageManipulationService
{
    public string StampImage(string backgroundPath, Stamp stamp, string outputFileName)
    {
        ValidateStampInputs(stamp, backgroundPath, outputFileName);

        const int overlayHeight = 180;
        const int overlayWidth = 1080;
        const int defaultPadding = 10;
        const int spaceBetweenLines = 15;
        const int spaceBetweenCaptionAndValue = 10;
        const int spaceBetweenMapImageAndMapInfo = 30;
        const int mapMarkerSize = 40;

        using var background = new MagickImage(backgroundPath);
        using var mapImage = new MagickImage(stamp.GPSMapImagePath);
        using var mapMarker = new MagickImage("Assets/MapMarker.png");

        mapMarker.Resize(mapMarkerSize, mapMarkerSize);

        mapImage.Resize(overlayHeight - (defaultPadding * 2), overlayHeight - (defaultPadding * 2));

        // Draw map marker centered
        var markerX = (mapImage.Width / 2) - (mapMarker.Width / 2);
        var markerY = (mapImage.Height / 2) - mapMarker.Height + 10;
        mapImage.Composite(mapMarker, (int)markerX, (int)markerY, CompositeOperator.Over);

        // Create semi-transparent overlay
        using var overlay = new MagickImage(new MagickColor("rgba(0,0,0,0.5)"), overlayWidth, overlayHeight);

        var firstColumn = mapImage.Width + spaceBetweenMapImageAndMapInfo;

        // Setup text drawing
        var titleFontSize = 30;
        var defaultFontSize = 22;

        var draw = new Drawables()
            .Font("SFUIText-Bold")
            .FontPointSize(titleFontSize)
            .FillColor(MagickColors.White);

        // Measure title
        var titleMetrics = overlay.FontTypeMetrics(stamp.Location.DisplayName);
        if (titleMetrics!.TextWidth > overlayWidth - firstColumn)
        {
            var adjusted = 50;
            while (titleMetrics != null && titleMetrics.TextWidth > overlayWidth - firstColumn && adjusted > 25)
            {
                adjusted -= 5;
                draw.FontPointSize(adjusted);
                titleMetrics = overlay.FontTypeMetrics(stamp.Location.DisplayName);
            }
        }

        var captionTop = markerY;
        var firstLineTop = markerY + titleMetrics!.TextHeight + 5;

        draw.Text(firstColumn, captionTop, stamp.Location.DisplayName);

        // Captions
        var captionFontSize = defaultFontSize;
        var captionDraw = new Drawables()
            .Font("SFUIText-Regular")
            .FontPointSize(captionFontSize)
            .FillColor(MagickColors.White);

        var valueDraw = new Drawables()
            .Font("SFUIText-Bold")
            .FontPointSize(defaultFontSize)
            .FillColor(MagickColors.White);

        // Second column
        var secondColumn = (overlayWidth / 2) - 50;

        // Latitude
        captionDraw.Text(firstColumn, firstLineTop + spaceBetweenLines, "Latitude:");
        var latX = (int)(firstColumn + overlay.FontTypeMetrics("Latitude:")!.TextWidth + spaceBetweenCaptionAndValue);
        valueDraw.Text(latX, firstLineTop + spaceBetweenLines, stamp.Location.Latitude.ToString("F6"));

        // Longitude
        captionDraw.Text(firstColumn, firstLineTop + spaceBetweenLines * 2 + 20, "Longitude:");
        var lonX = (int)(firstColumn + overlay.FontTypeMetrics("Longitude:")!.TextWidth + spaceBetweenCaptionAndValue);
        valueDraw.Text(lonX, firstLineTop + spaceBetweenLines * 2 + 20, stamp.Location.Longitude.ToString("F6"));

        // Date
        captionDraw.Text(secondColumn, firstLineTop + spaceBetweenLines, "Date:");
        var dateX = (int)(secondColumn + overlay.FontTypeMetrics("Date:")!.TextWidth + spaceBetweenCaptionAndValue);
        valueDraw.Text(dateX, firstLineTop + spaceBetweenLines, stamp.DateTime.ToString("dd-MM-yyyy"));

        // Time
        captionDraw.Text(secondColumn, firstLineTop + spaceBetweenLines * 2 + 20, "Time:");
        var timeX = (int)(secondColumn + overlay.FontTypeMetrics("Time:")!.TextWidth + spaceBetweenCaptionAndValue);
        valueDraw.Text(timeX, firstLineTop + spaceBetweenLines * 2 + 20, stamp.DateTime.ToShortTimeString());

        // Apply text
        overlay.Composite(mapImage, defaultPadding, defaultPadding, CompositeOperator.Over);
        draw.Draw(overlay);
        captionDraw.Draw(overlay);
        valueDraw.Draw(overlay);

        // Resize background
        if (background.Width > background.Height)
            background.Resize(new MagickGeometry(1920, 1080) { IgnoreAspectRatio = false });
        else
            background.Resize(new MagickGeometry(1080, 1920) { IgnoreAspectRatio = false });

        var overlayX = (background.Width > background.Height)
            ? (background.Width / 2) - (overlay.Width / 2)
            : 0;
        var overlayY = background.Height - overlay.Height;

        background.Composite(overlay, (int)overlayX, (int)overlayY, CompositeOperator.Over);

        ApplyExifMetadata(background, stamp);

        background.Write(outputFileName);

        if (File.Exists(stamp.GPSMapImagePath))
            File.Delete(stamp.GPSMapImagePath);

        return outputFileName;
    }

    public static void ApplyExifMetadata(MagickImage image, Stamp stamp)
    {
        var profile = image.GetExifProfile() ?? new ImageMagick.ExifProfile();

        var formatted = stamp.DateTime.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

        profile.SetValue(ImageMagick.ExifTag.DateTimeOriginal, formatted);
        profile.SetValue(ExifTag.DateTimeDigitized, formatted);
        profile.SetValue(ExifTag.DateTime, formatted);

        // GPS
        profile.SetValue(ExifTag.GPSLatitudeRef, stamp.Location.Latitude >= 0 ? "N" : "S");
        profile.SetValue(ExifTag.GPSLatitude, ToRationalGPS(stamp.Location.Latitude));

        profile.SetValue(ExifTag.GPSLongitudeRef, stamp.Location.Longitude >= 0 ? "E" : "W");
        profile.SetValue(ExifTag.GPSLongitude, ToRationalGPS(stamp.Location.Longitude));

        // Remove unwanted
        profile.RemoveValue(ExifTag.Artist);
        profile.RemoveValue(ExifTag.HostComputer);
        profile.RemoveValue(ExifTag.OwnerName);

        image.SetProfile(profile);
    }

    private static Rational[] ToRationalGPS(double value)
    {
        value = Math.Abs(value);
        var degrees = (int)value;
        var minutesDecimal = (value - degrees) * 60;
        var minutes = (int)minutesDecimal;
        var seconds = (minutesDecimal - minutes) * 60;

        return
        [
            new Rational(degrees, true),
            new Rational(minutes, true),
            new Rational((int)(seconds * 10000), true)
        ];
    }

    #region Helper Methods

    private static void ValidateStampInputs(
    Stamp? stamp,
    string? backgroundPath,
    string? outputFileName)
    {
        List<string> errors = [];

        if (stamp == null)
        {
            errors.Add("Stamp object is null.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(stamp.GPSMapImagePath))
                errors.Add("Stamp.GPSMapImagePath is null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(backgroundPath))
            errors.Add("Background path is null, empty, or whitespace.");

        if (string.IsNullOrWhiteSpace(outputFileName))
            errors.Add("Output file name is null, empty, or whitespace.");

        if (errors.Count > 0)
            throw new ArgumentException("Invalid arguments:\n" + string.Join("\n", errors));
    }


    #endregion
}
