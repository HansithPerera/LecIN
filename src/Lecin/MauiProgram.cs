using System.Reflection;
using CommunityToolkit.Maui;
using Lecin.PageModels.Admin;
using Lecin.PageModels.Teacher;
using Lecin.Pages.Teacher;
using Lecin.Resources.Fonts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Supabase;
using SupabaseShared.Models;
using Syncfusion.Maui.Toolkit.Hosting;

namespace Lecin;

public static class MauiProgram
{
    private static void AddJsonSettings(this MauiAppBuilder builder, string jsonFile)
    {
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"{nameof(Lecin)}.{jsonFile}");

        if (stream != null)
            builder.Configuration.AddJsonFile(jsonFile);
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
            .UseMauiCommunityToolkit()
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
        builder.Services.AddSingleton<ModalErrorHandler>();
        builder.Services.AddTransient<MainPageModel>();
        builder.Services.AddTransient<AdminPageModel>();

        builder.Services.AddTransientWithShellRoute<LoginPage, LoginPageModel>("login");
        builder.Services.AddTransientWithShellRoute<TeacherCourseListPage, TeacherCourseListPageModel>("teacher/courses");
        builder.Services.AddTransientWithShellRoute<TeacherCourseViewPage, TeacherCourseViewPageModel>("teachers/course");
        builder.Services.AddTransientWithShellRoute<TeacherClassViewPage, TeacherClassViewPageModel>("teachers/class");

        return builder.Build();
    }
}