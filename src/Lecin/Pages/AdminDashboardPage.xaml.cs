using Lecin.PageModels.Admin;
using Supabase;
using SupabaseShared.Models;

namespace Lecin.Pages;

public partial class AdminDashboardPage : ContentPage
{
    private readonly Supabase.Client _supabase;
    
    public AdminDashboardPage(AdminPageModel vm, Supabase.Client supabase)
    {
        InitializeComponent();
        _supabase = supabase;
        BindingContext = vm;
    }
    
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (_supabase.Auth.CurrentUser == null)
        {
            await Shell.Current.GoToAsync("login");
        }
        else if (BindingContext is AdminPageModel vm)
        {
            await vm.LoadDataAsync();
        }
    }
    
    private void OnDownloadCsvClicked(object sender, EventArgs e)
    {
        if (sender is not Button { BindingContext: Course course }) return;
        
        if (BindingContext is AdminPageModel vm && vm.DownloadAttendanceCommand.CanExecute(course))
        {
            vm.DownloadAttendanceCommand.Execute(course);
        }
    }
}
