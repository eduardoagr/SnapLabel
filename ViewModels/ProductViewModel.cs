namespace SnapLabel.ViewModels;

public partial class ProductViewModel : ObservableObject {

    // Event to notify changes in properties

    public event Action? ProductPropertiesChanged;

    private readonly Product _product;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveToModelCommand))]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveToModelCommand))]
    public partial decimal Price { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveToModelCommand))]
    public partial byte[] ImageBytes { get; set; } = Array.Empty<byte>();

    [ObservableProperty]
    public partial string ImageSize { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ImageSource? ImagePreview { get; set; }

    public ProductViewModel(Product product) {
        _product = product;

        // Copy model data to ViewModel
        Name = product.Name ?? string.Empty;
        Price = product.Price;
        ImageBytes = product.ImageBytes ?? [];
        ImagePreview = product.ImagePreview;
        ImageSize = product.ImageSize ?? string.Empty;

        // Listen for property changes
        PropertyChanged += (s, e) => ProductPropertiesChanged?.Invoke();
    }

    public bool CanSave =>
        !string.IsNullOrWhiteSpace(Name)
        && Price > 0
        && ImageBytes.Length > 0;

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void SaveToModel() {
        _product.Name = Name;
        _product.Price = Price;
        _product.ImageBytes = ImageBytes;
        _product.ImagePreview = ImagePreview;
        _product.ImageSize = ImageSize;
    }

    public Product GetProduct() {
        SaveToModel();
        return _product;
    }
}
