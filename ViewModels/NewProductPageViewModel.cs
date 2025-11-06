
namespace SnapLabel.ViewModels;

public partial class NewProductPageViewModel : ObservableObject {

    #region Readonly and Static Fields
    private readonly IMediaPicker _mediaPicker;
    private readonly IShellService _shellService;
    private readonly IDatabaseService _databaseService;
    private readonly Client _supabaseClient;
    //private readonly DatabaseService databaseService;

    #endregion Readonly and Static Fields

    public ProductViewModel ProductVM { get; }

    // Explicit command
    public AsyncRelayCommand SaveProductAsyncCommand { get; set; }

    #region Constructor

    public NewProductPageViewModel(IMediaPicker mediaPicker, IShellService shellService,
        IDatabaseService databaseService, Client supabaseClient) {

        _mediaPicker = mediaPicker;
        _shellService = shellService;
        _databaseService = databaseService;
        _supabaseClient = supabaseClient;

        ProductVM = new ProductViewModel(new Product());
        ProductVM.ProductPropertiesChanged += ProductVM_ProductPropertiesChanged;
        SaveProductAsyncCommand = new AsyncRelayCommand(SaveProductAsync, CanSaveProduct);
    }

    #endregion Constructor

    private void ProductVM_ProductPropertiesChanged() {

        SaveProductAsyncCommand?.NotifyCanExecuteChanged();
    }

    private bool CanSaveProduct() => ProductVM.CanSave;

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

        ProductVM.ImageBytes = Operations.CompressImage(ms.ToArray())!;
    }

    #endregion Command for picking/capturing images

    private async Task SaveProductAsync() {

        ProductVM.SaveToModel();
        var product = ProductVM.GetProduct();
        var id = await _databaseService.TryAddProductAsync(product);
        if(id > 0) {
            //Convert everythin to json and Create from product
            var qr = Operations.GenerateProdutQrCode(product);

            await _shellService.NavigateBackAsync();

        }
    }
}