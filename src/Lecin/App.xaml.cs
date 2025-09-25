using Supabase;

namespace Lecin;

public partial class App : Application
{
    public static Client? CurrentSupabase { get; private set; }

    public App(Client supabase)
    {
        InitializeComponent();
        CurrentSupabase = supabase;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Before login: no roles, not logged in
        return new Window(new AppShell(new List<string>(), isLoggedIn: false));
    }

}
