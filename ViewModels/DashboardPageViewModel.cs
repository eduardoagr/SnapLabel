namespace SnapLabel.ViewModels;

public partial class DashboardPageViewModel(IShellService shellService, IFirebaseAuthClient firebaseAuthClient,
    IDatabaseService<Store> databaseService,
    ITileService tileService,
    ICustomDialogService customDialogService, IMessenger messenger) :
    BasePageViewModel<Store>(shellService, firebaseAuthClient, databaseService, customDialogService, messenger) {

    [ObservableProperty]
    public partial ObservableCollection<Tile> DashboardTiles { get; set; } = [];

    [ObservableProperty]
    public partial string? Username { get; set; }

    [RelayCommand]
    async Task InitializeAsync() {

        tileService ??= new TileService(ShellService, FirebaseAuthClient);

        int? tilesCount = DashboardTiles?.Count;
        if(tilesCount < 4)
            DashboardTiles = tileService.GetDashboardTiles();

        Username = FirebaseAuthClient.User.Info?.DisplayName;

        await CheckForStores();
    }

    private async Task CheckForStores() {
        var stores = await DatabaseService.GetAllAsync(AppConstants.STORES_NODE);
        if(stores.Any()) {

            var prodTile = DashboardTiles.FirstOrDefault(t => t.Title == AppConstants.PRODUCTS_NODE);
            var empTile = DashboardTiles.FirstOrDefault(t => t.Title == AppConstants.Employees);

            prodTile?.IsVisible = true;
            empTile?.IsVisible = true;
        }
    }


    [RelayCommand]
    async Task GoToManageStores() {

        await NavigateAsync(nameof(StoresPage));

    }


}
