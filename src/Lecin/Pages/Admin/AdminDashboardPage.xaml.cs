using Lecin.PageModels.Admin;
using SupabaseShared.Models;

namespace Lecin.Pages.Admin;

public partial class AdminDashboardPage : BaseContentPage
{
    public AdminDashboardPage(AdminPageModel vm) : base(vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private void OnDownloadCsvClicked(object sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: Course course }) return;

        if (BindingContext is AdminPageModel vm && vm.DownloadAttendanceCommand.CanExecute(course))
            vm.DownloadAttendanceCommand.Execute(course);
    }

    private void GotoCameras(object? sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"//{nameof(AdminListCameraPage)}");
    }
    
    private void GotoLocations(object? sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"//{nameof(AdminListLocationPage)}");
    }
}