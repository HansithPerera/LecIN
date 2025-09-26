using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase.Functions;
using SupabaseShared.Models;

namespace Lecin.PageModels.Teacher;

[QueryProperty(nameof(Class), "class")]
public partial class TeacherClassViewPageModel(Supabase.Client client): BasePageModel
{
    [ObservableProperty] 
    private SupabaseShared.Models.Class? _class;
    
    [ObservableProperty]
    private ObservableCollection<Attendance>? _attendances;
    
    [ObservableProperty]
    private string _reason = "";
    
    [ObservableProperty]
    private string _studentId = "";
    
    
    public async Task AddAttendance()
    {
        var studentId = StudentId.Trim();
        var reason = Reason.Trim();
        if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(reason)) return;

        if (!Guid.TryParse(studentId, out var studentGuid))
        {
            return;
        }
        if (Class == null) return;

        await client.Functions.Invoke("teacher-set-attendance", options: new Client.InvokeFunctionOptions()
        {
            Body = new Dictionary<string, object>()
            {
                { "ClassId", Class.Id },
                { "StudentId", studentGuid },
                { "Reason", reason }
            }
        });
        
        await LoadDataAsync();
        StudentId = "";
        Reason = "";
    }
    
    public override async Task LoadDataAsync()
    {
        try
        {
            if (Class == null) return;

            var attendances = await client.From<Attendance>()
                .Select("*")
                .Get();

            Attendances = new ObservableCollection<Attendance>(attendances.Models);
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to load attendances");
        }
    }
    
}