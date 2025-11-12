namespace SnapLabel.ViewModels;

public partial class ProductViewModel : ObservableObject {

    private readonly Product _product;

    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial decimal? Price { get; set; }

    [ObservableProperty]
    public partial byte[] ImageBytes { get; set; } = Array.Empty<byte>();

    [ObservableProperty]
    public partial string Location { get; set; }

    public ProductViewModel(Product product) {
    }
}