namespace SnapLabel;

public partial class App : Application {

    private readonly AppShell _appShell;
    private readonly IConnectivity _connectivity;

    public App(AppShell appShell, IConnectivity connectivity) {
        InitializeComponent();

        _appShell = appShell;
        _connectivity = connectivity;

        // Subscribe to connectivity changes
        _connectivity.ConnectivityChanged += (_, e) =>
            MainThread.BeginInvokeOnMainThread(async () => await HandleConnectivityAsync(e));
    }

    protected override Window CreateWindow(IActivationState? activationState) {
        var window = new Window(_appShell);

        // Run initialization after window is created
        MainThread.BeginInvokeOnMainThread(async () => await InitAppAsync());

        return window;
    }

    private async Task InitAppAsync() {

        if(_connectivity.NetworkAccess != NetworkAccess.Internet) {
            if(!_appShell.Navigation.ModalStack.OfType<NoInternetPage>().Any())
                await _appShell.Navigation.PushModalAsync(new NoInternetPage());
        }
    }

    private async Task HandleConnectivityAsync(ConnectivityChangedEventArgs e) {
        bool hasNoInternetModal = _appShell.Navigation.ModalStack.OfType<NoInternetPage>().Any();

        if(e.NetworkAccess != NetworkAccess.Internet) {
            if(!hasNoInternetModal)
                await _appShell.Navigation.PushModalAsync(new NoInternetPage());
        }
        else {
            if(hasNoInternetModal)
                await _appShell.Navigation.PopModalAsync();
        }
    }
}
