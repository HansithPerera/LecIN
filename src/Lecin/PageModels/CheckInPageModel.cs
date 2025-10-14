using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices.Sensors;

namespace Lecin.PageModels;

public partial class CheckInPageModel : BasePageModel
{
    private CameraView? _cameraView;

    [ObservableProperty] private ImageSource? _photoPreview;
    [ObservableProperty] private bool _hasCapturedPhoto;
    [ObservableProperty] private DateTime _capturedAt = DateTime.Now;
    [ObservableProperty] private Location? _location;
    [ObservableProperty] private string _locationDisplay = "";

    public IAsyncRelayCommand CapturePhotoCommand { get; }
    public IAsyncRelayCommand ConfirmCommand { get; }

    public bool CanCapture => _cameraView != null && !_hasCapturedPhoto;
    public bool CanConfirm => _hasCapturedPhoto && _location != null;

    public CheckInPageModel()
    {
        CapturePhotoCommand = new AsyncRelayCommand(CapturePhoto, () => CanCapture);
        ConfirmCommand = new AsyncRelayCommand(Confirm, () => CanConfirm);
    }

    public void InitializeCameraView(CameraView cameraView)
    {
        _cameraView = cameraView;
        _cameraView.MediaCaptured -= OnMediaCaptured;
        _cameraView.MediaCaptured += OnMediaCaptured;
        _ = PrepareAsync();
        NotifyCanExec();
    }

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
        finally
        {
            NotifyCanExec();
        }
    }

    private Task CapturePhoto()
    {
        CapturedAt = DateTime.Now;
        _cameraView?.CaptureImage();
        return Task.CompletedTask;
    }

    private void OnMediaCaptured(object? sender, MediaCapturedEventArgs e)
    {
        PhotoPreview = ImageSource.FromStream(() => new MemoryStream(e.ImageData));
        HasCapturedPhoto = true;
        NotifyCanExec();
    }

    private async Task Confirm()
    {
        // Placeholder: Here you would send photo bytes + time + location to backend
        await Application.Current!.MainPage!.DisplayAlert("Checked In",
            $"Time: {CapturedAt:HH:mm:ss}\nLocation: {LocationDisplay}", "OK");

        await Shell.Current.GoToAsync("..");
    }

    private void NotifyCanExec()
    {
        CapturePhotoCommand.NotifyCanExecuteChanged();
        ConfirmCommand.NotifyCanExecuteChanged();
    }
}


