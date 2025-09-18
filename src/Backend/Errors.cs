namespace Backend;

public static class Errors
{
    public enum DeleteError
    {
        NotFound
    }

    public enum GetError
    {
        NotFound
    }

    public enum InsertError
    {
        Conflict
    }

    public enum NewCourseError
    {
        MissingCode,
        MissingName,
        InvalidYear,
        InvalidSemesterCode,
        Conflict,
        UnknownError
    }

    public enum NewTeacherError
    {
        MissingFirstName,
        MissingLastName,
        Conflict,
        UnknownError
    }

    public enum UpdateError
    {
        NotFound,
        Conflict
    }
    
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

    public enum NewCameraError
    {
        MissingLocation,
        MissingName,
        Conflict,
        UnknownError,
    }
    
    public enum GenerateCameraApiKeyError
    {
        CameraNotFound,
        UnknownError
    }
}