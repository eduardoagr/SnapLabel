

namespace SnapLabel.ViewModels;

public partial class NewProductPageViewModel(IShellService shellService,
    IDatabaseService<Product> databaseService,
    IBleManager bleManager,
    ICustomDialogService customDialogService,
    IMediaPicker mediaPicker,
    IMessenger messenger) : BasePageViewModel<Product>(shellService, databaseService, customDialogService, messenger) {

    [ObservableProperty]
    public partial bool IsDeviceConnected { get; set; }

    [RelayCommand]
    public void GetConnctedDeices() {

        var devices = bleManager.GetConnectedPeripherals();

        var connectedDevice = devices.FirstOrDefault();

        if(connectedDevice is not null) {
            IsDeviceConnected = true;
        }
    }

    [ObservableProperty]
    public partial bool IsCustomImage {
        get; set;
    }

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

        //ProductVM.ImageBytes = Operations.CompressImage(ms.ToArray())!;
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
        if(IsDeviceConnected) {
            PrintData(){

            }
        }
    }
}