// Pages/StudentProfilePage.xaml.cs
using Lecin.PageModels;

namespace Lecin.Pages
{
    public partial class StudentProfilePage : ContentPage
    {
        public StudentProfilePageModel ViewModel { get; }

        public StudentProfilePage(StudentProfilePageModel viewModel)
        {
            InitializeComponent();
            
            ViewModel = viewModel;
            BindingContext = ViewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Auto-load students when the page appears
            if (ViewModel.AllStudents.Count == 0)
            {
                ViewModel.LoadAllStudentsCommand.Execute(null);
            }
        }
    }
}