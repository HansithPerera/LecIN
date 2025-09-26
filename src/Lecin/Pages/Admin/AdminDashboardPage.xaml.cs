using Lecin.PageModels.Admin;
using Supabase;
using SupabaseShared.Models;

namespace Lecin.Pages.Admin;

public partial class AdminDashboardPage : BaseContentPage
{
    private readonly Client _supabase;

    public AdminDashboardPage(AdminPageModel vm, Client supabase) : base(vm, supabase)
    {
        InitializeComponent();
        _supabase = supabase;
        BindingContext = vm;
    }

    private void OnDownloadCsvClicked(object sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: Course course }) return;

        if (BindingContext is AdminPageModel vm && vm.DownloadAttendanceCommand.CanExecute(course))
            vm.DownloadAttendanceCommand.Execute(course);
    }
}