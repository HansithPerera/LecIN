using System.Reflection;
using CommunityToolkit.Maui;
using Lecin.PageModels.Admin;
using Lecin.PageModels.Student;
using Lecin.PageModels.Teacher;
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
        using var stream = asm.GetManifestResourceStream($"{asm.GetName().Name}.{jsonFile}");
        if (stream != null)
            builder.Configuration.AddJsonStream(stream);
    }

    private static void AddAppSettings(this MauiAppBuilder builder)
    {
        builder.AddJsonSettings("appsettings.json");
#if DEBUG
#if ANDROID
        builder.AddJsonSettings("appsettings.android.Development.json");
#elif WINDOWS
        builder.AddJsonSettings("appsettings.Development.json");
#endif
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
            .UseMauiCommunityToolkitCamera()
            .UseMauiCommunityToolkit(options => options.SetShouldEnableSnackbarOnWindows(true))
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
                fonts.AddFont("MauiMaterialAssets.ttf", "MaterialAssets");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
            });

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        builder.Services.AddSingleton<Client>(sp =>
            new Client(
                builder.Configuration["Supabase:Url"]!,
                builder.Configuration["Supabase:Key"]!
            )
        );

        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<ModalErrorHandler>();

        builder.Services.AddTransient<LoginPageModel>();
        builder.Services.AddTransient<LandingPageModel>();
        builder.Services.AddTransient<AppShellViewModel>();
        builder.Services.AddTransient<CheckInPageModel>();
        builder.Services.AddTransient<CheckInPage>();

        builder.Services.AddTransient<TeacherCourseListPageModel>();
        builder.Services.AddTransient<StudentCourseViewPageModel>();
        builder.Services.AddTransient<TeacherClassViewPageModel>();
        builder.Services.AddTransient<TeacherDashboardPageModel>();
        builder.Services.AddTransient<TeacherCourseViewPageModel>();

        builder.Services.AddTransient<AttendanceHistoryPageModel>();
        builder.Services.AddTransient<AttendanceStreaksPageModel>();
        builder.Services.AddTransient<StudentDashboardPageModel>();
        builder.Services.AddTransient<StudentCourseViewPageModel>();
        builder.Services.AddTransient<StudentViewProfilePageModel>();
        builder.Services.AddTransient<StudentRegisterFacePageModel>();
        builder.Services.AddTransient<ClassmatesPageModel>();

        builder.Services.AddTransient<AdminPageModel>();
        builder.Services.AddTransient<AdminListLocationPageModel>();
        builder.Services.AddTransient<AdminViewLocationPageModel>();
        builder.Services.AddTransient<AdminViewCameraPageModel>();
        builder.Services.AddTransient<AdminListCameraPageModel>();

        return builder.Build();
    }
}