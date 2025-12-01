namespace SnapLabel.ViewModels;

public partial class StoresPageViewMode(
    IShellService shellService,
    IFirebaseAuthClient firebaseAuthClient,
    IDatabaseService<Store> databaseService,
    ICustomDialogService customDialogService,
    IMessenger messenger) : BasePageViewModel<Store>(shellService, firebaseAuthClient, databaseService, customDialogService, messenger) {

    [ObservableProperty]
    public partial List<Store> Stores { get; set; } = new();

    [RelayCommand]
    private async Task AddStore() {
        await NavigateAsync(nameof(NewStorePage));
    }

    [RelayCommand]
    async Task GetStores() {
        Stores.Clear();
        var stores = await DatabaseService.GetAllAsync("Stores");
        Stores = stores.ToList();
    }
}
