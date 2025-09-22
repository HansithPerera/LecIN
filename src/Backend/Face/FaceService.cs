using OpenCvSharp;
using OpenCvSharp.Face;
using System.Text.Json;

namespace Backend.Face;

public class RecognizedFace
{
    public required string PersonId { get; set; }
    public required float Confidence { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public FaceRectangle? Rectangle { get; set; }
}

public class FaceRectangle
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class TrainingData
{
    public required string PersonId { get; set; }
    public required string PersonName { get; set; }
    public required string ImagePath { get; set; }
}

public class FaceService : IDisposable
{
    private readonly CascadeClassifier _faceCascade;
    private readonly FaceRecognizer _faceRecognizer;
    private readonly string _modelPath;
    private readonly string _mappingPath;
    private readonly Dictionary<int, PersonInfo> _personMappings;
    private readonly ILogger<FaceService>? _logger;

    private class PersonInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public FaceService(ILogger<FaceService>? logger = null)
    {
        _logger = logger;
        
        // Initialize paths
        var dataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "FaceRecognition");
        var modelsPath = Path.Combine(dataPath, "Models");
        _modelPath = Path.Combine(modelsPath, "face_model.yml");
        _mappingPath = Path.Combine(modelsPath, "person_mappings.json");
        
        var cascadePath = Path.Combine(modelsPath, "haarcascade_frontalface_alt.xml");
        
        if (!File.Exists(cascadePath))
        {
            throw new FileNotFoundException($"Haar cascade file not found at {cascadePath}");
        }

        // Initialize OpenCV components
        _faceCascade = new CascadeClassifier(cascadePath);
       _faceRecognizer = LBPHFaceRecognizer.Create(
            radius: 1, 
            neighbors: 8, 
            gridX: 8, 
            gridY: 8, 
            threshold: 80.0
        );
        
        _personMappings = new Dictionary<int, PersonInfo>();
        
        // Load existing model if available
        LoadExistingModel();
    }

    public async Task<List<RecognizedFace>> RecognizeFacesAsync(Stream imageData)
    {
        try
        {
            using var mat = LoadImageFromStream(imageData);
            if (mat.Empty())
            {
                _logger?.LogWarning("Empty or invalid image provided for face recognition");
                return new List<RecognizedFace>();
            }

            return await Task.Run(() => RecognizeFaces(mat));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during face recognition");
            throw new InvalidOperationException("Face recognition failed", ex);
        }
    }

    public async Task<List<FaceRectangle>> DetectFacesAsync(Stream imageData)
    {
        try
        {
            using var mat = LoadImageFromStream(imageData);
            var faces = DetectFaces(mat);
            
            return faces.Select(f => new FaceRectangle 
            { 
                X = f.X, 
                Y = f.Y, 
                Width = f.Width, 
                Height = f.Height 
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during face detection");
            throw new InvalidOperationException("Face detection failed", ex);
        }
    }

    public async Task<bool> TrainModelAsync(List<TrainingData> trainingData)
    {
        try
        {
            if (!trainingData.Any())
            {
                _logger?.LogWarning("No training data provided");
                return false;
            }

            var faces = new List<Mat>();
            var labels = new List<int>();
            var labelCounter = 0;
            
            _personMappings.Clear();

            var groupedData = trainingData.GroupBy(x => x.PersonId);
            
            foreach (var group in groupedData)
            {
                var personInfo = new PersonInfo 
                { 
                    Id = group.Key, 
                    Name = group.First().PersonName 
                };
                _personMappings[labelCounter] = personInfo;
                
                foreach (var data in group)
                {
                    if (!File.Exists(data.ImagePath))
                    {
                        _logger?.LogWarning($"Training image not found: {data.ImagePath}");
                        continue;
                    }

                    using var image = Cv2.ImRead(data.ImagePath);
                    if (image.Empty())
                    {
                        _logger?.LogWarning($"Could not load training image: {data.ImagePath}");
                        continue;
                    }

                    var detectedFaces = DetectFaces(image);
                    
                    foreach (var faceRect in detectedFaces)
                    {
                        using var face = ExtractFace(image, faceRect);
                        faces.Add(face.Clone());
                        labels.Add(labelCounter);
                    }
                }
                labelCounter++;
            }

            if (faces.Count < 2)
            {
                _logger?.LogWarning("Insufficient training data - need at least 2 face samples");
                return false;
            }

            // Train the model
            await Task.Run(() => _faceRecognizer.Train(faces, labels.ToArray()));
            
            // Save the model
            _faceRecognizer.Write(_modelPath);
            await SavePersonMappingsAsync();
            
            // Cleanup
            faces.ForEach(f => f.Dispose());
            
            _logger?.LogInformation($"Model trained successfully with {faces.Count} faces from {_personMappings.Count} persons");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Training failed");
            throw new InvalidOperationException("Model training failed", ex);
        }
    }

    public bool IsModelTrained()
    {
        return File.Exists(_modelPath) && _personMappings.Count > 0;
    }

    private List<RecognizedFace> RecognizeFaces(Mat image)
    {
        var results = new List<RecognizedFace>();
        var faces = DetectFaces(image);
        
        if (!IsModelTrained())
        {
            _logger?.LogWarning("Face recognition model not trained");
            // Return detected faces but mark as unrecognized
            return faces.Select(f => new RecognizedFace
            {
                PersonId = "unknown",
                PersonName = "Unknown",
                Confidence = 0.0f,
                Rectangle = new FaceRectangle { X = f.X, Y = f.Y, Width = f.Width, Height = f.Height }
            }).ToList();
        }
        
        foreach (var faceRect in faces)
        {
            using var face = ExtractFace(image, faceRect);
            
            _faceRecognizer.Predict(face, out int label, out double confidence);
            
            // Convert confidence to 0-1 scale (lower OpenCV confidence = higher certainty)
            var normalizedConfidence = Math.Max(0.0, Math.Min(1.0, (100.0 - confidence) / 100.0));
            var isRecognized = confidence < 80.0 && _personMappings.ContainsKey(label);
            
            var personInfo = isRecognized ? _personMappings[label] : null;
            
            results.Add(new RecognizedFace
            {
                PersonId = personInfo?.Id ?? "unknown",
                PersonName = personInfo?.Name ?? "Unknown",
                Confidence = (float)normalizedConfidence,
                Rectangle = new FaceRectangle 
                { 
                    X = faceRect.X, 
                    Y = faceRect.Y, 
                    Width = faceRect.Width, 
                    Height = faceRect.Height 
                }
            });
        }
        
        return results;
    }

    private List<Rect> DetectFaces(Mat image)
    {
        using var grayImage = new Mat();
        Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
        Cv2.EqualizeHist(grayImage, grayImage);
        
        var faces = _faceCascade.DetectMultiScale(
            grayImage,
            scaleFactor: 1.1,
            minNeighbors: 3,
            flags: HaarDetectionTypes.ScaleImage,
            minSize: new OpenCvSharp.Size(50, 50),
            maxSize: new OpenCvSharp.Size(300, 300)
        );
        
        return faces.ToList();
    }

    private Mat ExtractFace(Mat image, Rect faceRect)
    {
        using var face = new Mat(image, faceRect);
        using var grayFace = new Mat();
        Cv2.CvtColor(face, grayFace, ColorConversionCodes.BGR2GRAY);
        
        var resizedFace = new Mat();
        Cv2.Resize(grayFace, resizedFace, new OpenCvSharp.Size(100, 100));
        Cv2.EqualizeHist(resizedFace, resizedFace);
        
        return resizedFace;
    }

    private Mat LoadImageFromStream(Stream imageStream)
    {
        using var memoryStream = new MemoryStream();
        imageStream.CopyTo(memoryStream);
        var imageBytes = memoryStream.ToArray();
        return Cv2.ImDecode(imageBytes, ImreadModes.Color);
    }

    private void LoadExistingModel()
    {
        if (File.Exists(_modelPath) && File.Exists(_mappingPath))
        {
            try
            {
                _faceRecognizer.Read(_modelPath);
                var json = File.ReadAllText(_mappingPath);
                var loadedMappings = JsonSerializer.Deserialize<Dictionary<string, PersonInfo>>(json);
                
                if (loadedMappings != null)
                {
                    _personMappings.Clear();
                    foreach (var kvp in loadedMappings)
                    {
                        if (int.TryParse(kvp.Key, out int key))
                        {
                            _personMappings[key] = kvp.Value;
                        }
                    }
                }
                
                _logger?.LogInformation("Existing face recognition model loaded successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to load existing model");
            }
        }
    }

    private async Task SavePersonMappingsAsync()
    {
        var json = JsonSerializer.Serialize(_personMappings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_mappingPath, json);
    }

    public void Dispose()
    {
        _faceCascade?.Dispose();
        _faceRecognizer?.Dispose();
        GC.SuppressFinalize(this);
    }
}