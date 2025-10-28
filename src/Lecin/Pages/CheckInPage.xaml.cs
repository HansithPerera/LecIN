namespace Lecin.Pages;

public partial class CheckInPage : ContentPage
{
    private bool _hasPhoto = false;

    public CheckInPage()
    {
        InitializeComponent();
        UpdateTime();
    }

    private void UpdateTime()
    {
        TimeLabel.Text = $"Time: {DateTime.Now:hh:mm:ss tt}";

        Task.Run(async () =>
        {
            await Task.Delay(1000);
            MainThread.BeginInvokeOnMainThread(() => UpdateTime());
        });
    }

    private async void OnCaptureClicked(object sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();

            if (photo != null)
            {
                PhotoLabel.Text = "Photo Captured!";
                CaptureButton.Text = "Recapture";
                _hasPhoto = true;
                ConfirmButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Camera error: {ex.Message}", "OK");
        }
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Success", "Check-In Successful! Your attendance has been recorded.", "OK");
        await Shell.Current.GoToAsync("..");
    }
}
