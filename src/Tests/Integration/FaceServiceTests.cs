using Backend.Face;
using Microsoft.Extensions.DependencyInjection;
using Supabase.Gotrue;
using SupabaseShared.Models;

namespace Tests.Integration;

public class FaceServiceTests : IClassFixture<MockAppBuilder>
{
    private readonly FaceService _faceService;
    
    private readonly Supabase.Client _supabase;

    public FaceServiceTests(MockAppBuilder mockAppBuilder)
    {
        _faceService = mockAppBuilder.Services.GetRequiredService<FaceService>();
        _supabase = mockAppBuilder.Services.GetRequiredService<Supabase.Client>();
    }
    
    [Theory]
    [InlineData("Images/NoFace.png")]
    public async Task TestRecognizeFaceAsync_NoFaceInImage_ReturnsNull(string path)
    {
        using var imageStream = File.OpenRead(path);
        var result = await _faceService.RecognizeFaceAsync(imageStream);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Images/OnePerson.jpeg")]
    public async Task TestRecognizeFaceAsync_FaceInImage_NotTrained_ReturnsNull(string path)
    {
        using var imageStream = File.OpenRead(path);
        var result = await _faceService.RecognizeFaceAsync(imageStream);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Images/Similar1.png", "Images/Similar2.png")]
    public async Task TestRecognizeFaceAsync_FaceInImage_Trained_ReturnsPerson(string path1, string path2)
    {
        using var imageStream1 = File.OpenRead(path1);
        var trained = await _faceService.TrainFaceAsync(Guid.Parse("22222222-2222-2222-2222-222222222222"), imageStream1);
        
        var imageStream2 = File.OpenRead(path2);
        var result = await _faceService.RecognizeFaceAsync(imageStream2);
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), result!.StudentId);
    }
}