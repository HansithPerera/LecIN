using System.Text.Json;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Supabase;

namespace Lecin.PageModels.Student;

public partial class ClassmatesPageModel(Client client) : ObservableObject
{
    [ObservableProperty] private string _courseName = string.Empty;
    [ObservableProperty] private ObservableCollection<StudentInfo> _classmates = new();

    public class StudentInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    private class ClassmateDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public async Task LoadClassmatesAsync(string courseCode)
    {
        CourseName = courseCode;
        Classmates.Clear();

        try
        {
            var resp = await client.Rpc(
                "classmates_for_course",
                new { course_code = courseCode }
            );

            var rows = JsonSerializer.Deserialize<List<ClassmateDto>>(
                resp.Content ?? "[]",
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<ClassmateDto>();

            foreach (var row in rows)
            {
                var name = $"{row.FirstName} {row.LastName}".Trim();
                if (string.IsNullOrWhiteSpace(name)) name = "(No name)";

                Classmates.Add(new StudentInfo
                {
                    Name = name,
                    Email = row.Email ?? string.Empty
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading classmates: {ex.Message}");
        }
    }
}
