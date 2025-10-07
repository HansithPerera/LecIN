using Supabase;

namespace Lecin.Pages;

public abstract class BaseContentPage : ContentPage
{
    private readonly Client _client;

    protected BaseContentPage(BasePageModel vm, Client client)
    {
        _client = client;
        BindingContext = vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (_client.Auth.CurrentUser == null)
            await Shell.Current.GoToAsync("login");
        else if (BindingContext is BasePageModel vm) await vm.LoadDataAsync();
    }
}