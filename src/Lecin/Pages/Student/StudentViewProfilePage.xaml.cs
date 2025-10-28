using Lecin.PageModels.Student;

namespace Lecin.Pages.Student;

public partial class StudentViewProfilePage : BaseContentPage
{
    public StudentViewProfilePage(StudentViewProfilePageModel vm) : base(vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }


    private void GotoRegisterFaceView(object? sender, EventArgs e)
    {
        if (BindingContext is not StudentViewProfilePageModel vm) return;
        try
        {
            Shell.Current.GoToAsync(nameof(StudentRegisterFacePage));
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to navigate to face registration: {ex.Message}", "OK");
        }
    }
}