namespace SnapLabel;

public partial class AppShell : Shell {
    private readonly IFirebaseAuthClient _auth;

    public AppShell(IFirebaseAuthClient auth) {
        InitializeComponent();
        _auth = auth;

        Routing.RegisterRoute(nameof(NewProductPage), typeof(NewProductPage));
        Routing.RegisterRoute(nameof(StoresPage), typeof(StoresPage));
        Routing.RegisterRoute(nameof(InventoryPage), typeof(InventoryPage));
        Routing.RegisterRoute(nameof(NoInternetPage), typeof(NoInternetPage));
        Routing.RegisterRoute(nameof(DrawingPage), typeof(DrawingPage));
        Routing.RegisterRoute(nameof(NewStorePage), typeof(NewStorePage));

    }

    protected override async void OnAppearing() {
        base.OnAppearing();

        if(_auth.User == null)
            await GoToAsync($"//{AppConstants.AUTH}");
        else
            await GoToAsync($"//{AppConstants.HOME}");
    }

}