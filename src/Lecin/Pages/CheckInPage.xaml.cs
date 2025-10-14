using CommunityToolkit.Maui.Views;

namespace Lecin.Pages;

public partial class CheckInPage : ContentPage
{
    public CheckInPage(PageModels.CheckInPageModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.InitializeCameraView(Camera);
    }
}


