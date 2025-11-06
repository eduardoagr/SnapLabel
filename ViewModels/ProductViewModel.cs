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
    public partial string? Price { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveToModelCommand))]
    public partial byte[] ImageBytes { get; set; } = Array.Empty<byte>();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveToModelCommand))]
    public partial string Location { get; set; }

    public string FormattedPrice {
        get {
            if(decimal.TryParse(Price, NumberStyles.Any, CultureInfo.CurrentCulture, out var value))
                return string.Format(CultureInfo.CurrentCulture, "{0:C}", value);
            return Price ?? string.Empty;
        }
    }

    public ProductViewModel(Product product) {
        _product = product;

        // Copy model data to ViewModel
        Name = product.Name ?? string.Empty;
        Price = product.Price;
        ImageBytes = product.ImageBytes ?? [];
        Location = product.Location ?? string.Empty;

        // Listen for property changes
        PropertyChanged += (s, e) => ProductPropertiesChanged?.Invoke();
    }

    public bool CanSave =>
        !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(Price)
        && (ImageBytes?.Length ?? 0) > 0
        && !string.IsNullOrWhiteSpace(Location);

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void SaveToModel() {
        _product.Name = Name;
        _product.Price = Price;
        _product.ImageBytes = ImageBytes;
        _product.Location = Location;
    }

    public Product GetProduct() {
        SaveToModel();
        return _product;
    }

    partial void OnPriceChanged(string? value) {
        OnPropertyChanged(nameof(FormattedPrice));
    }
}