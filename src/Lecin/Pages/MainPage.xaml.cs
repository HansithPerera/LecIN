using Supabase;

namespace Lecin.Pages;

public partial class MainPage : ContentPage
{
    private readonly Client _supabase;
    
    public MainPage(MainPageModel vm, Client supabase)
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
            await Shell.Current.GoToAsync($"//{nameof(LoginPage)}");
        }
        else if (BindingContext is MainPageModel vm)
        {
            await vm.LoadDataAsync();
        }
    }
}