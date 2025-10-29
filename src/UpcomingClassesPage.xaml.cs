using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Lecin.Views
{
    public partial class UpcomingClassesPage : ContentPage
    {
        public ObservableCollection<ClassInfo> UpcomingClasses { get; set; }

        public UpcomingClassesPage()
        {
            InitializeComponent();
            UpcomingClasses = new ObservableCollection<ClassInfo>();
            BindingContext = this;

            LoadUpcomingClasses();
        }

        private async Task LoadUpcomingClasses()
        {
            try
            {
                var classes = await SupabaseService.GetUpcomingClassesAsync(); // Connects to Supabase backend
                UpcomingClasses.Clear();

                foreach (var item in classes)
                {
                    UpcomingClasses.Add(item);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Failed to load upcoming classes: " + ex.Message, "OK");
            }
        }
    }

    public class ClassInfo
    {
        public string ClassName { get; set; }
        public DateTime StartTime { get; set; }
        public string TeacherName { get; set; }

        public string DisplayTime => StartTime.ToString("ddd, dd MMM yyyy hh:mm tt");
    }
}
