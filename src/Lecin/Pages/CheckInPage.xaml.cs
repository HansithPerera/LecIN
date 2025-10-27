using CommunityToolkit.Maui.Core;

namespace Lecin.Pages;

 public partial class CheckInPage : ContentPage
{
    public CheckInPage()
    {
        InitializeComponent();
        var vm = new PageModels.CheckInPageModel();
        BindingContext = vm;
        vm.OnPageAppearing();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
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


