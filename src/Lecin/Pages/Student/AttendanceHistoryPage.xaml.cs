using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;

namespace Lecin.Pages.Student;

public partial class AttendanceHistoryPage : ContentPage
{
    public AttendanceHistoryPage()
    {
        InitializeComponent();

        // Fake data for testing
        AttendanceList.ItemsSource = new List<string>
        {
            "2025-09-20 - Present",
            "2025-09-21 - Absent",
            "2025-09-22 - Present"
        };
    }
}
