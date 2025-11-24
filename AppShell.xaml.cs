namespace SnapLabel {
    public partial class AppShell : Shell {

        public AppShell() {
            InitializeComponent();

            Routing.RegisterRoute(nameof(NewProductPage), typeof(NewProductPage));
            Routing.RegisterRoute(nameof(StoresPage), typeof(StoresPage));
            Routing.RegisterRoute(nameof(InventoryPage), typeof(InventoryPage));
            Routing.RegisterRoute(nameof(NoInternetPage), typeof(NoInternetPage));
            Routing.RegisterRoute(nameof(DrawingPage), typeof(DrawingPage));


        }
    }
}
