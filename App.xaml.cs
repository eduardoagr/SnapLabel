namespace SnapLabel;
public partial class App : Application {

    private readonly AppShell _appShell;
    private readonly Client _supabase;
    private readonly NoInternetPage _noInternetPage;
    private readonly DesktopWarningPage _desktopWarningPage;
    private readonly IConnectivity connectivity;

    private bool _supabaseInitialized = false;

    public App(AppShell appShell, Client supabase, IConnectivity _connectivity, NoInternetPage noInternetPage, DesktopWarningPage desktopWarning) {

        _appShell = appShell;

        _supabase = supabase;

        connectivity = _connectivity;

        _noInternetPage = noInternetPage;

        _desktopWarningPage = desktopWarning;


        InitializeComponent();

    }


    protected override Window CreateWindow(IActivationState? activationState) {

        //#if WINDOWS

        //        return new Window(_desktopWarningPage);

        //#endif

        var initialPage = connectivity.NetworkAccess == NetworkAccess.Internet ? (Page)_appShell : _noInternetPage;

        return new Window(initialPage);
    }

    protected override void OnStart() {

        base.OnStart();

        if(connectivity.NetworkAccess == NetworkAccess.Internet) {
            _ = InitializeSupabaseAsync();
        }
    }

    private async Task InitializeSupabaseAsync() {

        if(_supabaseInitialized)
            return;

        await _supabase.InitializeAsync();

        _supabaseInitialized = true;
    }


}