namespace SnapLabel.ViewModels;

public partial class NewStorePageViewModel : BasePageViewModel<Store> {

    private readonly IFirebaseAuthClient _firebaseAuthClient;
    private readonly ICustomDialogService _customDialogService;
    private readonly IDatabaseService<User> _usersDb;

    public NewStorePageViewModel(IFirebaseAuthClient authClient, IShellService shellService,
        ICustomDialogService customDialogService,
        IDatabaseService<Store> storeDatabase,
        IDatabaseService<User> usersDatabase,
        IMessenger messenger)
        : base(shellService, storeDatabase, customDialogService, messenger) {

        _firebaseAuthClient = authClient;
        _customDialogService = customDialogService;
        _usersDb = usersDatabase;

        // Initialize Store and wire reevaluation
        Store = new Store();
        TrackModel(Store, CreateStoreCommand);
    }

    [ObservableProperty]
    public partial List<User> Users { get; set; } = [];

    [ObservableProperty]
    public partial Store Store { get; set; } = new Store();

    [ObservableProperty]
    public partial User? Manager { get; set; }


    [RelayCommand]
    private async Task GetUsers() {
        var items = await _usersDb.GetAllAsync("Users");

        Users = items.Where(u => string.IsNullOrEmpty(u.StoreId)).ToList();

        // If the current user is available, pre-populate Manager
        var currentUser = Users.FirstOrDefault(u => u.Email == _firebaseAuthClient.User.Info.Email);
        if(currentUser is not null) {
            Manager = currentUser;
        }
        else {
            Manager = null; // don’t pre-populate if already assigned
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateStore))]
    async Task CreateStore() {

        await _customDialogService.ShowAsync("Creating Store...", "loading.gif");

        Store = new Store {
            Name = Store.Name,
            Address = Store.Address,
            Phones = Store.Phones,
            StoreEmail = Store.StoreEmail,
            ManagerEmail = Manager?.Email,
            ManagerUsername = Manager?.Username,
            ManagerId = Manager?.Id,
            TotalRevenue = string.Empty,
            TotalSales = string.Empty,
            Id = string.Empty
        };

        await DatabaseService.InsertAsync(Store);

        Manager!.StoreId = Store.Id;   // assign the new store’s Id
        await _usersDb.UpdateAsync(Manager);


        await _customDialogService.HideAsync();

        await NavigateBackAsync();
    }

    private bool CanCreateStore() => Validation.AllFilled(Store.Name, Store.Address, Store.Phones, Store.StoreEmail);

    partial void OnStoreChanged(Store value) {
        if(value is null)
            return;
        TrackModel(value, CreateStoreCommand);

    }

}