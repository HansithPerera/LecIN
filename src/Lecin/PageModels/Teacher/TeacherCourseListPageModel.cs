using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Supabase;
using SupabaseShared.Models;

namespace Lecin.PageModels.Teacher;

public partial class TeacherCourseListPageModel(Client client) : BasePageModel
{
    [ObservableProperty] private ObservableCollection<Course> _courses = [];

    public override async Task LoadDataAsync()
    {
        var courses = await client.From<Course>().Select("*").Get();
        Courses = new ObservableCollection<Course>(courses.Models);
    }
}