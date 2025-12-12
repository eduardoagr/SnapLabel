namespace SnapLabel.Models;

public partial class Category : ObservableObject, IFirebaseEntity {

    public string? Id { get; set; }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Description { get; set; }

    [ObservableProperty]
    public partial string HexColor { get; set; }
}
