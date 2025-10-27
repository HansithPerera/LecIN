using Lecin.PageModels.Student;

namespace Lecin.Pages.Student;

[QueryProperty(nameof(CourseCode), "courseCode")]
public partial class ClassmatesPage : ContentPage
{
    private readonly ClassmatesPageModel _vm;
    private string _courseCode = "CS101";

    public string CourseCode
    {
        get => _courseCode;
        set
        {
            _courseCode = value;
            LoadData();
        }
    }

    public ClassmatesPage(ClassmatesPageModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    private async void LoadData()
    {
        await _vm.LoadClassmatesAsync(_courseCode);
    }
}
