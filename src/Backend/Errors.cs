namespace Backend;

public static class Errors
{
    public enum InsertError
    {
        Conflict,
    }
    
    public enum UpdateError
    {
        NotFound,
        Conflict,
    }
    
    public enum DeleteError
    {
        NotFound,
    }
}