using Lecin.PageModels.Teacher;
using Supabase;

namespace Lecin.Pages.Teacher;

public partial class TeacherClassViewPage : BaseContentPage
{
    public TeacherClassViewPage(TeacherClassViewPageModel vm, Client client) : base(vm)
    {
        InitializeComponent();
    }

    private async void OnAddAttendanceClicked(object? sender, EventArgs e)
    {
        try
        {
            if (BindingContext is not TeacherClassViewPageModel vm) return;
            await vm.AddAttendance();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void MarkPresent(object? sender, EventArgs e)
    {
        try
        {
            if (BindingContext is not TeacherClassViewPageModel vm) return;
            if (sender is not Button { BindingContext: SupabaseShared.Models.Student student }) return;
            await vm.SetAttendance(student.Id, "Present");
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }

    private async void MarkLate(object? sender, EventArgs e)
    {
        try
        {
            if (BindingContext is not TeacherClassViewPageModel vm) return;
            if (sender is not Button { BindingContext: SupabaseShared.Models.Student student }) return;
            await vm.SetAttendance(student.Id, "Late");
        }
        catch (Exception exception)
        {
            await DisplayAlert("Error", exception.Message, "OK");
        }
    }
}