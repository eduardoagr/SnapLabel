using FFImageLoading.Maui;

#if __IOS__
using SnapLabel.Platforms.iOS;
#endif

#if __ANDROID__
using SnapLabel.Platforms.Android;
#endif

#if WINDOWS
using SnapLabel.Platforms.Windows;
#endif


namespace SnapLabel;

public static class MauiProgram {

    public static MauiApp CreateMauiApp() {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseFFImageLoading()
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

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<NewProductPage, NewProductPageViewModel>();
        builder.Services.AddSingleton<InventoryPage, InventoryPageViewModel>();
        builder.Services.AddSingleton(MediaPicker.Default);
        builder.Services.AddSingleton<IShellService, ShellService>();
        builder.Services.AddSingleton<DatabaseService>();

#if WINDOWS
        builder.Services.AddSingleton<IBluetoothService, WindowsBluetoothScanner>();
#elif ANDROID
        builder.Services.AddSingleton<IBluetoothService, AndroidBluetoothScanner>();
#elif IOS || MACCATALYST
        builder.Services.AddSingleton<IBluetoothService, AppleBluetoothScanner>();
#endif
        return builder.Build();
    }
}
