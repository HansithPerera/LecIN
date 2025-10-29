using System.Diagnostics;
using Lecin.PageModels.Student;

namespace Lecin.Pages.Student;

public partial class StudentRegisterFacePage : BaseContentPage
{
    public StudentRegisterFacePage(StudentRegisterFacePageModel vm) : base(vm)
    {
        BindingContext = vm;
        vm.ImageProcessed += (_, _) =>
        {
            Dispatcher.Dispatch(async void () =>
            {
                try
                {
                    await Shell.Current.GoToAsync($"//{nameof(StudentViewProfilePage)}");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            });
        };
        InitializeComponent();
    }
}