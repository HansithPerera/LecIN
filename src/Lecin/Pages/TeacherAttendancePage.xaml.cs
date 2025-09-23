using System.Collections.ObjectModel;
using System.ComponentModel;
using Lecin.Models;
using Lecin.Services;

namespace Lecin.Pages;

public partial class TeacherAttendancePage : ContentPage
{
    private readonly VM vm = new();
    public TeacherAttendancePage()
    {
        InitializeComponent();          // generated when x:Class matches this namespace/class
        BindingContext = vm;
    }

    private async void OnFetchClicked(object? sender, EventArgs e)
    {
        vm.Clear();
        if (!Guid.TryParse(StudentIdEntry.Text?.Trim(), out var studentId))
        {
            vm.Message = "Please enter a valid GUID.";
            return;
        }

        try
        {
            var dto = await BackendClient.GetStudentAttendancePercentAsync(studentId);
            if (dto.TotalClasses == 0)
            {
                vm.Message = "No classes/enrollments found for that student.";
                return;
            }

            vm.OverallText = $"{dto.Attended}/{dto.TotalClasses} classes • {dto.Percentage:F1}%";
            foreach (var c in dto.ByCourse)
                vm.Courses.Add(new CourseRow(c.CourseCode, c.Attended, c.TotalClasses, c.Percentage));
        }
        catch (Exception ex)
        {
            vm.Message = $"Error: {ex.Message}";
        }
    }

    class VM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        void N(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        public ObservableCollection<CourseRow> Courses { get; } = new();
        string overall = ""; public string OverallText { get => overall; set { overall = value; N(nameof(OverallText)); N(nameof(HasData)); } }
        string message = ""; public string Message { get => message; set { message = value; N(nameof(Message)); N(nameof(HasMessage)); } }
        public bool HasData => Courses.Count > 0 || !string.IsNullOrWhiteSpace(OverallText);
        public bool HasMessage => !string.IsNullOrWhiteSpace(Message);
        public void Clear() { Courses.Clear(); OverallText = ""; Message = ""; }
    }

    public record CourseRow(string CourseCode, int Attended, int Total, double Percentage)
    {
        public string TotalsLine => $"{Attended}/{Total} • {Percentage:F1}%";
    }
}