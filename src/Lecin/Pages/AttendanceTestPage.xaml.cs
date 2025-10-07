// Pages/AttendanceTestPage.xaml.cs - UPDATED VERSION
using Lecin.Models;
using Lecin.Services;

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
                StudentId = Guid.NewGuid(),
                ClassesAttended = 8,
                TotalClassesEnrolled = 10
            };
            
            DisplayResults(stats);
            ShowSuccessAlert("80% Test Passed! ✅", stats);
        }

        private void OnTest0PercentClicked(object sender, EventArgs e)
        {
            // Test case: 0 out of 10 classes attended (0%)
            var stats = new AttendanceStats
            {
                StudentId = Guid.NewGuid(),
                ClassesAttended = 0,
                TotalClassesEnrolled = 10
            };
            
            DisplayResults(stats);
            ShowSuccessAlert("0% Test Passed! ✅", stats);
        }

        private void OnTest100PercentClicked(object sender, EventArgs e)
        {
            // Test case: 5 out of 5 classes attended (100%)
            var stats = new AttendanceStats
            {
                StudentId = Guid.NewGuid(),
                ClassesAttended = 5,
                TotalClassesEnrolled = 5
            };
            
            DisplayResults(stats);
            ShowSuccessAlert("100% Test Passed! ✅", stats);
        }

        // NEW: Test with real Supabase data
        private async void OnTestRealDataClicked(object sender, EventArgs e)
        {
            try
            {
                // For now, let's manually create the service without DI
                // TODO: Replace with your actual Supabase URL and key
                var options = new Supabase.SupabaseOptions { AutoConnectRealtime = true };
                var supabaseClient = new Supabase.Client("https://maxaduxsenfrbgomfhpi.supabase.co", "sb_publishable_EsnRhHv2PDx4bRhGqIloIQ_Ez7Fm1sS", options);
                
                var attendanceService = new AttendanceService(supabaseClient);
                
                var testStudentId = Guid.Parse("07ab83f9-d6af-4146-939c-80221eb9d9d1");
                
                DisplayAlert("Loading", "Connecting to Supabase...", "OK");
                
                var realStats = await attendanceService.GetStudentAttendanceStatsAsync(testStudentId);
                
                DisplayResults(realStats);
                ShowSuccessAlert("Real Data Test! 🚀", realStats);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to connect to Supabase: {ex.Message}", "OK");
            }
        }

        private void DisplayResults(AttendanceStats stats)
        {
            // Update the UI labels with the calculated results
            StudentIdLabel.Text = stats.StudentId.ToString();
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