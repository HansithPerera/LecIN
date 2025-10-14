using System.Diagnostics;

namespace Lecin.Pages;

public partial class LandingPage : BaseContentPage
{
    public LandingPage(LandingPageModel vm) : base(vm)
    {
        BindingContext = vm;
        vm.OnSessionRestoreFailed += OnSessionRestoreFailed;
        InitializeComponent();
    }

    private async void OnSessionRestoreFailed(object? sender, EventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Failed to navigate to login page: " + ex.Message);
        }
    }
}