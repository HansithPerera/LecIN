using System.Diagnostics;
using Lecin.Shells;
using Lecin.ViewModels;

namespace Lecin;

public partial class App : Application
{
    private readonly AppShellViewModel _vm;

    public App(AppShellViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            var exception = args.ExceptionObject as Exception;
            Debug.WriteLine($"Unhandled Exception: {exception?.Message}");
        };
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell(_vm));
    }
}