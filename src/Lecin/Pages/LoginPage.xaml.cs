using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lecin.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageModel loginPageModel)
    {
        InitializeComponent();
        BindingContext = loginPageModel;
    }
    private async void OnAttendanceTestClicked(object sender, EventArgs e)
{
    await Navigation.PushAsync(new Lecin.Pages.AttendanceTestPage());
}
}

