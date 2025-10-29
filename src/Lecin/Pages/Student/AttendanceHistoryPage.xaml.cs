using Microsoft.Maui.Controls;

namespace Lecin.Pages.Student;

public partial class AttendanceHistoryPage : BaseContentPage
{
    public Lecin.PageModels.Student.AttendanceHistoryPageModel ViewModel { get; }

    public AttendanceHistoryPage(Lecin.PageModels.Student.AttendanceHistoryPageModel vm) : base(vm)
    {
        ViewModel = vm;
        InitializeComponent();
        AttendanceList.ItemsSource = ViewModel.Records;
    }
}

