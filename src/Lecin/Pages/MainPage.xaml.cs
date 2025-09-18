using Lecin.Models;
using Lecin.PageModels;

namespace Lecin.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(MainPageModel model)
    {
        InitializeComponent();
        BindingContext = model;
    }
}