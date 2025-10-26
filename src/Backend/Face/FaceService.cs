using Backend.Database;
using Emgu.CV;
using SupabaseShared.Models;

namespace Backend.Face;

public class FaceService(CascadeClassifier faceCascade, ResFaceEmbedder embedder, Repository repository)
{
    public async Task<StudentFace?> RecognizeFaceAsync(Stream image)
    {
        var embedding = await Util.GetFaceEmbeddingAsync(image, faceCascade, embedder);
        if (embedding == null)
            return null;
        var response = await repository.GetRecognizedFaceAsync(embedding);
        return response;
    }

    public async Task<bool> TrainFaceAsync(Guid studentId, Stream image)
    {
        var embedding = await Util.GetFaceEmbeddingAsync(image, faceCascade, embedder);
        if (embedding == null)
            return false;

        await repository.StoreStudentFaceAsync(studentId, embedding);
        return true;
    }
}