using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Dnn;

namespace Backend.Face;

public class ResFaceEmbedder(string onnxFile, int inputWidth, int inputHeight, bool bgrToRgb)
{
    private readonly Net _net = DnnInvoke.ReadNetFromONNX(onnxFile);

    /// <summary>
    ///     Generates an embedding from the given face Mat using the provided network.
    /// </summary>
    /// <param name="faceMat">
    ///     The Mat containing the face image, already cropped and aligned
    /// </param>
    /// <returns>
    ///     A float array representing the ArcFace embedding vector
    /// </returns>
    public float[] MatToEmbedding(Mat faceMat)
    {
        var resizedFace = new Mat();
        CvInvoke.Resize(faceMat, resizedFace, new Size(inputWidth, inputHeight));

        if (bgrToRgb) CvInvoke.CvtColor(resizedFace, resizedFace, ColorConversion.Bgr2Rgb);

        var blob = DnnInvoke.BlobFromImage(resizedFace);
        return BlobToEmbedding(blob);
    }

    /// <summary>
    ///     Creates embeddings from the given blob using the ArcFace network.
    /// </summary>
    /// <param name="blob">
    ///     The input blob created from the face image
    /// </param>
    /// <returns></returns>
    public float[] BlobToEmbedding(IInputArray blob)
    {
        _net.SetInput(blob);
        var embeddingMat = _net.Forward();
        var embedding = new float[embeddingMat.Cols];
        embeddingMat.CopyTo(embedding);
        return embedding;
    }
}