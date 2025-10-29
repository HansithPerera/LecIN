using CommunityToolkit.Mvvm.ComponentModel;
using Supabase;

namespace Lecin.PageModels.Admin;

public partial class AdminListLocationPageModel(Client client, ModalErrorHandler errorHandler) : BasePageModel
{
    [ObservableProperty] private bool _isLoading;

    public override async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            // Load location data logic here
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }
}