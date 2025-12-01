namespace SnapLabel.ViewModels;

public partial class DashboardPageViewModel : ObservableObject {

    private readonly IShellService? _shellService;
    private readonly IFirebaseAuthClient? _firebaseAuthClient;
    private readonly DashboardTileServices? _tileServices;

    public ObservableCollection<DashboardTile> DashboardTiles { get; set; } = [];

    [ObservableProperty]
    public partial string? Username { get; set; }

    public DashboardPageViewModel(IShellService shellService, IFirebaseAuthClient firebaseAuth) {

        _shellService = shellService;

        _firebaseAuthClient = firebaseAuth;

        _tileServices = new DashboardTileServices(shellService, firebaseAuth);

        DashboardTiles = _tileServices.GetDashboardTiles();

        Username = _firebaseAuthClient.User.Info?.DisplayName;
    }

    [RelayCommand]
    async Task GoToManageStores() {

        await _shellService!.NavigateToAsync(nameof(StoresPage));

    }


}
