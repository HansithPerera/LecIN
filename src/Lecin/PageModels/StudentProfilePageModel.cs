using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Lecin.Models;
using Lecin.Services;

namespace Lecin.PageModels
{
    public partial class StudentProfilePageModel : ObservableObject
    {
        private readonly IAttendanceService _attendanceService;
        
        // Properties for data binding
        [ObservableProperty]
        private AttendanceStats _attendanceStats = new();
        
        [ObservableProperty]
        private bool _isLoading = false;
        
        [ObservableProperty]
        private string _errorMessage = string.Empty;
        
        [ObservableProperty]
        private List<SupabaseShared.Models.Student> _allStudents = new();

        public StudentProfilePageModel(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
            
            // Commands
            LoadAttendanceCommand = new Command<string>(async (studentId) => await LoadAttendanceData(studentId));
            RefreshCommand = new Command(async () => await RefreshData());
            LoadAllStudentsCommand = new Command(async () => await LoadAllStudents());
        }

        [ObservableProperty]
        private SupabaseShared.Models.Student? _selectedStudent;

        // Commands
        public ICommand LoadAttendanceCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadAllStudentsCommand { get; }

        // Methods
        private async Task LoadAttendanceData(string? studentId)
        {
            if (Guid.TryParse(studentId, out var guid))
            {
                return;
            }
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Get attendance stats from Supabase
                AttendanceStats = await _attendanceService.GetStudentAttendanceStatsAsync(guid);
                
                if (AttendanceStats.TotalClassesEnrolled == 0)
                {
                    ErrorMessage = "No enrollment data found for this student";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading attendance data: {ex.Message}";
                AttendanceStats = new AttendanceStats { StudentId = guid };
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshData()
        {
            if (SelectedStudent == null)
            {
                ErrorMessage = "No student selected to refresh data.";
                return;
            }
            await LoadAttendanceData(SelectedStudent.Id.ToString());
        }

        private async Task LoadAllStudents()
        {
            try
            {
                IsLoading = true;
                AllStudents = await _attendanceService.GetAllStudentsAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading students: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}