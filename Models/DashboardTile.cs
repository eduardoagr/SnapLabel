using System.Windows.Input;

namespace SnapLabel.Models {

    public class DashboardTile {

        public string Title { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public string Icon { get; set; } = null!;

        public Color Background { get; set; } = Colors.Transparent;

        public ICommand? Command { get; set; }
    }
}
