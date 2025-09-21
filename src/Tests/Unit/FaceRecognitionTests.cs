
using Camera;
using Camera.Services;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Tests.Unit;

public class FaceRecognitionTests
{
    
    private FaceService _faceService;
    
    public FaceRecognitionTests()
    {
        var haarCascadePath = "haarcascade_frontalface_default.xml";
        _faceService = new FaceService(haarCascadePath);
    }

    [Fact]
    public async Task TestFaceRecognition()
    {
        var image = new Image<Bgr, byte>("Images/OnePerson.jpeg");
        var faces = await _faceService.ProcessFrameAsync(image);
        Assert.NotEmpty(faces);
        Assert.Single(faces);
        Assert.True(image.Size.Width > faces[0].Size.Width);
    }
}