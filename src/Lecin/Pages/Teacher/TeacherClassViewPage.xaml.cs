using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lecin.PageModels.Teacher;

namespace Lecin.Pages.Teacher;

public partial class TeacherClassViewPage : BaseContentPage
{
    public TeacherClassViewPage(TeacherClassViewPageModel vm, Supabase.Client client) : base(vm, client)
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
}