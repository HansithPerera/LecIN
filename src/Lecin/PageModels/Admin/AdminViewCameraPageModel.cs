using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase;
using SupabaseShared.Models;

namespace Lecin.PageModels.Admin;

[QueryProperty(nameof(CameraId), "cameraId")]
public partial class AdminViewCameraPageModel(Client client, ModalErrorHandler errorHandler) : BasePageModel
{
    [ObservableProperty] private Camera? _camera;

    [ObservableProperty] private Guid _cameraId;

    [ObservableProperty] private bool _isLoading;
    
    [ObservableProperty] private ApiKey? _primaryApiKey;
    
    [ObservableProperty] private ApiKey? _secondaryApiKey;
    
    public event EventHandler<NewApiKeyResponse> OnApiKeyGenerated;

    public override async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            if (CameraId == Guid.Empty)
                throw new ArgumentException("CameraId is not set.");

            Camera = await client.From<Camera>()
                .Select("*")
                .Where(c => c.Id == CameraId)
                .Single();
            
            PrimaryApiKey = Camera?.ApiKeys.Where(cak => cak.Role == ApiKeyRole.Primary)
                .Select(cak => cak.ApiKey)
                .FirstOrDefault();
            
            SecondaryApiKey = Camera?.ApiKeys.Where(cak => cak.Role == ApiKeyRole.Secondary)
                .Select(cak => cak.ApiKey)
                .FirstOrDefault();
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
    
    [RelayCommand]
    private async Task GeneratePrimaryKeyAsync()
    {
        await GenerateKey(ApiKeyRole.Primary);
    }
    
    [RelayCommand]
    private async Task GenerateSecondaryKeyAsync()
    {
        await GenerateKey(ApiKeyRole.Secondary);
    }
    
    public async Task GenerateKey(ApiKeyRole role)
    {
        if (Camera == null)
            throw new InvalidOperationException("Camera is not loaded.");

        IsLoading = true;
        try
        {
            var key = await client.Functions.Invoke<NewApiKeyResponse>("create-camera-api-key",
                options: new Supabase.Functions.Client.InvokeFunctionOptions()
                {
                    Body = new Dictionary<string, object>()
                    {
                        { "CameraId", Camera.Id },
                        { "Primary", role == ApiKeyRole.Primary }
                    }
                });
            if (key == null)
                throw new Exception("Failed to generate API key.");
            await LoadDataAsync();
            OnApiKeyGenerated?.Invoke(this, key);
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