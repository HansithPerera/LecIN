using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lecin.PageModels.Teacher;

namespace Lecin.Pages.Teacher;

public partial class TeacherCourseViewPage
{
    public TeacherCourseViewPage(TeacherCourseViewPageModel vm, Supabase.Client client): base(vm, client)
    {
        InitializeComponent();
    }

    private async void GotoClassView(object? sender, EventArgs e)
    {
        try
        {
            if (sender is not Button { BindingContext: SupabaseShared.Models.Class cls }) return;

            await Shell.Current.GoToAsync($"teachers/class", new Dictionary<string, object>
            {
                { "class", cls }
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to navigate to class view: {ex.Message}", "OK");
        }
    }
}