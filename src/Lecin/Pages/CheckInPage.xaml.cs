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

    private void CapturePhotoButton_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is PageModels.CheckInPageModel vm) vm.OnCaptureRequested();
    }

    private async void ConfirmButton_Clicked(object? sender, EventArgs e)
    {
        if (BindingContext is PageModels.CheckInPageModel vm) await vm.Confirm();
    }
}


