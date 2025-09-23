
using Camera;
using Camera.Services;
using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using FaceService = Backend.Face.FaceService;

namespace Tests.Unit;

public class FaceRecognitionTests
{
    
    private readonly FaceService _faceService;
    
    public FaceRecognitionTests()
    {
        const string haarCascadePath = "haarcascade_frontalface_default.xml";
        _faceService = new FaceService(new CascadeClassifier(haarCascadePath), new LBPHFaceRecognizer());
        var image = new FileStream("Images/OnePerson.jpeg", FileMode.Open, FileAccess.Read);
        _faceService.TrainOnFaceAsync(Guid.Empty, image).Wait();
    }

    [Fact]
    public async Task TestFaceRecognition()
    {
        var image = new FileStream("Images/OnePerson.jpeg", FileMode.Open, FileAccess.Read);
        var face = await _faceService.RecognizeFaceAsync(image);
        Assert.NotNull(face);
    }
}