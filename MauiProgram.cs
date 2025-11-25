using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Firebase.Database;

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
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddHttpClient("Firestore", client => {
            client.BaseAddress = new Uri("https://firestore.googleapis.com/v1/");
        });


        // Pages + ViewModels

        builder.Services.AddTransient<NewProductPage>();
        builder.Services.AddTransient<NewProductPageViewModel>();

        builder.Services.AddTransient<DrawingPage>();
        builder.Services.AddTransient<DrawingPageViewModel>();

        builder.Services.AddTransient<InventoryPage>();
        builder.Services.AddTransient<InventoryPageViewModel>();

        builder.Services.AddTransient<NoInternetPage>();

        builder.Services.AddTransient<AuthenticationPage>();
        builder.Services.AddTransient<AuthenticationPageViewModel>();

        builder.Services.AddTransient<StoresPage>();
        builder.Services.AddTransient<StoresPageViewModel>();

        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<DashboardPageViewModel>();


        // Messenger (MVVM Toolkit)

        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        // Other services
        builder.Services.AddSingleton(SecureStorage.Default);
        builder.Services.AddSingleton(MediaPicker.Default);
        builder.Services.AddSingleton<IShellService, ShellService>();
        builder.Services.AddSingleton<ICustomDialogService, CustomDialogService>();
        builder.Services.AddSingleton(typeof(IDatabaseService<>), typeof(DatabaseService<>));
        builder.Services.AddBluetoothLE();
        builder.Services.AddSingleton(Connectivity.Current);

        builder.Services.AddSingleton<IFirebaseAuthClient>(new FirebaseAuthClient(new FirebaseAuthConfig {

            ApiKey = "AIzaSyC3zObE0sDsLJCFwUjlznXv6r5se3Wri-E",
            AuthDomain = "snaplabel-88b46.firebaseapp.com",
            Providers = [
                new EmailProvider()
            ],
            UserRepository = new FileUserRepository(AppConstants.UserData)
        }));


        builder.Services.AddSingleton(provider => {
            return new FirebaseClient("https://snaplabel-88b46-default-rtdb.europe-west1.firebasedatabase.app/");
        });



        return builder.Build();
    }
}