namespace SnapLabel.Services {

    public partial class TileService(IShellService shellService,IFirebaseAuthClient authClient):ITileService {

        public ObservableCollection<Tile> GetDashboardTiles() {

            return [
                new() {
                    Background = Color.FromArgb("#af7f3c"),
                    Title = AppConstants.STORES_NODE,
                    Command = GoToStoresPageCommand,
                    Subtitle = "Tap to open",
                    Icon = "stores.png",
                    IsVisible = true,
                },
                new() {
                    Background = Color.FromArgb("#851d22"),
                    Title = AppConstants.LogOut,
                    Command = LogoutCommand,
                    Subtitle = "Tap to logout",
                    Icon = "logout.png",
                    IsVisible = true,
                },
                new() {
                    Background = Color.FromArgb("#167533"),
                    Title = AppConstants.PRODUCTS_NODE,
                    Command = GoToInventoryPageCommand,
                    Subtitle = "Tap to open",
                    Icon = "products.png",
                    IsVisible = false,
                },
                new() {
                    Background = Color.FromArgb("#003f5c"),
                    Title = AppConstants.Employees,
                    //command = GoToStoresPageCommand,
                    Subtitle = "Tap to open",
                    Icon = "employees.png",
                    IsVisible = false,
                },

            ];
        }

        [RelayCommand]
        async Task GoToStoresPage() {

            await shellService.NavigateToAsync(nameof(StoresPage));
        }

        [RelayCommand]
        async Task GoToInventoryPage() {

            await shellService.NavigateToAsync(nameof(InventoryPage));
        }

        [RelayCommand]
        async Task Logout() {

            if(authClient?.User != null) {
                authClient.SignOut();
            }

            var secretsPath = Path.Combine(FileSystem.AppDataDirectory,AppConstants.UserData);
            if(File.Exists(secretsPath)) {
                File.Delete(secretsPath);
            }

            await shellService.NavigateToAsync($"//{AppConstants.AUTH}");
        }
    }
}
