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
        BindingContext = new Lecin.PageModels.Student.AttendanceHistoryPageModel();

        // Bind the CollectionView
        AttendanceList.ItemsSource = ((Lecin.PageModels.Student.AttendanceHistoryPageModel)BindingContext).Records;
    }
}
