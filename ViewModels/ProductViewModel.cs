namespace SnapLabel.ViewModels;

public partial class ProductViewModel : ObservableObject {

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial decimal? Price { get; set; }

    [ObservableProperty]
    public partial byte[] ImageBytes { get; set; } = Array.Empty<byte>();

    [ObservableProperty]
    public partial string Location { get; set; }

    public ProductViewModel(Product product) {


        // Listen for property changes
    }

    public bool CanSave =>
        !string.IsNullOrWhiteSpace(Name)
        && (ImageBytes?.Length ?? 0) > 0
        && !string.IsNullOrWhiteSpace(Location);

}