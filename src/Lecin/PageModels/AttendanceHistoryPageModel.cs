using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.ObjectModel;
using Lecin.Models;
using Lecin.Services;

namespace Lecin.PageModels
{
    public class AttendanceHistoryPageModel : INotifyPropertyChanged
    {
        private readonly IAttendanceHistoryService _attendanceHistoryService;
        
        private ObservableCollection<AttendanceHistoryItem> _attendanceHistory = new();
        private bool _isLoading = false;
        private bool _isRefreshing = false;
        private string _errorMessage = string.Empty;
        private string _currentStudentId = string.Empty;

        public AttendanceHistoryPageModel(IAttendanceHistoryService attendanceHistoryService)
        {
            _attendanceHistoryService = attendanceHistoryService;
            
            // Commands
            LoadAttendanceHistoryCommand = new Command<string>(async (studentId) => await LoadAttendanceHistory(studentId));
            RefreshCommand = new Command(async () => await RefreshAttendanceHistory());
            ViewDetailCommand = new Command<AttendanceHistoryItem>(async (item) => await ViewAttendanceDetail(item));
            LoadTestDataCommand = new Command(async () => await LoadTestData());
        }

        // Properties
        public ObservableCollection<AttendanceHistoryItem> AttendanceHistory
        {
            get => _attendanceHistory;
            set
            {
                _attendanceHistory = value;
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

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
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
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public string CurrentStudentId
        {
            get => _currentStudentId;
            set
            {
                _currentStudentId = value;
                OnPropertyChanged();
            }
        }

        // Commands
        public ICommand LoadAttendanceHistoryCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ViewDetailCommand { get; }
        public ICommand LoadTestDataCommand { get; }

        // Methods
        private async Task LoadAttendanceHistory(string studentId)
        {
            if (string.IsNullOrEmpty(studentId))
            {
                ErrorMessage = "Student ID is required";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                CurrentStudentId = studentId;

                var historyItems = await _attendanceHistoryService.GetStudentAttendanceHistoryAsync(studentId);
                
                AttendanceHistory.Clear();
                foreach (var item in historyItems)
                {
                    AttendanceHistory.Add(item);
                }

                if (!AttendanceHistory.Any())
                {
                    ErrorMessage = "No attendance records found for this student";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading attendance history: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTestData()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Load test data to demonstrate the feature
                var testHistoryItems = await _attendanceHistoryService.GetTestAttendanceHistoryAsync();
                
                AttendanceHistory.Clear();
                foreach (var item in testHistoryItems)
                {
                    AttendanceHistory.Add(item);
                }

                CurrentStudentId = "test-student-1";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading test data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshAttendanceHistory()
        {
            if (string.IsNullOrEmpty(CurrentStudentId))
                return;

            try
            {
                IsRefreshing = true;
                ErrorMessage = string.Empty;

                var historyItems = await _attendanceHistoryService.GetStudentAttendanceHistoryAsync(CurrentStudentId);
                
                AttendanceHistory.Clear();
                foreach (var item in historyItems)
                {
                    AttendanceHistory.Add(item);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error refreshing attendance history: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task ViewAttendanceDetail(AttendanceHistoryItem item)
        {
            try
            {
                var detailItem = await _attendanceHistoryService.GetAttendanceDetailAsync(
                    item.StudentId, 
                    item.ClassId, 
                    item.Date);

                if (detailItem != null)
                {
                    // Show detailed information in an alert (for now)
                    string message = $"Course: {detailItem.CourseName}\n" +
                                   $"Date: {detailItem.DisplayDate}\n" +
                                   $"Time: {detailItem.ClassTime}\n" +
                                   $"Status: {detailItem.StatusText}\n" +
                                   $"Notes: {detailItem.Notes}";

                    if (Application.Current?.Windows?.FirstOrDefault()?.Page != null)
                    {
                        await Application.Current.Windows.First().Page.DisplayAlert(
                            "Attendance Details", 
                            message, 
                            "OK");
                    }
                }
                else
                {
                    // For test data, show simplified details
                    string message = $"Date: {item.DisplayDate}\n" +
                                   $"Time: {item.Time}\n" +
                                   $"Status: {item.StatusText}\n" +
                                   $"Notes: {item.RawReason}";

                    if (Application.Current?.Windows?.FirstOrDefault()?.Page != null)
                    {
                        await Application.Current.Windows.First().Page.DisplayAlert(
                            "Attendance Details", 
                            message, 
                            "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.Windows?.FirstOrDefault()?.Page != null)
                {
                    await Application.Current.Windows.First().Page.DisplayAlert(
                        "Error", 
                        $"Error loading attendance details: {ex.Message}", 
                        "OK");
                }
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