namespace SnapLabel;

public static class MauiProgram {
    public static MauiApp CreateMauiApp() {
        SyncfusionLicenseProvider.RegisterLicense(AppConstants.SYNCFUSION);
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
            .UseFFImageLoading()
            .UseShiny()
            .ConfigureSyncfusionCore().ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "Mat");
            }).UseMauiCommunityToolkit();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        var options = new SupabaseOptions {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
        };
        builder.Services.AddSingleton<AppShell>();

        // Pages + ViewModels

        builder.Services.AddTransient<NewProductPage>();
        builder.Services.AddTransient<NewProductPageViewModel>();

        builder.Services.AddSingleton<DrawingPage>();
        builder.Services.AddTransient<DrawingPageViewModel>();

        builder.Services.AddSingleton<InventoryPage>();
        builder.Services.AddSingleton<InventoryPageViewModel>();

        builder.Services.AddSingleton<NoInternetPage>();

        builder.Services.AddSingleton<AuthenticationPage>();
        builder.Services.AddSingleton<AuthenticationPageViewModel>();

        builder.Services.AddSingleton<StoresPage>();
        builder.Services.AddSingleton<StoresViewModel>();

        builder.Services.AddSingleton<DashboardPage>();
        builder.Services.AddSingleton<DashboardPageViewModel>();

        // Messenger (MVVM Toolkit)

        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        // Other services
        builder.Services.AddSingleton(SecureStorage.Default);
        builder.Services.AddSingleton(MediaPicker.Default);
        builder.Services.AddSingleton<IShellService, ShellService>();
        builder.Services.AddSingleton<ICustomDialogService, CustomDialogService>();
        builder.Services.AddSingleton(typeof(IDatabaseService<>), typeof(DatabaseService<>));
        builder.Services.AddSingleton(Connectivity.Current);
        builder.Services.AddSingleton(provider => new Client(
            AppConstants.SUPABASE_URL, AppConstants.SUPABASE_APIKEY, options));

        builder.Services.AddBluetoothLE();
        return builder.Build();
    }
}