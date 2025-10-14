using Newtonsoft.Json;
using ResultSharp;
using Supabase.Gotrue;
using Supabase.Gotrue.Exceptions;
using SupabaseShared.Models;
using Client = Supabase.Client;

namespace Lecin.Services;

public class SavedSession
{
    public required UserType UserType { get; set; }
    public required Session Session { get; set; }
}

public enum SignInError
{
    InvalidCredentials,
    InvalidUserType,
    Error
}

public class LoggedInEventArgs : EventArgs
{
    public required UserType UserType { get; set; }
}

public class AuthService(Client supabase)
{
    private const string SessionKey = "user_session";

    private bool _loggedIn;

    public UserType? CurrentUserType { get; private set; }

    public event EventHandler<LoggedInEventArgs>? LoggedIn;

    public event EventHandler? LoggedOut;

    public async Task<Result<UserType, SignInError>> SignIn(string email, string password, UserType userType)
    {
        try
        {
            var session = await supabase.Auth.SignInWithPassword(email, password);
            if (!Guid.TryParse(session?.User?.Id, out var userId))
                return Result.Err<UserType, SignInError>(SignInError.InvalidCredentials);

            var success = userType switch
            {
                UserType.Admin =>
                    await supabase.From<Admin>()
                        .Where(a => a.Id == userId)
                        .Single() != null,
                UserType.Teacher =>
                    await supabase.From<Teacher>()
                        .Where(t => t.Id == userId)
                        .Single() != null,
                UserType.Student =>
                    await supabase.From<Student>()
                        .Where(s => s.Id == userId)
                        .Single() != null,
                _ => false
            };

            if (!success)
            {
                await supabase.Auth.SignOut();
                return Result.Err<UserType, SignInError>(SignInError.InvalidUserType);
            }

            CurrentUserType = userType;
            _loggedIn = true;
            await SaveSession();
            LoggedIn?.Invoke(this, new LoggedInEventArgs { UserType = userType });
            return Result.Ok<UserType, SignInError>(userType);
        }
        catch (GotrueException ex) when (ex.StatusCode == 400)
        {
            return Result.Err<UserType, SignInError>(SignInError.InvalidCredentials);
        }
        catch (Exception e)
        {
            return Result.Err<UserType, SignInError>(SignInError.Error);
        }
    }

    public async Task SignOut()
    {
        await supabase.Auth.SignOut();
        await ClearSession();
        CurrentUserType = null;
        _loggedIn = false;
        LoggedOut?.Invoke(this, EventArgs.Empty);
    }

    public async Task SaveSession()
    {
        var session = supabase.Auth.CurrentSession;
        if (session != null && CurrentUserType != null)
        {
            var savedSession = new SavedSession
            {
                UserType = CurrentUserType.Value,
                Session = session
            };
            var sessionJson = JsonConvert.SerializeObject(savedSession);
            await SecureStorage.SetAsync(SessionKey, sessionJson);
        }
    }

    public async Task<UserType?> RestoreSession()
    {
        try
        {
            var sessionJson = await SecureStorage.GetAsync(SessionKey);
            if (string.IsNullOrEmpty(sessionJson))
                return null;
            var saved = JsonConvert.DeserializeObject<SavedSession>(sessionJson);
            if (saved == null)
                return null;
            await supabase.Auth.SetSession(saved.Session.AccessToken, saved.Session.RefreshToken, true);
            _loggedIn = true;
            CurrentUserType = saved.UserType;
            LoggedIn?.Invoke(this, new LoggedInEventArgs { UserType = saved.UserType });
            return saved.UserType;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error restoring session: {ex.Message}");
            return null;
        }
    }

    public async Task ClearSession()
    {
        SecureStorage.Remove(SessionKey);
    }
}