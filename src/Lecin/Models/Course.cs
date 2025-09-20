using Supabase.Postgrest.Models;

namespace Lecin.Models;

public class Course: BaseModel
{
    public string Code { get; set; }

    public int Year { get; set; }

    public int SemesterCode { get; set; }

    public string Name { get; set; }
}