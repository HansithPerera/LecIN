using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase.Postgrest;
using SupabaseShared.Models;
using Client = Supabase.Functions.Client;
using Location = SupabaseShared.Models.Location;

namespace Lecin.PageModels.Teacher;

[QueryProperty(nameof(Course), "course")]
public partial class TeacherCourseViewPageModel(Supabase.Client client, ModalErrorHandler errorHandler): BasePageModel
{
    [ObservableProperty] private Course? _course;
    
    [ObservableProperty] private ObservableCollection<Class>? _classes;
    
    [ObservableProperty] private DateTimeOffset _newClassStartTime = DateTimeOffset.Now;
    [ObservableProperty] private DateTimeOffset _newClassEndTime = DateTimeOffset.Now.AddHours(1);
    
    [ObservableProperty] private Location? _newClassLocation;
    [ObservableProperty] private ObservableCollection<Location> _availableLocations = [];

    
    [RelayCommand]
    public async Task CreateClassAsync()
    {
        if (Course == null || NewClassLocation == null) return;
        try
        {
            var newClass = await client.Functions.Invoke<Class>("create-class", options: new Client.InvokeFunctionOptions()
            {
                Body = new Dictionary<string, object>()
                {
                    { "CourseCode", Course.Code },
                    { "CourseYear", Course.Year },
                    { "CourseSemesterCode", Course.SemesterCode },
                    { "StartTime", NewClassStartTime },
                    { "EndTime", NewClassEndTime },
                    { "Location", NewClassLocation.Id }
                }
            });
            if (newClass != null)
            {
                Classes?.Add(newClass);
            }
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(ex);
        }
    }

    public override async Task LoadDataAsync()
    {
        if (Course == null) return;

        try
        {
            var classes = await client.From<Class>()
                .Filter(e => e.CourseCode, Constants.Operator.Equals, Course.Code)
                .Filter(e => e.CourseYear, Constants.Operator.Equals, Course.Year)
                .Filter(e => e.CourseSemesterCode, Constants.Operator.Equals, Course.SemesterCode)
                .Select("*")
                .Order(e => e.StartTime, Constants.Ordering.Ascending)
                .Get();
            
            var locations = await client.From<Location>()
                .Select("*")
                .Get();
            
            AvailableLocations = new ObservableCollection<Location>(locations.Models);
            Classes = new ObservableCollection<Class>(classes.Models);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading classes: {ex.Message}");
        }
    }
}