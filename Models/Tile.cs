namespace SnapLabel.Models;

public partial class Tile:ObservableObject {

    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string Icon { get; set; } = null!;

    public Color Background { get; set; } = Colors.Transparent;

    public ICommand? Command { get; set; }

    [ObservableProperty]
    public partial bool IsVisible { get; set; }
}
