using FFImageLoading.Maui;

using Shiny;

using Supabase;



namespace SnapLabel;

public static class MauiProgram {

    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseFFImageLoading()
            .UseShiny()
            .ConfigureSyncfusionCore()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "Mat");
            })
            .UseMauiCommunityToolkit();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var options = new SupabaseOptions {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
        };

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<NewProductPage, NewProductPageViewModel>();
        builder.Services.AddSingleton<InventoryPage, InventoryPageViewModel>();
        builder.Services.AddSingleton(MediaPicker.Default);
        builder.Services.AddSingleton<IShellService, ShellService>();
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton(provider =>
        new Client(Constants.Constants.SUPABASE_URL,
            Constants.Constants.SUPABASE_APIKEY, options));
        builder.Services.AddBluetoothLE();

        return builder.Build();
    }
}
