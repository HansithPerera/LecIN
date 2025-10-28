namespace Camera.Services;

public class FrameBuffer
{
    private object Lock { get; } = new();

    private byte[]? CurrentFrame { get; set; }

    public void SetFrame(byte[] frame)
    {
        lock (Lock)
        {
            CurrentFrame = frame;
        }
    }

    public void GetFrame(out byte[]? frame)
    {
        lock (Lock)
        {
            frame = CurrentFrame;
        }
    }
}