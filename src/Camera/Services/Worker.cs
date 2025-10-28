using Emgu.CV;
using Emgu.CV.Structure;
using FaceONNX;
using FaceShared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using Supabase;

namespace Camera.Services;

public class Worker(
    FaceEmbedder embedder,
    FrameBuffer buffer,
    FaceDetector detector,
    IConfiguration configuration,
    Client client,
    ILogger<Worker> logger) : BackgroundService
{
    private readonly string _apiKey = configuration.GetValue<string>("CameraApiKey") ??
                                      throw new InvalidOperationException("API Key not found in configuration");

    private readonly int _cameraIndex = configuration.GetValue("CameraIndex", 0);

    private async Task UpdateFrameBuffer(Image<Rgb24> frame, Rectangle[] faces)
    {
        var drawnFrameTask = await FaceUtil.DrawRectanglesOnImage(frame, faces);
        using var ms = new MemoryStream();
        await drawnFrameTask.SaveAsync(ms, new JpegEncoder());
        buffer.SetFrame(ms.ToArray());
    }

    private async Task ProcessDetectedFace(Image<Rgb24> frame, Rectangle faceRect)
    {
        var face = await FaceUtil.ResizeImage(frame, faceRect);
        var embedding = await FaceUtil.GetFaceEmbeddingAsync(face, embedder);
        var options = new Supabase.Functions.Client.InvokeFunctionOptions
        {
            Headers = new Dictionary<string, string>
            {
                { "apikey", _apiKey }
            },
            Body = new Dictionary<string, object>
            {
                { "embedding", embedding }
            }
        };
        await client.Functions.Invoke("camera-check-in", options: options);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var capture = new VideoCapture(_cameraIndex);
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            try
            {
                // Capture frame from camera
                var mat = capture.QueryFrame();
                if (mat == null) continue;

                // Convert Mat to ImageSharp Image
                var image = MatToImage(mat);

                // Detect faces in the frame
                var detectedFaces = await FaceUtil.DetectFaces(image, detector);
                await UpdateFrameBuffer(image, detectedFaces);

                if (detectedFaces.Length == 0)
                    logger.LogInformation("No faces detected");
                else
                    logger.LogInformation("Detected {count} faces", detectedFaces.Length);

                // Process each detected face asynchronously
                var tasks = detectedFaces.Select(rect => ProcessDetectedFace(image, rect)).ToArray();
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                logger.LogError("Error in worker loop: {message}", ex.Message);
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }

    private static Image<Rgb24> MatToImage(Mat mat)
    {
        var bytes = mat.ToImage<Bgr, byte>().ToJpegData();
        return Image.Load<Rgb24>(bytes);
    }
}