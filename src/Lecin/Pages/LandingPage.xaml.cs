namespace Lecin.Pages;

public partial class LandingPage : BaseContentPage
{
    public LandingPage(LandingPageModel vm) : base(vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}