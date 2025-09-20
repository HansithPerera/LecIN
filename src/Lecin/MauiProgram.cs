using System.Reflection;
using Backend.Api;
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
        using var devStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"{nameof(Lecin)}.appsettings.dev.json");
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"{nameof(Lecin)}.appsettings.json");
        
        IConfiguration config;
        if (devStream != null)
        {
            config = new ConfigurationBuilder()
                .AddJsonStream(stream!)
                .AddJsonStream(devStream)
                .Build();
        }
        else
        {
            config = new ConfigurationBuilder()
                .AddJsonStream(stream!)
                .Build();
        }
        
        builder.Configuration.AddConfiguration(config);
        
        builder.Services.AddSingleton<Supabase.Client>(sp =>
            new Supabase.Client(
                builder.Configuration["Supabase:Url"] ?? throw new InvalidOperationException("Supabase URL is not configured."),
                builder.Configuration["Supabase:Key"] ?? throw new InvalidOperationException("Supabase Key is not configured.")
            )
        );
        builder.Services.AddTransient<OpenApiClient>();
        builder.Services.AddTransient<AuthHandler>();
        builder.Services.AddHttpClient<OpenApiClient>(client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["Api:Url"] ?? throw new InvalidOperationException("API URL is not configured."));
        }).AddHttpMessageHandler<AuthHandler>();
        
        builder.Services.AddSingleton<ModalErrorHandler>();
        builder.Services.AddSingleton<MainPageModel>();

        builder.Services.AddTransientWithShellRoute<LoginPage, LoginPageModel>("login");

        return builder.Build();
    }
}