using CommunityToolkit.Mvvm.ComponentModel;

namespace Lecin.PageModels;

public abstract class BasePageModel : ObservableObject
{
    public virtual Task LoadDataAsync()
    {
        return Task.CompletedTask;
    }
}