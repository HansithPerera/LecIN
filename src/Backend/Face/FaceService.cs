using System.Drawing;
using Emgu.CV;
using Emgu.CV.Face;

namespace Backend.Face;

public class RecognizedFace
{
    public required Guid PersonId { get; set; }
}

public class FaceService(CascadeClassifier faceCascade, LBPHFaceRecognizer recognizer)
{
    private int _maxIndex = 0;
    
    private readonly Dictionary<int, string> _faceMap = new();
    
    private int InsertFace(Guid personId)
    {
        var nextIndex = _maxIndex++;
        _faceMap[nextIndex] = personId.ToString();
        return nextIndex;
    }

    public async Task TrainOnFaceAsync(Guid personId, Stream image)
    {
        await Task.Run(async () =>
        {
            var mat = await DetectFaceAsync(image);
            if (mat == null)
                return;
            recognizer.Update([ mat ], [ InsertFace(personId) ]);
        });
    }

    private async Task<Mat?> DetectFaceAsync(Stream image)
    {
        return await Task.Run(async () =>
        {
            var mat = await Util.MatFromImageStreamAsync(image);
            var faces = faceCascade.DetectMultiScale(mat, 1.1, 4, Size.Empty);
            if (faces.Length == 0) return null;
            var face = faces.First();
            var faceMat = new Mat(mat, face);
            CvInvoke.Resize(faceMat, faceMat, new Size(200, 200));
            return faceMat;
        });
    }
    
    public async Task<RecognizedFace?> RecognizeFaceAsync(Stream image, double threshold = 80)
    {
        return await Task.Run(async () =>
        {
            var face = await DetectFaceAsync(image);
            if (face == null) return null;
            var prediction = recognizer.Predict(face);
            var label = prediction.Label;
            var distance = prediction.Distance;
            if (label == -1 || distance > threshold) return null;
            var personId = _faceMap.GetValueOrDefault(label);
            if (personId == null) return null;
            return new RecognizedFace
            {
                PersonId = Guid.Parse(personId)
            };
        });
    }
}