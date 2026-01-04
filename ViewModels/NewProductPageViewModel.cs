namespace SnapLabel.ViewModels;

public partial class NewProductPageViewModel : BasePageViewModel<Product> {
    private readonly IMediaPicker _mediaPicker;
    private readonly IBleManager _bleManager;
    private readonly IDatabaseService<Store> _storeDB;
    private readonly Client _client;
    private readonly IFirebaseAuthClient _firebaseAuthClient;

    public NewProductPageViewModel(
        IShellService shellService,
        IFirebaseAuthClient firebaseAuthClient,
        IDatabaseService<Product> databaseService,
        IMediaPicker mediaPicker,
        ICustomDialogService customDialogService,
        IBleManager bleManager,
        IDatabaseService<Store> storeDB,
        Client client,
        IMessenger messenger
    ) : base(shellService, firebaseAuthClient, databaseService, customDialogService, messenger) {

        _mediaPicker = mediaPicker;
        _bleManager = bleManager;
        _storeDB = storeDB;
        _client = client;
        _firebaseAuthClient = firebaseAuthClient;

        Product = new Product();
        TrackModel(Product, SaveProductCommand);
    }

    [ObservableProperty]
    public partial Product Product { get; set; } = new Product();

    [ObservableProperty]
    public partial List<Store> Stores { get; set; } = new List<Store>();

    [ObservableProperty]
    public partial bool IsDeviceConnected { get; set; }

    [ObservableProperty]
    public partial Store Store { get; set; } = new Store();

    [RelayCommand]
    public async Task GetConnctedDeices() {

        var store = await _storeDB.GetAllAsync("Stores");

        Stores = store.ToList();

        Store = Stores.FirstOrDefault(s => s.ManagerEmail == _firebaseAuthClient.User.Info.Email)!;

        var devices = _bleManager.GetConnectedPeripherals();

        var connectedDevice = devices.FirstOrDefault();

        if(connectedDevice is not null) {
            IsDeviceConnected = true;
        }
    }

    [ObservableProperty]
    public partial bool IsCustomImage { get; set; }

    #region Command for picking/capturing images

    [RelayCommand]
    public async Task CaptureImageAsync() {
        if(!_mediaPicker.IsCaptureSupported)
            return;

        var photo = await _mediaPicker.CapturePhotoAsync();
        if(photo is null)
            return;

        using var stream = await photo.OpenReadAsync();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        Product.ImageBytes = Operations.CompressImage(ms.ToArray())!;
    }

    #endregion

    // In SaveProductAsync(), remove the unnecessary assignment to 'uploadResponse'.
    // The result of the upload is not used, so you can simply await the upload call.

    [RelayCommand(CanExecute = nameof(CanExecute))]
    async Task SaveProductAsync() {

        await CustomDialogService.ShowAsync("Please wait", "loading.gif");

        var p = new Product {
            StoreId = Store.Id,
            Name = Product.Name!.TrimEnd(),
            Price = Product.Price!,
            Description = Product.Description!.TrimEnd(),
            ImageBytes = null,
            Quantity = Product.Quantity,
            ImageUrl = await Operations.SupabaseUploadAndGetUrlAsync(
               ShellService,
               _client,
               Product.Name!,
               Product.ImageBytes!,
               AppConstants.SUPABASE_BUCKET)

        };

        var id = await DatabaseService.InsertAsync(p);

        var qrCode = Operations.GeneerateQR(id);

        p.QrUrl = await Operations.SupabaseUploadAndGetUrlAsync(
            ShellService,
            _client,
            id,
            qrCode!,
            AppConstants.SUPABASE_BUCKET) ?? string.Empty;

        await DatabaseService.UpdateAsync(p);

        await NavigateBackAsync();

        await CustomDialogService.HideAsync();
    }

    async partial void OnIsCustomImageChanged(bool value) {
        if(value) {
            await NavigateAsync(nameof(DrawingPage));

            IsCustomImage = false;
        }
    }

    [RelayCommand]
    public async Task PrintProductAsync() {


    }

    private bool CanExecute() =>
       Validation.AllFilled(Product.Name, Product.Price, Product.Quantity, Product.Description)
       && Product.ImageBytes is { Length: > 0 };

    partial void OnProductChanged(Product value) {

        if(value is null)
            return;

        TrackModel(value, SaveProductCommand);
    }
}