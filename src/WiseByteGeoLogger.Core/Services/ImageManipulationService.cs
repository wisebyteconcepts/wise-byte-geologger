using System.Globalization;
using System.Reflection;

using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using WiseByteGeoLogger.Core.Contracts;
using WiseByteGeoLogger.Core.Helpers;

using WiseByteGeoLogger.Core.Models;

namespace WiseByteGeoLogger.Core.Services;

public class ImageManipulationService : IImageManipulationService
{

    public string StampImage(string backgroundPath, Stamp stamp, string outputFileName)
    {
        if (stamp == null || string.IsNullOrWhiteSpace(backgroundPath) ||
            string.IsNullOrWhiteSpace(outputFileName) ||
            string.IsNullOrWhiteSpace(stamp.GPSMapImagePath))
        {
            throw new ArgumentException("Invalid stamp or background path.");
        }

        const int spaceBetweenCaptionAndValue = 10;
        const int spaceBetweenLines = 15;
        const float titleFontSize = 30;
        const float defaultFontSize = 22;
        const int defaultPadding = 10;
        //const float cornerRadius = 20f;


        var collection = new FontCollection();

        using var regularStream = LoadFont("SFUIText-Regular.ttf");
        using var boldStream = LoadFont("SFUIText-Bold.ttf");

        var regularFontFamily = collection.Add(regularStream);
        var boldFontFamily = collection.Add(boldStream);


        using var background = Image.Load(backgroundPath);
        using var mapImage = Image.Load(stamp.GPSMapImagePath);
        using var mapMarker = Image.Load("Assets/MapMarker.png");


        // Resize the map marker maintaining aspect ratio give only width and height should be according to the aspect ratio of itself
        mapMarker.Mutate(ctx => ctx.Resize(new ResizeOptions
        {
            Size = new Size(40, 40),
            Mode = ResizeMode.Max
        }));

        // Create a semi-transparent black overlay at the bottom of the image
        const int overlayHeight = 180;
        const int overlayWidth = 1080;
        using var overlay = new Image<Rgba32>(overlayWidth, overlayHeight, new Rgba32(0, 0, 0, 128));
        //960, 160

        mapImage.Mutate(ctx =>
        {
            ctx.Resize(new ResizeOptions
            {
                Size = new Size(overlayHeight - (defaultPadding * 2), overlayHeight - (defaultPadding * 2)),
                Mode = ResizeMode.Crop
            });

            // Draw the map marker on the exact midde of the map image and so that the mapmarker middle bottom is on the center of the map image
            var markerPosition = new Point((mapImage.Width / 2) - (mapMarker.Width / 2), (mapImage.Height / 2) - mapMarker.Height + 10);
            ctx.DrawImage(mapMarker, markerPosition, 1f);
        });

        var boxSize = overlay.Size;
        var firstColumn = mapImage.Width + 30;


        // Add the text on the right side of the overlay (adjust position and text)
        // Make the font bold
        var titleFont = boldFontFamily.CreateFont(titleFontSize, FontStyle.Bold);
        var captionFont = regularFontFamily.CreateFont(defaultFontSize);
        var valueFont = boldFontFamily.CreateFont(defaultFontSize, FontStyle.Bold);
        //var titleFont = SystemFonts.CreateFont("Sf UI Text", titleFontSize, FontStyle.Bold);
        //var captionFont = SystemFonts.CreateFont("Sf UI Text", defaultFontSize);
        //var valueFont = SystemFonts.CreateFont("Sf UI Text", defaultFontSize, FontStyle.Bold);
        var titleTextRectangle = TextMeasurer.MeasureSize(stamp.Location.DisplayName, new TextOptions(titleFont));
        //var textPosition = new PointF(firstColumn, overlayHeight - defaultPadding);

        // If the text is too long, adjust the font size up to size 25 and then turnate it to fit
        if (titleTextRectangle.Width > overlay.Width - firstColumn)
        {
            var adjustedFontSize = 50;
            while (titleTextRectangle.Width > overlay.Width - firstColumn && adjustedFontSize > 25)
            {
                adjustedFontSize -= 5;
                titleFont = SystemFonts.CreateFont("Arial", adjustedFontSize, FontStyle.Bold);
                titleTextRectangle = TextMeasurer.MeasureSize(stamp.Location.DisplayName, new TextOptions(titleFont));
            }
            // Truncate the text if it still doesn't fit
            if (titleTextRectangle.Width > overlay.Width - firstColumn)
            {
                stamp.Location.DisplayName = $"{stamp.Location.DisplayName.AsSpan(0, stamp.Location.DisplayName.Length / 2)}...";
            }
        }

        var lineRectangle = TextMeasurer.MeasureSize(stamp.Location.Latitude.ToString(), new TextOptions(valueFont));

        var firstLineTop = (int)titleTextRectangle.Height + 10;
        var secondLineTop = firstLineTop + (int)lineRectangle.Height + spaceBetweenLines;


        using var textOverlay = new Image<Rgba32>(boxSize.Width, secondLineTop + (int)lineRectangle.Bottom + 10, Color.Transparent);
        var secondColumn = (int)(textOverlay.Width / 2) - 50;

        textOverlay.Mutate(ctx =>
        {
            // Draw the first text (Location.DisplayName)
            ctx.DrawText(stamp.Location.DisplayName, titleFont, Color.White, new Point(0, 0));



            ctx.DrawText("Latitude:", captionFont, Color.White, new Point(0, firstLineTop));
            var latx = (int)(TextMeasurer.MeasureSize("Latitude:", new TextOptions(captionFont)).Width + spaceBetweenCaptionAndValue);
            ctx.DrawText(stamp.Location.Latitude.ToString().Substring(0, 8), valueFont, Color.White, new Point(latx, firstLineTop));

            ctx.DrawText("Longitude:", captionFont, Color.White, new Point(0, secondLineTop));
            var longx = (int)(TextMeasurer.MeasureSize("Longitude:", new TextOptions(captionFont)).Width + spaceBetweenCaptionAndValue);
            ctx.DrawText(stamp.Location.Longitude.ToString().Substring(0, 8), valueFont, Color.White, new Point(longx, secondLineTop));


            // Draw the second text (DateTime)
            ctx.DrawText("Date:", captionFont, Color.White, new Point(secondColumn, firstLineTop));
            var dateTextX = (int)(secondColumn + TextMeasurer.MeasureSize("Date:", new TextOptions(captionFont)).Width + spaceBetweenCaptionAndValue);
            ctx.DrawText(stamp.DateTime.Date.ToString("dd-MM-yyyy"), valueFont, Color.White, new Point(dateTextX, firstLineTop));


            // Draw the second text (DateTime)
            ctx.DrawText("Time:", captionFont, Color.White, new Point(secondColumn, secondLineTop));
            var timeTextX = (int)(secondColumn + TextMeasurer.MeasureSize("Time:", new TextOptions(captionFont)).Width + spaceBetweenCaptionAndValue);
            ctx.DrawText(stamp.DateTime.ToShortTimeString(), valueFont, Color.White, new Point(timeTextX, secondLineTop));



        });


        overlay.Mutate(ctx =>
        {
            // Draw the map image on the overlay
            ctx.DrawImage(mapImage, new Point(defaultPadding, defaultPadding), 1f);
            ctx.DrawImage(textOverlay, new Point(firstColumn, (overlay.Height / 2) - (textOverlay.Height / 2)), 1f);
            //ctx.DrawImage(textOverlay, new Point(firstColumn, 0), 1f);


        });


        // Draw the resized overlay onto the background at the bottom
        background.Mutate(ctx =>
        {
            // The background image needs to be resized to 1920x720 pixel to maintain aspect raio of 16:19
            // First determine if the provided image is portrait of landscape, then resize accordingly
            if (background.Width > background.Height)
            {
                // Landscape
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(1920, 1080),
                    Mode = ResizeMode.Crop
                });
            }
            else
            {
                // Portrait
                ctx.Resize(new ResizeOptions
                {
                    Size = new Size(1080, 1920),
                    Mode = ResizeMode.Crop
                });
            }


            //var newOverlayWidth = background.Width;
            //var aspectRatio = (float)overlay.Height / overlay.Width;
            //var newOverlayHeight = (int)(newOverlayWidth * aspectRatio);

            //// Resize the overlay
            //using var resizedOverlay = overlay.Clone(ctx => ctx.Resize(newOverlayWidth, newOverlayHeight));

            // If the image is landscape, then the overlay needs to be placed at the middle bottom of the image
            if (background.Width > background.Height)
            {
                // Landscape
                ctx.DrawImage(overlay, new Point((background.Width / 2) - (overlay.Width / 2), background.Height - overlay.Height), 1f);
            }
            else
            {
                ctx.DrawImage(overlay, new Point(0, background.Height - overlay.Height), 1f);
            }
        });

        ApplyExifMetadata(background, stamp);

        background.Save(outputFileName);

        if (File.Exists(stamp.GPSMapImagePath))
        {
            File.Delete(stamp.GPSMapImagePath);
        }


        return outputFileName;
    }

    public static void ApplyExifMetadata(Image image, Stamp stamp)
    {
        ArgumentNullException.ThrowIfNull(image);

        var exif = image.Metadata.ExifProfile ?? new ExifProfile();
        image.Metadata.ExifProfile = exif;

        // Format date as "YYYY:MM:DD HH:MM:SS"
        var formattedDate = stamp.DateTime.ToString("yyyy:MM:dd HH:mm:ss", CultureInfo.InvariantCulture);

        // Set date taken
        exif.SetValue(ExifTag.DateTimeOriginal, formattedDate);
        exif.SetValue(ExifTag.DateTimeDigitized, formattedDate);
        exif.SetValue(ExifTag.DateTime, formattedDate); // overwrite Date Modified

        // Set GPS latitude
        exif.SetValue(ExifTag.GPSLatitudeRef, stamp.Location.Latitude >= 0 ? "N" : "S");
        exif.SetValue(ExifTag.GPSLatitude, ConvertToExifGpsCoordinate(stamp.Location.Latitude));

        // Set GPS longitude
        exif.SetValue(ExifTag.GPSLongitudeRef, stamp.Location.Longitude >= 0 ? "E" : "W");
        exif.SetValue(ExifTag.GPSLongitude, ConvertToExifGpsCoordinate(stamp.Location.Longitude));

        // Remove/overwrite unwanted tags
        exif.RemoveValue(ExifTag.Artist);        // Author
        exif.RemoveValue(ExifTag.HostComputer);  // Often used for system info
        exif.RemoveValue(ExifTag.OwnerName);  // Only in some formats — optional
    }

    private static Rational[] ConvertToExifGpsCoordinate(double decimalDegrees)
    {
        decimalDegrees = Math.Abs(decimalDegrees);
        var degrees = (int)decimalDegrees;
        var minutesDecimal = (decimalDegrees - degrees) * 60;
        var minutes = (int)minutesDecimal;
        var secondsDecimal = (minutesDecimal - minutes) * 60;
        var secondsScaled = (int)(secondsDecimal * 10000);

        return
        [
            new Rational((uint)degrees, 1),
            new Rational((uint)minutes, 1),
            new Rational((uint)secondsScaled, 10000)
        ];
    }


    private static readonly Assembly _asm = typeof(ImageManipulationService).Assembly;
    private static readonly string _ns = _asm.GetName().Name!;
    private static Stream LoadFont(string fileName)
    {
        var resourcePath = $"{_ns}.Assets.{fileName}";

        return _asm.GetManifestResourceStream(resourcePath)
            ?? throw new FileNotFoundException($"Embedded resource not found: {resourcePath}");
    }
}
