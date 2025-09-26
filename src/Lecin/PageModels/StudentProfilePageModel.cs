using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Lecin.Models;
using Lecin.Services;

namespace Lecin.PageModels
{
    public class StudentProfilePageModel : INotifyPropertyChanged
    {
        private readonly IAttendanceService _attendanceService;
        
        // Properties for data binding
        private AttendanceStats _attendanceStats = new();
        private Student _selectedStudent = new();
        private bool _isLoading = false;
        private string _errorMessage = string.Empty;
        private List<Student> _allStudents = new();

        public StudentProfilePageModel(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
            
            // Commands
            LoadAttendanceCommand = new Command<string>(async (studentId) => await LoadAttendanceData(studentId));
            RefreshCommand = new Command(async () => await RefreshData());
            LoadAllStudentsCommand = new Command(async () => await LoadAllStudents());
        }

        // Properties for UI binding
        public AttendanceStats AttendanceStats
        {
            get => _attendanceStats;
            set
            {
                _attendanceStats = value;
                OnPropertyChanged();
            }
        }

        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                OnPropertyChanged();
                
                // Auto-load attendance when student is selected
                if (!string.IsNullOrEmpty(value?.StudentId))
                {
                    LoadAttendanceCommand.Execute(value.StudentId);
                }
            }
        }

        public List<Student> AllStudents
        {
            get => _allStudents;
            set
            {
                _allStudents = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand LoadAttendanceCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadAllStudentsCommand { get; }

        // Methods
        private async Task LoadAttendanceData(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                ErrorMessage = "Please select a student";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Get attendance stats from Supabase
                AttendanceStats = await _attendanceService.GetStudentAttendanceStatsAsync(studentId);
                
                if (AttendanceStats.TotalClassesEnrolled == 0)
                {
                    ErrorMessage = "No enrollment data found for this student";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading attendance data: {ex.Message}";
                AttendanceStats = new AttendanceStats { StudentId = studentId };
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshData()
        {
            if (!string.IsNullOrEmpty(SelectedStudent.StudentId))
            {
                await LoadAttendanceData(SelectedStudent.StudentId);
            }
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

        // Property changed notification
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}