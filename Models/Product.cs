namespace SnapLabel.Models;


public partial class Product : ObservableObject, IFirebaseEntity {

    public string? Id { get; set; }

    [ObservableProperty]
    public partial string? Name { get; set; }

    [ObservableProperty]
    public partial string? Price { get; set; }

    [ObservableProperty]
    public partial string? Location { get; set; }

    [ObservableProperty]
    public partial string? Quantity { get; set; }

    [JsonIgnore]
    [ObservableProperty]
    public partial byte[]? ImageBytes { get; set; } = Array.Empty<byte>();

    public string? ImageUrl { get; set; }

    public string? QrUrl { get; set; }

    public string? StoreId { get; set; }
}
