using System.Drawing;
using Backend.Face;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Backend;

public class Util
{
    private const double MatrixFaceScaling = 1.1;

    private const int MinMatrixNeighbors = 6;

    public static Task<Mat> MatFromImageStreamAsync(Stream imageStream)
    {
        return Task.Run(() =>
        {
            var mat = new Mat();
            CvInvoke.Imdecode(imageStream, ImreadModes.AnyColor, mat);
            return mat;
        });
    }

    public static async Task<float[]?> GetFaceEmbeddingAsync(Stream image, CascadeClassifier faceCascade,
        ResFaceEmbedder embedder)
    {
        var imageMat = await MatFromImageStreamAsync(image);
        var faces = DetectFacesInMat(imageMat, faceCascade);
        if (faces.Length == 0)
            return null;
        var faceMat = faces[0];

        var embedding = embedder.MatToEmbedding(faceMat);
        return NormalizeEmbedding(embedding);
    }

    public static Rectangle ExpandRectangle(Rectangle rect, Mat imageMat, double scale)
    {
        // Center of the original rectangle
        var centerX = rect.X + rect.Width / 2;
        var centerY = rect.Y + rect.Height / 2;

        // New width and height
        var newWidth = (int)(rect.Width * scale);
        var newHeight = (int)(rect.Height * scale);

        // Top-left coordinates, clamped to image bounds
        var x = Math.Max(centerX - newWidth / 2, 0);
        var y = Math.Max(centerY - newHeight / 2, 0);

        // Clamp width/height so rectangle stays inside the image
        newWidth = Math.Min(newWidth, imageMat.Cols - x);
        newHeight = Math.Min(newHeight, imageMat.Rows - y);

        return new Rectangle(x, y, newWidth, newHeight);
    }

    public static float[] NormalizeEmbedding(float[] embedding)
    {
        var norm = Math.Sqrt(embedding.Select(x => x * x).Sum());
        return embedding.Select(x => (float)(x / norm)).ToArray();
    }

    public static float CosineSimilarity(float[] emb1, float[] emb2)
    {
        if (emb1.Length != emb2.Length)
            throw new ArgumentException("Embedding vectors must have the same length.");

        float dot = 0;
        float norm1 = 0;
        float norm2 = 0;

        for (var i = 0; i < emb1.Length; i++)
        {
            dot += emb1[i] * emb2[i];
            norm1 += emb1[i] * emb1[i];
            norm2 += emb2[i] * emb2[i];
        }

        return dot / (float)(Math.Sqrt(norm1) * Math.Sqrt(norm2));
    }

    public static Mat[] DetectFacesInMat(Mat imageMat, CascadeClassifier faceCascade)
    {
        using var gray = new Mat();
        CvInvoke.CvtColor(imageMat, gray, ColorConversion.Bgr2Gray);

        var faces = faceCascade.DetectMultiScale(
            gray,
            MatrixFaceScaling,
            MinMatrixNeighbors
        );

        return faces.Select(rect => new Mat(imageMat, ExpandRectangle(rect, imageMat, 1.3))).ToArray();
    }
}