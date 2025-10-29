using Lecin.PageModels.Admin;

namespace Lecin.Pages.Admin;

public partial class AdminListLocationPage : BaseContentPage
{
    public AdminListLocationPage(AdminListLocationPageModel pageModel) : base(pageModel)
    {
        BindingContext = pageModel;
        InitializeComponent();
    }
}