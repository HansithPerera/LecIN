using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Media;

namespace Lecin.PageModels;

public partial class CheckInPageModel : BasePageModel
{
    [ObservableProperty] private ImageSource? _photoPreview;
    [ObservableProperty] private bool _hasCapturedPhoto;
    [ObservableProperty] private DateTime _capturedAt = DateTime.Now;
    [ObservableProperty] private Location? _location;
    [ObservableProperty] private string _locationDisplay = "";
    [ObservableProperty] private bool _isCameraPermissionGranted;
    [ObservableProperty] private string _cameraStatus = "Initializing...";

    private bool _isInitialized;

    public CheckInPageModel()
    {
        // Constructor intentionally empty
    }

    public async void OnPageAppearing()
    {
        if (!_isInitialized)
        {
            _isInitialized = true;
            await PrepareAsync();
        }
    }

    private async Task PrepareAsync()
    {
        try
        {
            CameraStatus = "Initializing...";
            IsCameraPermissionGranted = false;
            HasCapturedPhoto = false;
            PhotoPreview = null;
            
            await EnsurePermissions();
            CapturedAt = DateTime.Now;
            await CaptureLocation();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PrepareAsync error: {ex}");
            CameraStatus = "Initialization failed - tap to retry";
            throw;
        }
    }

    private async Task EnsurePermissions()
    {
        try
        {
            var window = Application.Current?.Windows[0];
            if (window?.Page == null) return;

            // Check camera permission
            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus == PermissionStatus.Denied)
            {
                IsCameraPermissionGranted = false;
                CameraStatus = "Camera permission denied - tap to enable in settings";
                await window.Page.DisplayAlert(
                    "Camera Permission Required",
                    "Please enable camera access in your device settings to use the check-in feature.",
                    "OK");
                return;
            }
            
            if (cameraStatus != PermissionStatus.Granted)
            {
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            }
            
            IsCameraPermissionGranted = cameraStatus == PermissionStatus.Granted;
            CameraStatus = IsCameraPermissionGranted ? "Camera ready" : "Camera permission required";
            
            // Check location permission
            var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus == PermissionStatus.Denied)
            {
                LocationDisplay = "Location access denied - tap to enable in settings";
                await window.Page.DisplayAlert(
                    "Location Permission Required",
                    "Please enable location access in your device settings to use the check-in feature.",
                    "OK");
                return;
            }

            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (locationStatus == PermissionStatus.Granted)
            {
                await CaptureLocation();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Permission error: {ex}");
            CameraStatus = $"Permission error: {ex.Message}";
            IsCameraPermissionGranted = false;
            
            var window = Application.Current?.Windows[0];
            if (window?.Page != null)
            {
                await window.Page.DisplayAlert(
                    "Error", 
                    "Failed to initialize camera and location services. Please try again.", 
                    "OK");
            }
        }
    }

    private async Task CaptureLocation()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            Location = await Geolocation.GetLocationAsync(request);
            LocationDisplay = Location != null
                ? $"{Location.Latitude:F5}, {Location.Longitude:F5}"
                : "Unavailable";
        }
        catch
        {
            LocationDisplay = "Unavailable";
        }
        finally { }
    }

    public async Task OnCaptureRequested()
    {
        try
        {
            if (!IsCameraPermissionGranted)
            {
                CameraStatus = "Camera permission required - tap to retry";
                await EnsurePermissions();
                return;
            }

            CameraStatus = "Capturing photo...";
            
            // Check if MediaPicker is available
            if (MediaPicker.Default == null)
            {
                CameraStatus = "Camera not available on this device";
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            
            if (photo != null)
            {
                using var stream = await photo.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                var imageData = memoryStream.ToArray();
                
                OnMediaCaptured(imageData);
                CapturedAt = DateTime.Now;
                CameraStatus = "Photo captured successfully";
            }
            else
            {
                CameraStatus = "Photo capture cancelled";
            }
        }
        catch (UnauthorizedAccessException)
        {
            CameraStatus = "Camera permission denied - tap to retry";
            IsCameraPermissionGranted = false;
            await EnsurePermissions();
        }
        catch (FeatureNotSupportedException)
        {
            CameraStatus = "Camera not supported on this device";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Camera error: {ex}");
            CameraStatus = $"Capture error: {ex.Message}";
        }
    }

    public async Task RetryCameraPermission()
    {
        await EnsurePermissions();
    }

    public void OnMediaCaptured(byte[] imageData)
    {
        PhotoPreview = ImageSource.FromStream(() => new MemoryStream(imageData));
        HasCapturedPhoto = true;
    }

    public void ClearPhoto()
    {
        PhotoPreview = null;
        HasCapturedPhoto = false;
        CameraStatus = "Camera ready";
    }

    public async Task Confirm()
    {
        // Placeholder: Here you would send photo bytes + time + location to backend
        if (Shell.Current?.CurrentPage != null)
        {
            await Shell.Current.CurrentPage.DisplayAlert("Checked In",
                $"Time: {CapturedAt:HH:mm:ss}\nLocation: {LocationDisplay}", "OK");

            await Shell.Current.GoToAsync("..");
        }
    }
}


