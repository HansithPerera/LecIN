using CommunityToolkit.Maui.Core;
using Lecin.PageModels;

namespace Lecin.Pages;

 public partial class CheckInPage : ContentPage
{
    public CheckInPage(CheckInPageModel viewModel)
    {
        try
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"CheckInPage initialization error: {ex}");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Error", "Failed to initialize check-in page. Please try again.", "OK");
            });
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is CheckInPageModel viewModel)
        {
            viewModel.OnPageAppearing();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is CheckInPageModel viewModel)
        {
            viewModel.ClearPhoto();
        }
    }

    private async void CapturePhotoButton_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is PageModels.CheckInPageModel vm) await vm.OnCaptureRequested();
    }

    private async void ConfirmButton_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is PageModels.CheckInPageModel vm) await vm.Confirm();
    }

    private async void OnCameraStatusTapped(object? sender, EventArgs e)
    {
        if (BindingContext is PageModels.CheckInPageModel vm) 
        {
            await vm.RetryCameraPermission();
        }
    }

    private async void RetakePhotoButton_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is PageModels.CheckInPageModel vm) 
        {
            vm.ClearPhoto();
            await vm.OnCaptureRequested();
        }
    }
}


