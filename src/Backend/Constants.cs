using Backend.Auth;
using Backend.Models;

namespace Backend;

public static class Constants
{
    #region Supabase

    public const string SupabaseAuthenticatedRole = "authenticated";

    #endregion

    #region Authentication

    public const string ApiKeyAuthScheme = "ApiKey";

    public const string JwtAuthScheme = "Bearer";

    public const string ApiKeyHeaderName = "X-API-KEY";

    #endregion

    #region Environments

    public const string DevelopmentEnv = "Dev";

    public const string ProductionEnv = "Prod";

    public const string TestingEnv = "Testing";

    #endregion

    #region Policies

    public const string AdminAuthorizationPolicy = nameof(UserType.Admin);
    public const string TeacherAuthorizationPolicy = nameof(UserType.Teacher);
    public const string StudentAuthorizationPolicy = nameof(UserType.Student);
    public const string CameraAuthorizationPolicy = nameof(IntegrationType.Camera);

    #region Admin Permissions

    public const string AdminExportDataPermission = nameof(AdminPermissions.ExportData);
    public const string AdminManageCoursesPermission = nameof(AdminPermissions.ManageCourses);
    public const string AdminManageReportsPermission = nameof(AdminPermissions.ManageReports);
    public const string AdminManageTeachersPermission = nameof(AdminPermissions.ManageTeachers);
    public const string AdminManageStudentsPermission = nameof(AdminPermissions.ManageStudents);

    public const string AdminReadTeachersPermission = nameof(AdminPermissions.ReadTeachers);
    public const string AdminReadStudentsPermission = nameof(AdminPermissions.ReadStudents);
    public const string AdminReadCoursesPermission = nameof(AdminPermissions.ReadCourses);
    public const string AdminReadReportsPermission = nameof(AdminPermissions.ReadReports);

    #endregion

    #endregion
}