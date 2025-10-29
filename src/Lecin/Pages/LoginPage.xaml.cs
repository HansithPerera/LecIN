namespace Lecin.Pages;

public partial class LoginPage
{
    public LoginPage(LoginPageModel vm) : base(vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}