using Lecin.PageModels;

namespace Lecin.Pages
{
    public partial class AttendanceHistoryPage : ContentPage
    {
        public AttendanceHistoryPageModel ViewModel { get; }

        public AttendanceHistoryPage(AttendanceHistoryPageModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            if (ViewModel.AttendanceHistory.Count == 0)
            {
                // Try to get the real logged-in student ID first
                string? loggedInStudentId = await GetLoggedInStudentId();
                
                if (!string.IsNullOrEmpty(loggedInStudentId))
                {
                    // Use real student ID
                    ViewModel.LoadAttendanceHistoryCommand.Execute(loggedInStudentId);
                }
                else
                {
                    // Fallback to test data for demonstration
                    ViewModel.LoadTestDataCommand.Execute(null);
                }
            }
        }

        private async Task<string?> GetLoggedInStudentId()
        {
            try
            {
                // Get the student email from secure storage
                var studentEmail = await SecureStorage.GetAsync("student_email");
                
                if (studentEmail == "student@student.com")
                {
                    // TODO: Replace this with actual student GUID from your database
                    // You need to query your Students table to get the real GUID
                    return "replace-with-actual-student-guid";
                }
                
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting logged-in student ID: {ex.Message}");
                return null;
            }
        }
    }
}