using Backend.Auth;

namespace Backend;

public static class Constants
{
    #region Supabase

    public const string SupabaseAuthenticatedRole = "authenticated";

    #endregion

    public const string CameraAuthorizationPolicy = $"{nameof(IntegrationType.Camera)}Policy";

    #region Authentication

    public const string ApiKeyAuthScheme = "ApiKey";

    public const string ApiKeyHeaderName = "X-API-KEY";

    #endregion

    #region Environments

    public const string DevelopmentEnv = "Dev";

    public const string ProductionEnv = "Prod";

    public const string TestingEnv = "Testing";

    #endregion
}