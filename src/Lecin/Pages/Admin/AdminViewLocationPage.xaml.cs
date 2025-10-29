using Lecin.PageModels.Admin;

namespace Lecin.Pages.Admin;

public partial class AdminViewLocationPage : BaseContentPage
{
    public AdminViewLocationPage(AdminViewLocationPageModel vm) : base(vm)
    {
        BindingContext = vm;
        InitializeComponent();
    }
}