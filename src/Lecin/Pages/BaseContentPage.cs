namespace Lecin.Pages;

public abstract class BaseContentPage : ContentPage
{
    protected BaseContentPage(BasePageModel vm)
    {
        BindingContext = vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        if (BindingContext is BasePageModel vm) await vm.LoadDataAsync();
    }
}