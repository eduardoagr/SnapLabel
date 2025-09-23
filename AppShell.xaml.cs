using SnapLabel.Views;

namespace SnapLabel {
    public partial class AppShell : Shell {

        public AppShell() {
            InitializeComponent();
            Routing.RegisterRoute(nameof(NewProductPage), typeof(NewProductPage));
        }
    }
}
