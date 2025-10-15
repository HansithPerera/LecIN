using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Devices.Sensors;

namespace Lecin.PageModels;

public partial class CheckInPageModel : BasePageModel
{
    [ObservableProperty] private ImageSource? _photoPreview;
    [ObservableProperty] private bool _hasCapturedPhoto;
    [ObservableProperty] private DateTime _capturedAt = DateTime.Now;
    [ObservableProperty] private Location? _location;
    [ObservableProperty] private string _locationDisplay = "";

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
        await Permissions.RequestAsync<Permissions.Camera>();
        await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
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

    public void OnCaptureRequested()
    {
        CapturedAt = DateTime.Now;
    }

    public void OnMediaCaptured(byte[] imageData)
    {
        PhotoPreview = ImageSource.FromStream(() => new MemoryStream(imageData));
        HasCapturedPhoto = true;
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


