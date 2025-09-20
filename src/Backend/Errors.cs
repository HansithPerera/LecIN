namespace Backend;

public static class Errors
{
    public enum CheckInError
    {
        FaceRecognitionFailed,
        StudentNotFound,
        AlreadyCheckedIn,
        ClassNotFound,
        StudentNotEnrolled,
        UnknownError
    }

    public enum ClassRetrievalError
    {
        NoCameraFound,
        NoClassFound,
        UnknownError
    }

    public enum FaceRecognitionError
    {
        NoFaceDetected,
        MultipleFacesDetected,
        RecognitionFailed,
        InvalidImageFormat,
        UnknownError
    }
}