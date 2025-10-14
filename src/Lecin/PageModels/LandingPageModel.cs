namespace Lecin.PageModels;

public class LandingPageModel(AuthService auth) : BasePageModel
{
    public event EventHandler? OnSessionRestoreFailed;

    public override async Task LoadDataAsync()
    {
        var session = await auth.RestoreSession();
        if (session == null) OnSessionRestoreFailed?.Invoke(this, EventArgs.Empty);
    }
}