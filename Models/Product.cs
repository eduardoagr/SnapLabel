namespace SnapLabel.Models;


public partial class Product : ObservableObject, IFirebaseEntity {

    public string? Id { get; set; }

    [ObservableProperty]
    public partial string? Name { get; set; }

    [ObservableProperty]
    public partial string Price { get; set; }

    [ObservableProperty]
    public partial string Location { get; set; }

    [JsonIgnore]
    [ObservableProperty]
    public partial byte[] ImageeBytes { get; set; }

    public string? ImageUrl { get; set; }

    public string? QrUrl { get; set; }

    public string? StoreId { get; set; }
}
