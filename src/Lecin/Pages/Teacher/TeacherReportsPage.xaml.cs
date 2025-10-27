using SupabaseShared.Models;

namespace Lecin.Pages.Teacher;

[QueryProperty(nameof(CourseCode), "courseCode")]
[QueryProperty(nameof(CourseName), "courseName")]
public partial class TeacherReportsPage : ContentPage
{
    private List<CourseInfo> _courses = new();
    private List<AtRiskStudent> _atRiskStudents = new();
    private string? _preselectedCourseCode = null;
    private string? _preselectedCourseName = null;

    public TeacherReportsPage()
    {
        InitializeComponent();
        LoadCourses();
        InitializeDatePickers();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        // Try to preselect course if parameters were passed
        if (!string.IsNullOrEmpty(_preselectedCourseCode) && !string.IsNullOrEmpty(_preselectedCourseName))
        {
            var courseName = $"{_preselectedCourseCode} - {_preselectedCourseName}";
            var courseInfo = _courses.FirstOrDefault(c => c.Name == courseName);
            if (courseInfo != null)
            {
                var index = _courses.IndexOf(courseInfo);
                CoursePicker.SelectedIndex = index;
            }
        }
    }

    public string? CourseCode
    {
        get => _preselectedCourseCode;
        set => _preselectedCourseCode = value;
    }

    public string? CourseName
    {
        get => _preselectedCourseName;
        set => _preselectedCourseName = value;
    }

    private void LoadCourses()
    {
        // Mock data for now - in real implementation, this would load from database
        _courses = new List<CourseInfo>
        {
            new CourseInfo { Id = 1, Name = "Mathematics 101" },
            new CourseInfo { Id = 2, Name = "Computer Science 201" },
            new CourseInfo { Id = 3, Name = "Physics 101" },
            new CourseInfo { Id = 4, Name = "Chemistry 101" }
        };

        CoursePicker.ItemsSource = _courses.Select(c => c.Name).ToList();
    }

    private void InitializeDatePickers()
    {
        // Set default date range to last 30 days
        StartDatePicker.Date = DateTime.Now.AddDays(-30);
        EndDatePicker.Date = DateTime.Now;
    }

    private async void OnGenerateReportClicked(object sender, EventArgs e)
    {
        if (CoursePicker.SelectedIndex == -1)
        {
            await DisplayAlert("Error", "Please select a course", "OK");
            return;
        }

        if (EndDatePicker.Date < StartDatePicker.Date)
        {
            await DisplayAlert("Error", "End date must be after start date", "OK");
            return;
        }

        // Generate report with mock data
        GenerateAttendanceReport();
    }

    private void GenerateAttendanceReport()
    {
        // Mock attendance data - in real implementation, this would query from database
        var random = new Random();
        int totalStudents = random.Next(20, 40);
        double averageAttendance = random.Next(70, 95);

        // Generate at-risk students
        _atRiskStudents = new List<AtRiskStudent>();
        int atRiskCount = random.Next(2, 6);

        for (int i = 0; i < atRiskCount; i++)
        {
            var attendance = random.Next(40, 74);
            var consecutiveAbsences = random.Next(0, 6);
            string reason = "";

            if (attendance < 75 && consecutiveAbsences >= 3)
            {
                reason = $"Low attendance ({attendance}%) and {consecutiveAbsences} consecutive absences";
            }
            else if (attendance < 75)
            {
                reason = $"Low attendance rate";
            }
            else if (consecutiveAbsences >= 3)
            {
                reason = $"{consecutiveAbsences} consecutive absences";
            }

            _atRiskStudents.Add(new AtRiskStudent
            {
                StudentId = i + 1,
                StudentName = $"Student {i + 1}",
                AttendancePercentage = attendance,
                ConsecutiveAbsences = consecutiveAbsences,
                RiskReason = reason
            });
        }

        // Update UI
        TotalStudentsLabel.Text = totalStudents.ToString();
        AverageAttendanceLabel.Text = $"{averageAttendance:F1}%";
        AtRiskStudentsLabel.Text = atRiskCount.ToString();
        AtRiskStudentsCollection.ItemsSource = _atRiskStudents;
    }

    private async void OnProvideSupportClicked(object sender, EventArgs e)
    {
        if (_atRiskStudents.Count == 0)
        {
            await DisplayAlert("Info", "No at-risk students to support", "OK");
            return;
        }

        var studentNames = string.Join("\n", _atRiskStudents.Select(s => $"- {s.StudentName} ({s.AttendancePercentage}%)"));

        bool result = await DisplayAlert(
            "Academic Support",
            $"Schedule early checkups for the following students?\n\n{studentNames}",
            "Schedule",
            "Cancel"
        );

        if (result)
        {
            await DisplayAlert(
                "Success",
                "Academic support sessions have been scheduled. Students will be notified.",
                "OK"
            );
        }
    }
}

// Model classes
public class CourseInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AtRiskStudent
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int AttendancePercentage { get; set; }
    public int ConsecutiveAbsences { get; set; }
    public string RiskReason { get; set; } = string.Empty;
}
