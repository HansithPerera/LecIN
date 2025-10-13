using Supabase;

namespace Lecin.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageModel loginPageModel)
    {
        InitializeComponent();
        BindingContext = loginPageModel;
    }
    
    protected override async void OnAppearing()
    {
        if (BindingContext is LoginPageModel vm)
        {
            await vm.LoadDataAsync();
        }
    }
}