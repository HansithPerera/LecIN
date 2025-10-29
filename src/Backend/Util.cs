using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace Backend;

public class Util
{
    public static Task<Mat> MatFromImageStreamAsync(Stream imageStream, int width = 200, int height = 200)
    {
        return Task.Run(() =>
        {
            var mat = new Mat();
            CvInvoke.Imdecode(imageStream, ImreadModes.AnyColor, mat);
            var grayMat = new Mat();
            CvInvoke.CvtColor(mat, grayMat, ColorConversion.Bgr2Gray);
            CvInvoke.Resize(grayMat, grayMat, new Size(width, height));
            return grayMat;
        });
    }
}