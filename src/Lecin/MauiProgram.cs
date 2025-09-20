using System.Reflection;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

namespace Lecin;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
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
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"{nameof(Lecin)}.appsettings.json");
        
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream!)
            .Build();
        
        builder.Configuration.AddConfiguration(config);
        
        builder.Services.AddSingleton<Supabase.Client>(sp =>
            new Supabase.Client(
                builder.Configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL is not configured."),
                builder.Configuration["Supabase:Key"] ?? throw new InvalidOperationException("Supabase Key is not configured.")
            )
        );
        builder.Services.AddSingleton<ModalErrorHandler>();
        builder.Services.AddSingleton<MainPageModel>();

        builder.Services.AddTransientWithShellRoute<LoginPage, LoginPageModel>("login");

        return builder.Build();
    }
}