using Lecin.Pages.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lecin.Pages;

public partial class StudentDashboardPage : ContentPage
{
    public StudentDashboardPage()
    {
        InitializeComponent();
    }

    private async void OnHistoryClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AttendanceHistoryPage));
    }

    private async void OnStreaksClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AttendanceStreaksPage));
    }
}

