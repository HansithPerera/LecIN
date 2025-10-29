using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FaceONNX;
using FaceShared;
using SixLabors.ImageSharp.PixelFormats;
using Client = Supabase.Client;
using Exception = System.Exception;
using Image = SixLabors.ImageSharp.Image;

namespace Lecin.PageModels.Student;

public partial class StudentRegisterFacePageModel(Client client, ModalErrorHandler errorHandler) : BasePageModel
{
    private FaceDetector? _detector;
    private FaceEmbedder? _embedder;

    [ObservableProperty] private bool _isLoading = true;
    
    public event EventHandler<bool>? ImageProcessed;

    [ObservableProperty] private FileResult? _media;

    [ObservableProperty] private ImageSource? _selectedImage;

    [ObservableProperty] private SupabaseShared.Models.Student? _student;

    [RelayCommand]
    private async Task UploadPhotoAsync()
    {
        try
        {
            IsLoading = true;
            if (_detector == null || _embedder == null) return;

            Media = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select a photo"
            });

            if (Media == null) return;
            SelectedImage = ImageSource.FromFile(Media.FullPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to pick photo: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ProcessImage()
    {
        try
        {
            IsLoading = true;
            if (_detector == null || _embedder == null) return;

            if (Media == null)
            {
                errorHandler.HandleError(new Exception("No photo selected. Please select a photo first."));
                return;
            }

            var image = await Image.LoadAsync<Rgb24>(Media.FullPath);
            var embedding = await FaceUtil.DetectFaceAndGenerateEmbedding(image, _detector, _embedder);
            if (embedding == null)
            {
                errorHandler.HandleError(
                    new Exception("No face detected in the selected photo. Please select a different photo."));
                return;
            }

            await client.Functions.Invoke("register-student-face",
                options: new Supabase.Functions.Client.InvokeFunctionOptions
                {
                    Body = new Dictionary<string, object>
                    {
                        { "embedding", embedding }
                    }
                });
            ImageProcessed?.Invoke(this, true);
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
    public override async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            if (!Guid.TryParse(client.Auth.CurrentUser?.Id, out var userId)) return;
            Student = await client.From<SupabaseShared.Models.Student>()
                .Where(s => s.Id == userId)
                .Single();
            var faceDetectorTask = Task.Run(() => new FaceDetector());
            var faceEmbedderTask = Task.Run(() => new FaceEmbedder());
            await Task.WhenAll(faceDetectorTask, faceEmbedderTask);
            _detector = faceDetectorTask.Result;
            _embedder = faceEmbedderTask.Result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }
}