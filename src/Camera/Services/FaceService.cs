using System.Drawing;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Camera.Services;

public class FaceService
{
    private readonly CascadeClassifier _faceCascade;

    public FaceService(IConfiguration configuration)
    {
        _faceCascade = new CascadeClassifier(configuration["HaarCascadePath"]);
    }
    
    public FaceService(string haarCascadePath)
    {
        _faceCascade = new CascadeClassifier(haarCascadePath);
    }

    public Task<List<Image<Bgr,byte>>> ProcessFrameAsync(Image<Bgr,byte> frame)
    {
        var gray = frame.Convert<Gray, byte>();
        var faces = _faceCascade.DetectMultiScale(gray, 1.1, 10, Size.Empty);

        var croppedFaces = faces.Select(face => CropFace(frame, face)).ToList();
        return Task.FromResult(croppedFaces);
    }

    private static Image<Bgr, byte> CropFace(Image<Bgr, byte> frame, Rectangle face)
    {
        return frame.GetSubRect(face).Clone();
    }
}