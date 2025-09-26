using Lecin.Models;

namespace Lecin.Pages
{
    public partial class AttendanceTestPage : ContentPage
    {
        public AttendanceTestPage()
        {
            InitializeComponent();
        }

        private void OnTest80PercentClicked(object sender, EventArgs e)
        {
            // Test case: 8 out of 10 classes attended (80%)
            var stats = new AttendanceStats
            {
                StudentId = "student_80_test",
                ClassesAttended = 8,
                TotalClassesEnrolled = 10
            };
            
            DisplayResults(stats);
            ShowSuccessAlert("80% Test Passed! ✓", stats);
        }

        private void OnTest0PercentClicked(object sender, EventArgs e)
        {
            // Test case: 0 out of 10 classes attended (0%)
            var stats = new AttendanceStats
            {
                StudentId = "student_0_test",
                ClassesAttended = 0,
                TotalClassesEnrolled = 10
            };
            
            DisplayResults(stats);
            ShowSuccessAlert("0% Test Passed! ✓", stats);
        }

        private void OnTest100PercentClicked(object sender, EventArgs e)
        {
            // Test case: 5 out of 5 classes attended (100%)
            var stats = new AttendanceStats
            {
                StudentId = "student_100_test",
                ClassesAttended = 5,
                TotalClassesEnrolled = 5
            };
            
            DisplayResults(stats);
            ShowSuccessAlert("100% Test Passed! ✓", stats);
        }

        private void DisplayResults(AttendanceStats stats)
        {
            // Update the UI labels with the calculated results
            StudentIdLabel.Text = stats.StudentId;
            AttendedLabel.Text = stats.ClassesAttended.ToString();
            TotalLabel.Text = stats.TotalClassesEnrolled.ToString();
            PercentageLabel.Text = stats.AttendancePercentageDisplay;
            StatusLabel.Text = stats.AttendanceStatus;
            
            // Apply color based on percentage
            PercentageLabel.TextColor = stats.StatusColor;
            StatusLabel.TextColor = stats.StatusColor;
        }

        private async void ShowSuccessAlert(string title, AttendanceStats stats)
        {
            string message = $"Student: {stats.StudentId}\n" +
                           $"Attended: {stats.ClassesAttended}/{stats.TotalClassesEnrolled} classes\n" +
                           $"Percentage: {stats.AttendancePercentageDisplay}\n" +
                           $"Status: {stats.AttendanceStatus}";
            
            await DisplayAlert(title, message, "Great!");
        }
    }
}