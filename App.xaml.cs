using DeviceInfo = Microsoft.Maui.Devices.DeviceInfo;

namespace SnapLabel;

public partial class App : Application {

    private readonly AppShell _appShell;
    private readonly IConnectivity _connectivity;
    private readonly Client _supabase;
    private readonly NoInternetPage _noInternetPage;
    private readonly IShellService _shellService;

    public App(AppShell appShell, Client supabase, IConnectivity connectivity, NoInternetPage noInternetPage,
        IShellService shellService) {

        InitializeComponent();

        _appShell = appShell;
        _supabase = supabase;
        _connectivity = connectivity;
        _noInternetPage = noInternetPage;
        _shellService = shellService;

        // Subscribe to connectivity changes (handled on main thread)
        _connectivity.ConnectivityChanged += (_, e) =>
            MainThread.BeginInvokeOnMainThread(async () => await HandleConnectivityAsync(e));
    }

    protected override Window CreateWindow(IActivationState? activationState) {
        Window window = new(_appShell);

        if(DeviceInfo.Idiom == DeviceIdiom.Desktop) {
            // Subscribe to the window's Created event
            window.Created += OnWindowCreated;
        }
        else {
            // Mobile: start initialization immediately
            MainThread.BeginInvokeOnMainThread(async () => await InitAppAsync());
        }

        return window;
    }

    private async void OnWindowCreated(object? sender, EventArgs e) {
        if(sender is Window window)
            window.Created -= OnWindowCreated;

        // Wait until the Shell has a valid XamlRoot (ensures no flash)
        while(_appShell.Handler?.PlatformView == null)
            await Task.Delay(10);

        // Now safely initialize
        await InitAppAsync();
    }

    /// <summary>
    /// Initializes Supabase if internet is available,
    /// or shows the NoInternetPage if offline.
    /// </summary>
    private async Task InitAppAsync() {

        if(_connectivity.NetworkAccess != NetworkAccess.Internet) {

            // No internet on startup → show modal

            if(!_appShell.Navigation.ModalStack.Contains(_noInternetPage))

                await _appShell.Navigation.PushModalAsync(_noInternetPage);

            return;
        }

        await InitializeSupabaseAsync();

    }

    /// <summary>
    /// Handles when internet connectivity changes while the app is running.
    /// </summary>
    private async Task HandleConnectivityAsync(ConnectivityChangedEventArgs e) {

        IReadOnlyList<Page> modalStack = _appShell.Navigation.ModalStack;
        bool hasNoInternetModal = modalStack.OfType<NoInternetPage>().Any();

        if(e.NetworkAccess != NetworkAccess.Internet) {

            if(!hasNoInternetModal)
                await _appShell.Navigation.PushModalAsync(new NoInternetPage());
        }
        else {
            if(hasNoInternetModal)
                await _appShell.Navigation.PopModalAsync();

            await InitializeSupabaseAsync();
        }
    }

    /// <summary>
    /// Initializes Supabase connection and handles exceptions.
    /// </summary>
    private async Task InitializeSupabaseAsync() {
        if(_supabase == null)
            return;

        try {
            await _supabase.InitializeAsync();
        } catch(Exception ex) {
            await _shellService.DisplayToastAsync($"Supabase init failed: {ex.Message}");
        }
    }
}
