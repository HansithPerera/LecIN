using System;
using Microsoft.Maui.Controls;

namespace Lecin.Views
{
    public partial class TeacherProfilePage : ContentPage
    {
        public Teacher TeacherDetails { get; set; }

        public TeacherProfilePage(Teacher teacher)
        {
            InitializeComponent();
            TeacherDetails = teacher;
            BindingContext = TeacherDetails;
        }
    }

    public class Teacher
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public string OfficeLocation { get; set; }
    }
}
