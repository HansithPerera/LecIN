using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;

namespace Lecin.Pages.Student;

public partial class AttendanceStreaksPage : ContentPage
{
    public AttendanceStreaksPage()
    {
        InitializeComponent();

        // Fake streak value for testing
        StreakLabel.Text = "Current Streak: 5 days";
    }
}
