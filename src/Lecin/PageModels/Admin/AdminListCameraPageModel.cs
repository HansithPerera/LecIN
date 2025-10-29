using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase;
using SupabaseShared.Models;
using static Supabase.Functions.Client;
using Location = SupabaseShared.Models.Location;

namespace Lecin.PageModels.Admin;

public partial class AdminListCameraPageModel(Client client, ModalErrorHandler errorHandler) : BasePageModel
{
    [ObservableProperty] private bool _isLoading;
    
    [ObservableProperty] private ObservableCollection<Camera> _cameras = [];
    
    [ObservableProperty] private ObservableCollection<Location> _locations = [];

    [ObservableProperty] private string _newCameraName = string.Empty;
    
    [ObservableProperty] private Location? _newCameraLocation; 
    
    [RelayCommand]
    public override async Task LoadDataAsync()
    {
        IsLoading = true;
        try
        {
            var cameras = await client.From<Camera>()
                .Select("*")
                .Get();
            Cameras = new ObservableCollection<Camera>(cameras.Models);
            var locations = await client.From<Location>()
                .Select("*")
                .Get();
            Locations = new ObservableCollection<Location>(locations.Models);
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
    public async Task CreateCameraAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewCameraName))
            {
                errorHandler.HandleError(new Exception("Camera name cannot be empty."));
                return;
            }
            if (NewCameraLocation == null)
            {
                errorHandler.HandleError(new Exception("Please select a location for the camera."));
                return;
            }

            var options = new InvokeFunctionOptions
            {
                Body = new Dictionary<string, object>
                {
                    { "Name", NewCameraName },
                    { "Location", NewCameraLocation.Id }
                }
            };
            var createdCamera = await client.Functions.Invoke<Camera>("create-camera", options: options);
            if (createdCamera != null)
            {
                Cameras.Add(createdCamera);
                NewCameraName = string.Empty;
                NewCameraLocation = null;
            }
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(ex);
        }
    }
    
    public async Task DeleteCameraAsync(Camera camera)
    {
        try
        {
            await client.Functions.Invoke("delete-camera",
                options: new InvokeFunctionOptions
                {
                    Body = new Dictionary<string, object>
                    {
                        { "CameraId", camera.Id.ToString() }
                    }
                });
            Cameras.Remove(camera);
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(ex);
        }
    }
}