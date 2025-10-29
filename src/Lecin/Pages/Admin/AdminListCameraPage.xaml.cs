using Lecin.PageModels.Admin;
using SupabaseShared.Models;

namespace Lecin.Pages.Admin;

public partial class AdminListCameraPage : BaseContentPage
{
    public AdminListCameraPage(AdminListCameraPageModel pageModel) : base(pageModel)
    {
        BindingContext = pageModel;
        InitializeComponent();
    }

    private void GotoEditCameraView(object? sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: Camera camera })
            return;
        if (BindingContext is not AdminListCameraPageModel pageModel)
            return;
        Shell.Current.GoToAsync(nameof(AdminViewCameraPage), true, new Dictionary<string, object?>
        {
            { "cameraId", camera.Id }
        });
    }

    private async void DeleteCameraClicked(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not Button { BindingContext: Camera camera })
                return;
            if (BindingContext is not AdminListCameraPageModel pageModel)
                return;
            await pageModel.DeleteCameraAsync(camera);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}