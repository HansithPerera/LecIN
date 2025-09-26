using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SupabaseShared.Models;

namespace Lecin.PageModels.Teacher;

[QueryProperty(nameof(Course), "course")]
public partial class TeacherCourseViewPageModel(Supabase.Client client): BasePageModel
{
    [ObservableProperty] private Course? _course;
    
    [ObservableProperty] private ObservableCollection<Class>? _classes;


    public override async Task LoadDataAsync()
    {
        if (Course == null) return;

        var classes = await client.From<Class>()
            .Select("*")
            .Get();

        Classes = new ObservableCollection<Class>(classes.Models);
    }
}