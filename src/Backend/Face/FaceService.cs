namespace Backend.Face;

public class RecognizedFace
{
    public required string PersonId { get; set; }
    public required float Confidence { get; set; }
}

public class FaceService
{
    public Task<List<RecognizedFace>> RecognizeFacesAsync(Stream imageData)
    {
        throw new NotImplementedException();
    }
}