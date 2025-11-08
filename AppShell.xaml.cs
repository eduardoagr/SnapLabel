namespace SnapLabel {
    public partial class AppShell : Shell {

        public AppShell() {
            InitializeComponent();

            Routing.RegisterRoute(nameof(NewProductPage), typeof(NewProductPage));
            Routing.RegisterRoute(nameof(ManageStoresPage), typeof(ManageStoresPage));
            Routing.RegisterRoute(nameof(NoInternetPage), typeof(NoInternetPage));

        }
    }
}
