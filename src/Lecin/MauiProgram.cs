using System.Reflection;
using CommunityToolkit.Maui;
using Lecin.PageModels.Admin;
using Lecin.PageModels.Student;
using Lecin.PageModels.Teacher;
using Lecin.Pages.Admin;
using Lecin.Pages.Student;
using Lecin.Pages.Teacher;
using Lecin.Resources.Fonts;
using Lecin.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Supabase;
using Syncfusion.Maui.Toolkit.Hosting;

namespace Lecin;

public static class MauiProgram
{
    private static void AddJsonSettings(this MauiAppBuilder builder, string jsonFile)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm
            .GetManifestResourceStream($"{asm.GetName().Name}.{jsonFile}");

        if (stream != null)
            builder.Configuration.AddJsonStream(stream);
    }

    private static void AddAppSettings(this MauiAppBuilder builder)
    {
        builder.AddJsonSettings("appsettings.json");

#if DEBUG
        builder.AddJsonSettings("appsettings.Development.json");
#else
        builder.AddJsonSettings("appsettings.Production.json");
#endif
    }

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.AddAppSettings();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit(options => options.SetShouldEnableSnackbarOnWindows(true)
            )
            .ConfigureSyncfusionToolkit()
            .ConfigureMauiHandlers(handlers =>
            {
#if IOS || MACCATALYST
				handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();
#endif
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
            });

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif
        builder.Services.AddSingleton<Client>(sp =>
            new Client(
                builder.Configuration["Supabase:Url"] ??
                throw new InvalidOperationException("Supabase URL is not configured."),
                builder.Configuration["Supabase:Key"] ??
                throw new InvalidOperationException("Supabase Key is not configured.")
            )
        );

        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ModalErrorHandler>();

        builder.Services.AddTransientWithShellRoute<LoginPage, LoginPageModel>("login");
        builder.Services.AddTransientWithShellRoute<LandingPage, LandingPageModel>("landing");
        builder.Services.AddTransient<AppShellViewModel>();

        #region Teacher

        builder.Services.AddTransientWithShellRoute<TeacherCourseListPage, TeacherCourseListPageModel>(
            "teacher/courses");
        builder.Services
            .AddTransientWithShellRoute<TeacherCourseViewPage, TeacherCourseViewPageModel>("teacher/course");
        builder.Services.AddTransientWithShellRoute<TeacherClassViewPage, TeacherClassViewPageModel>("teacher/class");
        builder.Services.AddTransientWithShellRoute<TeacherDashboardPage, TeacherDashboardPageModel>(
            "teacher/dashboard");

        #endregion

        #region Student

        builder.Services.AddTransientWithShellRoute<AttendanceHistoryPage, AttendanceHistoryPageModel>(
            "student/attendance/history");
        builder.Services.AddTransientWithShellRoute<AttendanceStreaksPage, AttendanceStreaksPageModel>(
            "student/attendance/streaks");
        builder.Services.AddTransientWithShellRoute<StudentDashboardPage, StudentDashboardPageModel>(
            "student/dashboard");

        #endregion

        #region Admin

        builder.Services.AddTransientWithShellRoute<AdminDashboardPage, AdminPageModel>("admin/dashboard");

        #endregion

        return builder.Build();
    }
}