namespace Lecin.Pages;

public partial class LandingPage : ContentPage
{
    public LandingPage(LandingPageModel vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}