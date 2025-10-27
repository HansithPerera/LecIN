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

    public CheckInPageModel()
    {
    }

    public void OnPageAppearing() => _ = PrepareAsync();

    private async Task PrepareAsync()
    {
        await EnsurePermissions();
        CapturedAt = DateTime.Now;
        await CaptureLocation();
    }

    private async Task EnsurePermissions()
    {
        try
        {
            // Check camera permission
            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (cameraStatus == PermissionStatus.Denied)
            {
                IsCameraPermissionGranted = false;
                CameraStatus = "Camera permission denied - enable in settings";
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
            if (locationStatus != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        }
        catch (Exception ex)
        {
            CameraStatus = $"Permission error: {ex.Message}";
            IsCameraPermissionGranted = false;
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
        if (!IsCameraPermissionGranted)
        {
            CameraStatus = "Camera permission required - tap to retry";
            return;
        }

        try
        {
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
        }
        catch (FeatureNotSupportedException)
        {
            CameraStatus = "Camera not supported on this device";
        }
        catch (Exception ex)
        {
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


