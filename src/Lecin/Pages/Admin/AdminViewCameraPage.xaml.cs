using Lecin.PageModels.Admin;

namespace Lecin.Pages.Admin;

public partial class AdminViewCameraPage : BaseContentPage
{
    public AdminViewCameraPage(AdminViewCameraPageModel vm) : base(vm)
    {
        BindingContext = vm;
        vm.OnApiKeyGenerated += (s, e) =>
        {
            DisplayAlert("API Key Generated", $"Key copied to clipboard:\n\n{e.Key}", "OK");
            Clipboard.Default.SetTextAsync(e.Key);
        };
        InitializeComponent();
    }

    private void OnViewLocationDetailsClicked(object? sender, EventArgs e)
    {
        if (BindingContext is not AdminViewCameraPageModel vm || vm.Camera == null)
            return;
        
        Shell.Current.GoToAsync(nameof(AdminViewLocationPage), true, new Dictionary<string, object>
        {
            { "locationId", vm.Camera.Location }
        });
    }
}