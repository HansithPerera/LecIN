using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using Backend.Models;
using Microsoft.IdentityModel.Tokens;

namespace Backend;

public static class Util
{
    public static string GetSupabaseIssuer(string projectRef)
    {
        return $"https://{projectRef}.supabase.co/auth/v1";
    }

    public static SymmetricSecurityKey GetSymmetricSecurityKey([Base64String] string base64Secret)
    {
        var bytes = Convert.FromBase64String(base64Secret);
        return new SymmetricSecurityKey(bytes);
    }

    public static string GetCacheKey(string prefix, params object[] parts)
    {
        return $"{prefix}:{string.Join(":", parts)}";
    }
    
    public static string HashApiKey(string apiKey)
    {
        var bytes = Encoding.UTF8.GetBytes(apiKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static dynamic FlattenAttendance(Attendance attendance)
    {
        dynamic result = new ExpandoObject();
        result.StudentId = attendance.StudentId;
        result.StudentFirstName = attendance.Student?.FirstName;
        result.StudentLastName = attendance.Student?.LastName;

        result.ClassId = attendance.ClassId;
        result.ClassStartTime = attendance.Class?.StartTime;
        result.ClassEndTime = attendance.Class?.EndTime;
        result.ClassDuration = attendance.Class?.Duration;

        result.CourseCode = attendance.Class?.CourseCode;
        result.CourseYearId = attendance.Class?.CourseYearId;
        result.CourseSemesterCode = attendance.Class?.CourseSemesterCode;
        result.CourseName = attendance.Class?.Course?.Name;

        result.Timestamp = attendance.Timestamp;
        return result;
    }
}