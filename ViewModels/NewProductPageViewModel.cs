
namespace SnapLabel.ViewModels;

public partial class NewProductPageViewModel(IShellService shellService,
    IFirebaseAuthClient firebaseAuthClient,
    IDatabaseService<Product> databaseService,
    IMediaPicker mediaPicker,
    ICustomDialogService customDialogService,
    IBleManager bleManager,
    IDatabaseService<Store> _storeDB,
    IMessenger messenger) : BasePageViewModel<Product>(shellService, firebaseAuthClient, databaseService, customDialogService, messenger) {

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

        Store = Stores.FirstOrDefault(s => s.ManagerEmail == FirebaseAuthClient.User.Info.Email)!;

        var devices = bleManager.GetConnectedPeripherals();

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
        if(!mediaPicker.IsCaptureSupported)
            return;

        var photo = await mediaPicker.CapturePhotoAsync();
        if(photo is null)
            return;

        using var stream = await photo.OpenReadAsync();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);

        Product.ImageeBytes = Operations.CompressImage(ms.ToArray())!;
    }

    #endregion Command for picking/capturing images

    private async Task SaveProductAsync() {

        //ProductVM.SaveToModel();
        //var product = ProductVM.GetProduct();
        //var id = await _databaseService.TryAddProductAsync(product);
        //if(id > 0) {
        //    //Convert everythin to json and Create from product
        //    //var qr = Operations.GenerateProdutQrCode(product);

        //    //await _shellService.NavigateBackAsync();

        //}
    }

    async partial void OnIsCustomImageChanged(bool value) {
        if(value) {
            await NavigateAsync(nameof(DrawingPage));

            IsCustomImage = false;
        }
    }

    [RelayCommand]
    public async Task Print() {
        if(!IsDeviceConnected) {

            var goConnect = await DisplayConfirmAsync("No Device Connected",
                "Do you want to connect to a device",
                "OK", "Cancel");

            if(goConnect) {
                // Navigate back and say we want connection
                await NavigateAsync("..?connect=true");
            }

            return;
        }

    }
}