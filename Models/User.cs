namespace SnapLabel.Models;

public partial class User : ObservableObject, IFirebaseEntity {

    public string? Id { get; set; }

    [ObservableProperty]
    public partial string? Username { get; set; }

    [ObservableProperty]
    public partial string? Email { get; set; }

    [JsonIgnore]
    [ObservableProperty]
    public partial string? Password { get; set; }

    public string? PhoneNumber { get; set; }

    public string? StoreId { get; set; }
}

