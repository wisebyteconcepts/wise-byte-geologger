# Wise Byte GeoLogger

A modern Windows desktop application for adding geolocation metadata (GPS coordinates and satellite imagery) to your photos and images. Built with **WinUI 3** and **.NET**, GeoLogger provides an intuitive interface to stamp images with location data, timestamps, and satellite map overlays.

## 🎯 Features

- **Batch Image Processing**: Process multiple images simultaneously with geolocation stamping
- **GPS Coordinate Mapping**: Input latitude and longitude to retrieve accurate location data
- **Satellite Imagery Integration**: Automatically fetch and overlay satellite map images at specified coordinates
- **Location Reverse Geocoding**: Get address information from GPS coordinates using LocationIQ API
- **Timestamp Integration**: Combine location data with date and time information for comprehensive metadata
- **Image Manipulation**: Apply location stamps and satellite imagery directly to image files
- **Dark/Light Theme Support**: Native theme switching with Mica background effects
- **Local Settings Management**: Persist application preferences and API credentials securely
- **Responsive Dashboard**: Central hub for managing all application features

## 🛠️ Technology Stack

- **Framework**: .NET (Windows App SDK)
- **UI Framework**: WinUI 3
- **Architecture Pattern**: MVVM (Model-View-ViewModel)
- **Dependency Injection**: .NET Generic Host with Microsoft.Extensions.DependencyInjection
- **Reactive Patterns**: MVVM Toolkit (CommunityToolkit.Mvvm)
- **Geolocation Service**: LocationIQ API
- **Image Processing**: Custom ImageManipulationService
- **Data Persistence**: JSON-based local settings storage

## 📋 System Requirements

- **OS**: Windows 10 22H2 or Windows 11
- **.NET Runtime**: .NET 8.0 or later
- **Windows App SDK**: Latest version
- **RAM**: Minimum 4GB
- **Disk Space**: Minimum 500MB for application installation

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (or later) with Windows App SDK workload
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- [Git](https://git-scm.com/)

### Installation

1. **Clone the Repository**

2. **Open the Solution**
   - Open the solution file in Visual Studio 2022
   - Allow Visual Studio to restore NuGet packages automatically

3. **Restore Dependencies**

4. **Build the Solution**

5. **Run the Application**
   - Press `F5` in Visual Studio or select **__Run__** from the menu
   - The application will launch with the Dashboard view

## 📖 Usage Guide

### Main Workflow

#### 1. **Configure API Key** (First Time Setup)
- Navigate to the **Settings** page
- Enter your LocationIQ API key to enable geolocation features
- Save the settings

> **Note**: Obtain a free LocationIQ API key from [locationiq.com](https://locationiq.com)

#### 2. **Navigate to Image Stamper**
- From the Dashboard, select the **Stamper** option
- You will be redirected to the Image Stamper page

#### 3. **Input Location Coordinates**
- Enter **Latitude** (e.g., 26.187394)
- Enter **Longitude** (e.g., 91.563845)
- Click **Get Location** to retrieve address information and fetch satellite imagery
- A satellite map will display at the specified coordinates

#### 4. **Select Images for Processing**
- Click **Select Images** button
- Choose one or multiple image files (.png, .jpg, .jpeg, .bmp, .gif)
- Selected images will appear in the processing queue
- You can review and remove individual images as needed

#### 5. **Configure Timestamp Information**
- Set the **Date** using the date picker
- Set the **Time** using the time picker
- This information will be stamped onto your images

#### 6. **Process Images**
- Click **Batch Process** to begin processing
- Select an output directory where stamped images will be saved
- The application will:
  - Fetch satellite imagery for the specified coordinates
  - Apply location stamps and metadata to each image
  - Save processed images with unique filenames (format: `GPSCamera_yyyyMMdd_hhmmtt`)

#### 7. **Verify Results**
- Navigate to your output directory
- Verify the stamped images with embedded geolocation data and satellite overlays

### Advanced Features

#### Batch Processing Multiple Locations
- Process different sets of images with various coordinates by repeating the workflow
- Each location gets its own satellite imagery and metadata

#### Output File Naming
- Processed files follow the pattern: `GPSCamera_[Date]_[Time]_([Counter]).extension`
- Automatic counter prevents overwriting existing files

## 📁 Project Structure

## 🔑 Key Components

### Services

#### **ILocationService** (LocationIqLocationService)
- Retrieves location information from coordinates
- Fetches satellite imagery using LocationIQ API
- Supports address reverse geocoding

#### **IImageManipulationService**
- Applies location stamps to images
- Embeds satellite imagery as overlays
- Handles image file I/O operations

#### **ILocalSettingsService**
- Persists application settings (API keys, preferences)
- Supports both MSIX packaged and standard installations
- JSON-based storage format

#### **INavigationService**
- Manages page navigation and view switching
- Integrates with MVVM framework for seamless transitions

### View Models

#### **DashboardViewModel**
- Central hub for application navigation
- Displays overview and quick-access features

#### **StamperViewModel**
- Manages image selection and batch processing
- Handles coordinate input and location retrieval
- Controls output file generation

#### **SettingsViewModel**
- Manages application preferences
- Stores and retrieves API credentials
- Theme and display settings

## ⚙️ Configuration

### LocalSettingsOptions
Configure application data folder and settings file location in `appsettings.json`:

### API Configuration
Store LocationIQ API key in application settings through the Settings page:
- Key: `APIKey`
- Value: Your LocationIQ API key

## 🎨 UI Architecture

The application follows a **three-tier architecture**:

1. **Presentation Layer** (WinUI 3 Views)
   - XAML pages and controls
   - Responsive dialog and message boxes

2. **ViewModel Layer** (MVVM Toolkit)
   - `ObservableObject` and `ObservableRecipient` patterns
   - `RelayCommand` for command binding
   - Property binding and state management

3. **Business Logic Layer** (Core Services)
   - Image manipulation
   - Geolocation services
   - File I/O operations

## 🔒 Security Considerations

- **API Keys**: Stored securely in local application data
- **File Paths**: Validated to prevent directory traversal attacks
- **Image Files**: Only supported formats are processed (.png, .jpg, .jpeg, .bmp, .gif)

## 🐛 Troubleshooting

### Issue: "No API Key found from settings"
**Solution**: Navigate to the Settings page and configure your LocationIQ API key.

### Issue: "Static Map image cannot be found"
**Solution**: Ensure your internet connection is active and the LocationIQ API quota hasn't been exceeded.

### Issue: Images not saving to output directory
**Solution**: Verify that you have write permissions to the selected output directory and sufficient disk space.

### Issue: Application fails to launch
**Solution**: 
- Ensure all NuGet packages are restored: `dotnet restore`
- Verify .NET 8 SDK is installed: `dotnet --version`
- Rebuild the solution: `dotnet build --force`

## 📚 Dependencies

Key NuGet packages used in this project:

- **CommunityToolkit.Mvvm**: MVVM pattern implementation
- **Microsoft.Extensions.***: Dependency injection, configuration, logging
- **Windows App SDK**: WinUI 3 and modern Windows API access
- **WinUIEx**: Extended WinUI 3 functionality

## 🤝 Contributing

We welcome contributions to the GeoLogger project! Please refer to the [CONTRIBUTING.md](CONTRIBUTING.md) file for guidelines on:
- Code style and formatting conventions
- Submitting pull requests
- Reporting issues
- Development workflow

## 📄 License

This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.

## 📞 Support

For issues, feature requests, or questions:

1. **GitHub Issues**: [https://github.com/wisebyteconcepts/wise-byte-geologger/issues](https://github.com/wisebyteconcepts/wise-byte-geologger/issues)
2. **Documentation**: Check the project wiki for additional resources
3. **API Documentation**: [LocationIQ Documentation](https://locationiq.com/docs)

## 🙏 Acknowledgments

- [LocationIQ](https://locationiq.com) for geolocation and satellite imagery services
- [WinUI Community](https://github.com/microsoft/WinUI-Gallery) for UI patterns and best practices
- [MVVM Toolkit](https://github.com/CommunityToolkit/dotnet) for reactive programming support

## 📝 Version History

- **v1.0.0** - Initial release with core functionality
  - Batch image processing with geolocation metadata
  - Satellite imagery integration
  - Settings management and API key configuration

---

**Last Updated**: November 30, 2025

For the latest updates and releases, visit the [GitHub repository](https://github.com/wisebyteconcepts/wise-byte-geologger).