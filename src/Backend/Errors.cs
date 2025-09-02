namespace Backend;

public static class Errors
{
    public enum DeleteError
    {
        NotFound
    }

    public enum InsertError
    {
        Conflict
    }

    public enum NewTeacherError
    {
        MissingFirstName,
        MissingLastName,
        Conflict,
        UnknownError
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

    public enum UpdateError
    {
        NotFound,
        Conflict
    }
}