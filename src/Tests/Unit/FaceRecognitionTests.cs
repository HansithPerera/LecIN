using Backend;
using Backend.Face;
using Emgu.CV;

namespace Tests.Unit;

public class FaceRecognitionTests
{
    private readonly CascadeClassifier _cascadeClassifier;
    private readonly ResFaceEmbedder _faceEmbedder;

    public FaceRecognitionTests()
    {
        const string haarCascadePath = "haarcascade_frontalface_default.xml";
        const string onnxModelPath = "arcfaceresnet100-8.onnx";

        _cascadeClassifier = new CascadeClassifier(haarCascadePath);
        _faceEmbedder = new ResFaceEmbedder(onnxModelPath, 112, 112, true);
    }

    [Theory]
    [InlineData("Images/OnePerson.jpeg")]
    [InlineData("Images/Similar1.png")]
    [InlineData("Images/Similar2.png")]
    public async Task TestMatFromImage(string imagePath)
    {
        var image = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        var mat = await Util.MatFromImageStreamAsync(image);
        Assert.NotNull(mat);
    }

    [Fact]
    public async Task MatSizeAfterLoadingImageIsCorrect()
    {
        var image = new FileStream("Images/OnePerson.jpeg", FileMode.Open, FileAccess.Read);
        var mat = await Util.MatFromImageStreamAsync(image);
        Assert.Equal(734, mat.Rows);
        Assert.Equal(577, mat.Cols);
    }

    [Fact]
    public async Task FaceDetectionFindsFaceInImage()
    {
        var image = new FileStream("Images/OnePerson.jpeg", FileMode.Open, FileAccess.Read);
        var mat = await Util.MatFromImageStreamAsync(image);
        var faces = Util.DetectFacesInMat(mat, _cascadeClassifier);
        Assert.True(faces.Length >= 1);
    }

    [Fact]
    public async Task FaceEmbeddingGenerationProducesCorrectSize()
    {
        var image = new FileStream("Images/OnePerson.jpeg", FileMode.Open, FileAccess.Read);
        var embedding = await Util.GetFaceEmbeddingAsync(image, _cascadeClassifier, _faceEmbedder);
        Assert.NotNull(embedding);
        Assert.Equal(512, embedding.Length);
    }

    [Theory]
    [InlineData(new[] { 3f, 4f }, new[] { 0.6f, 0.8f })]
    [InlineData(new[] { 1f, 2f, 2f }, new[] { 0.3333f, 0.6667f, 0.6667f })]
    public async Task TestNormalizeEmbedding(float[] embedding, float[] expectedNormalized)
    {
        var normalized = Util.NormalizeEmbedding(embedding);
        Assert.Equal(expectedNormalized.Length, normalized.Length);
        for (var i = 0; i < expectedNormalized.Length; i++)
        {
            Assert.InRange(normalized[i], expectedNormalized[i] - 0.0001f, expectedNormalized[i] + 0.0001f);
        }
    }
    
    [Theory]
    [InlineData(new[] { 1f, 0f }, new[] { 0f, 1f }, 0f)]
    [InlineData(new[] { 1f, 0f }, new[] { 1f, 0f }, 1f)]
    [InlineData(new[] { 1f, 2f, 3f }, new[] { 4f, 5f, 6f }, 0.9746318f)]
    public async Task TestCosineSimilarity(float[] emb1, float[] emb2, float expectedSimilarity)
    {
        var similarity = Util.CosineSimilarity(emb1, emb2);
        Assert.InRange(similarity, expectedSimilarity - 0.0001f, expectedSimilarity + 0.0001f);
    }
    
    [Theory]
    [InlineData(new[] { 1f, 2f }, new[] { 1f, 2f, 3f })]
    [InlineData(new[] { 1f, 2f, 3f }, new[] { 1f, 2f })]
    [InlineData(new float[] { }, new[] { 1f, 2f })]
    public async Task CosineSimilarity_ThrowsForDifferentLengthEmbeddings(float[] emb1, float[] emb2)
    {
        await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            Util.CosineSimilarity(emb1, emb2);
            await Task.CompletedTask;
        });
    }
    
    [Theory]
    [InlineData("Images/Similar1.png", "Images/Similar2.png")]
    public async Task FacesAreSimilarForSamePerson(string person1, string person2)
    {
        var image1 = new FileStream(person1, FileMode.Open, FileAccess.Read);
        var emb1 = await Util.GetFaceEmbeddingAsync(image1, _cascadeClassifier, _faceEmbedder);
        Assert.NotNull(emb1);

        var image2 = new FileStream(person2, FileMode.Open, FileAccess.Read);
        var emb2 = await Util.GetFaceEmbeddingAsync(image2, _cascadeClassifier, _faceEmbedder);
        Assert.NotNull(emb2);

        var similarity = Util.CosineSimilarity(emb1, emb2);
        Assert.True(similarity > 0.7f);
    }

    [Theory]
    [InlineData("Images/Similar1.png", "Images/OnePerson.jpeg")]
    public async Task FacesAreDissimilarForDifferentPersons(string person1, string person2)
    {
        var image1 = new FileStream(person1, FileMode.Open, FileAccess.Read);
        var emb1 = await Util.GetFaceEmbeddingAsync(image1, _cascadeClassifier, _faceEmbedder);
        Assert.NotNull(emb1);
        var image2 = new FileStream(person2, FileMode.Open, FileAccess.Read);
        var emb2 = await Util.GetFaceEmbeddingAsync(image2, _cascadeClassifier, _faceEmbedder);
        Assert.NotNull(emb2);
        var similarity = Util.CosineSimilarity(emb1, emb2);
        Assert.True(similarity < 0.5f);
    }

    [Theory]
    [InlineData("Images/NoFace.png")]
    public async Task NoFaceDetectedReturnsNull(string imagePath)
    {
        var image = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        var embedding = await Util.GetFaceEmbeddingAsync(image, _cascadeClassifier, _faceEmbedder);
        Assert.Null(embedding);
    }
}