namespace SnapLabel {
    public partial class AppShell : Shell {

        public AppShell() {
            InitializeComponent();

            Routing.RegisterRoute(nameof(NewProductPage), typeof(NewProductPage));
            Routing.RegisterRoute(nameof(StoresPage), typeof(StoresPage));
            Routing.RegisterRoute(nameof(NoInternetPage), typeof(NoInternetPage));

        }
    }
}
