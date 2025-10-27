using CommunityToolkit.Mvvm.ComponentModel;

namespace Lecin.PageModels;

public partial class CheckInPageModel : ObservableObject
{
    [ObservableProperty]
    private string currentTime = "00:00:00";

    [ObservableProperty]
    private bool hasPhoto = false;

    public CheckInPageModel()
    {
    }
}
