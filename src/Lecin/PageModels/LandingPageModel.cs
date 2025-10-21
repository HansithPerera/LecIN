using CommunityToolkit.Mvvm.Messaging;
using Lecin.Messaging;

namespace Lecin.PageModels;

public partial class LandingPageModel(AuthService auth) : BasePageModel
{
    public override async Task LoadDataAsync()
    {
        var session = await auth.RestoreSession();
        if (session.HasValue)
            WeakReferenceMessenger.Default.Send(new LoggedInMessage { UserType = session.Value });
        else
            WeakReferenceMessenger.Default.Send(new LoggedOutMessage());
    }
}