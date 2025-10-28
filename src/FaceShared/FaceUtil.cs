using FaceONNX;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static SixLabors.ImageSharp.Color;
using PointF = SixLabors.ImageSharp.PointF;
using Rectangle = SixLabors.ImageSharp.Rectangle;

namespace FaceShared;

public static class FaceUtil
{
    public static Image<Rgb24> FaceToImage(float[][,] faceArray)
    {
        var height = faceArray[0].GetLength(0);
        var width = faceArray[0].GetLength(1);
        var image = new Image<Rgb24>(width, height);

        image.ProcessPixelRows(pixelAccessor =>
        {
            for (var y = 0; y < pixelAccessor.Height; y++)
            {
                var row = pixelAccessor.GetRowSpan(y);
                for (var x = 0; x < pixelAccessor.Width; x++)
                {
                    var r = (byte)(faceArray[2][y, x] * 255);
                    var g = (byte)(faceArray[1][y, x] * 255);
                    var b = (byte)(faceArray[0][y, x] * 255);
                    row[x] = new Rgb24(r, g, b);
                }
            }
        });

        return image;
    }

    public static float[][,] GetImageFloatArray(Image<Rgb24> image)
    {
        var array = new[]
        {
            new float [image.Height, image.Width],
            new float [image.Height, image.Width],
            new float [image.Height, image.Width]
        };

        image.ProcessPixelRows(pixelAccessor =>
        {
            for (var y = 0; y < pixelAccessor.Height; y++)
            {
                var row = pixelAccessor.GetRowSpan(y);
                for (var x = 0; x < pixelAccessor.Width; x++)
                {
                    array[2][y, x] = row[x].R / 255.0F;
                    array[1][y, x] = row[x].G / 255.0F;
                    array[0][y, x] = row[x].B / 255.0F;
                }
            }
        });

        return array;
    }

    public static async Task<Rectangle[]> DetectFaces(Image<Rgb24> image, FaceDetector detector)
    {
        var imageFloatArray = GetImageFloatArray(image.CloneAs<Rgb24>());
        var recs = await Task.Run(() => detector.Forward(imageFloatArray));
        return recs.Select(RecognitionResultToRectangle).ToArray();
    }

    private static Rectangle RecognitionResultToRectangle(FaceDetectionResult result)
    {
        return new Rectangle(result.Box.Left, result.Box.Top, result.Box.Width, result.Box.Height);
    }

    public static async Task<Image<Rgb24>> DrawRectanglesOnImage(Image<Rgb24> image, Rectangle[] rectangles)
    {
        return await Task.Run(() =>
        {
            var img = image.Clone();
            img.Mutate(ctx =>
            {
                foreach (var rect in rectangles)
                    ctx.DrawPolygon(
                        Red,
                        2,
                        new PointF(rect.Left, rect.Top),
                        new PointF(rect.Right, rect.Top),
                        new PointF(rect.Right, rect.Bottom),
                        new PointF(rect.Left, rect.Bottom)
                    );
            });
            return img;
        });
    }

    public static async Task<Image<Rgb24>> ResizeImage(Image<Rgb24> image, Rectangle rect)
    {
        return await Task.Run(() =>
        {
            var faceImage = image.Clone(ctx => ctx.Crop(rect));
            return faceImage;
        });
    }

    public static Rectangle ExpandRectangle(Rectangle rect, Image<Rgb24> image, double scale)
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
        newWidth = Math.Min(newWidth, image.Width - x);
        newHeight = Math.Min(newHeight, image.Height - y);

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

    public static async Task<float[]?> GenerateEmbedding(
        Image<Rgb24> image,
        Rectangle rectangle,
        FaceEmbedder embedder)
    {
        var expanded = ExpandRectangle(rectangle, image, 1.3);
        var faceImage = await ResizeImage(image, expanded);
        var embedding = await GetFaceEmbeddingAsync(faceImage, embedder);
        return embedding;
    }

    public static async Task<float[]> GetFaceEmbeddingAsync(Image<Rgb24> faceImage, FaceEmbedder embedder)
    {
        return await Task.Run(() => embedder.Forward(GetImageFloatArray(faceImage)));
    }

    public static async Task<float[]?> DetectFaceAndGenerateEmbedding(
        Image<Rgb24> image,
        FaceDetector faceDetector,
        FaceEmbedder embedder)
    {
        var faces = await DetectFaces(image, faceDetector);
        if (faces.Length == 0)
            return null;
        var firstFace = faces[0];
        return await GenerateEmbedding(image, firstFace, embedder);
    }
}