using FaceONNX;
using FaceShared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tests.Unit;

public class FaceRecognitionTests
{
    private readonly FaceDetector _detector;
    private readonly FaceEmbedder _faceEmbedder;

    public FaceRecognitionTests()
    {
        _detector = new FaceDetector();
        _faceEmbedder = new FaceEmbedder();
    }

    [Theory]
    [InlineData("Images/OnePerson.jpeg")]
    [InlineData("Images/Similar1.png")]
    [InlineData("Images/Similar2.png")]
    public async Task TestMatFromImage(string imagePath)
    {
        var img = Image.LoadAsync<Rgb24>(imagePath);
        Assert.NotNull(img);
    }

    [Fact]
    public async Task MatSizeAfterLoadingImageIsCorrect()
    {
        var img = await Image.LoadAsync<Rgb24>("Images/OnePerson.jpeg");
        Assert.Equal(734, img.Height);
        Assert.Equal(577, img.Width);
    }

    [Fact]
    public async Task FaceDetectionFindsFaceInImage()
    {
        var image = await Image.LoadAsync<Rgb24>("Images/OnePerson.jpeg");
        var faces = await FaceUtil.DetectFaces(image, _detector);
        Assert.True(faces.Length >= 1);
    }

    [Fact]
    public async Task FaceEmbeddingGenerationProducesCorrectSize()
    {
        var image = await Image.LoadAsync<Rgb24>("Images/OnePerson.jpeg");
        var embedding = await FaceUtil.GetFaceEmbeddingAsync(image, _faceEmbedder);
        Assert.NotNull(embedding);
        Assert.Equal(512, embedding.Length);
    }

    [Theory]
    [InlineData(new[] { 3f, 4f }, new[] { 0.6f, 0.8f })]
    [InlineData(new[] { 1f, 2f, 2f }, new[] { 0.3333f, 0.6667f, 0.6667f })]
    public async Task TestNormalizeEmbedding(float[] embedding, float[] expectedNormalized)
    {
        var normalized = FaceUtil.NormalizeEmbedding(embedding);
        Assert.Equal(expectedNormalized.Length, normalized.Length);
        for (var i = 0; i < expectedNormalized.Length; i++)
            Assert.InRange(normalized[i], expectedNormalized[i] - 0.0001f, expectedNormalized[i] + 0.0001f);
    }

    [Theory]
    [InlineData(new[] { 1f, 0f }, new[] { 0f, 1f }, 0f)]
    [InlineData(new[] { 1f, 0f }, new[] { 1f, 0f }, 1f)]
    [InlineData(new[] { 1f, 2f, 3f }, new[] { 4f, 5f, 6f }, 0.9746318f)]
    public async Task TestCosineSimilarity(float[] emb1, float[] emb2, float expectedSimilarity)
    {
        var similarity = FaceUtil.CosineSimilarity(emb1, emb2);
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
            FaceUtil.CosineSimilarity(emb1, emb2);
            await Task.CompletedTask;
        });
    }

    [Theory]
    [InlineData("Images/Similar1.png", "Images/Similar2.png")]
    public async Task FacesAreSimilarForSamePerson(string person1, string person2)
    {
        var image = await Image.LoadAsync<Rgb24>(person1);
        var emb1 = await FaceUtil.DetectFaceAndGenerateEmbedding(image, _detector, _faceEmbedder);
        Assert.NotNull(emb1);
        var image2 = await Image.LoadAsync<Rgb24>(person2);
        var emb2 = await FaceUtil.DetectFaceAndGenerateEmbedding(image2, _detector, _faceEmbedder);
        Assert.NotNull(emb2);
        var similarity = FaceUtil.CosineSimilarity(emb1, emb2);
        Assert.True(similarity > 0.7f);
    }

    [Theory]
    [InlineData("Images/Similar1.png", "Images/OnePerson.jpeg")]
    public async Task FacesAreDissimilarForDifferentPersons(string person1, string person2)
    {
        var image1 = await Image.LoadAsync<Rgb24>(person1);
        var emb1 = await FaceUtil.DetectFaceAndGenerateEmbedding(image1, _detector, _faceEmbedder);
        var image2 = await Image.LoadAsync<Rgb24>(person2);
        var emb2 = await FaceUtil.DetectFaceAndGenerateEmbedding(image2, _detector, _faceEmbedder);
        Assert.NotNull(emb2);
        Assert.NotNull(emb1);
        var similarity = FaceUtil.CosineSimilarity(emb1, emb2);
        Assert.True(similarity < 0.5f);
    }

    [Theory]
    [InlineData("Images/NoFace.png")]
    public async Task NoFaceDetectedReturnsNull(string imagePath)
    {
        var image = await Image.LoadAsync<Rgb24>(imagePath);
        var detected = await FaceUtil.DetectFaces(image, _detector);
        Assert.Empty(detected);
    }
}