namespace SnapLabel.Services {

    public partial class DashboardTileServices(IShellService shellService, IFirebaseAuthClient authClient) {

        public ObservableCollection<DashboardTile> GetDashboardTiles() {

            return [
                new() {
                    Background = Color.FromArgb("#af7f3c"),
                    Title = "Stores",
                    Command = GoToStoresPageCommand,
                    Subtitle = "Tap to open",
                    Icon = "stores.png"
                },
                new() {
                    Background = Color.FromArgb("#167533"),
                    Title = "Inventory",
                    Command = GoToInventoryPageCommand,
                    Subtitle = "Tap to open",
                    Icon = "products.png",
                },
                new() {
                    Background = Color.FromArgb("#003f5c"),
                    Title = "employees",
                    //command = GoToStoresPageCommand,
                    Subtitle = "Tap to open",
                    Icon = "employees.png",
                },
                new() {
                    Background = Color.FromArgb("#851d22"),
                    Title = "logout",
                    Command = LogoutCommand,
                    Subtitle = "Tap to logout",
                    Icon = "logout.png",
                }
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

            var secretsPath = Path.Combine(FileSystem.AppDataDirectory, AppConstants.UserData);
            if(File.Exists(secretsPath)) {
                File.Delete(secretsPath);
            }

            await shellService.NavigateToAsync($"//{AppConstants.AUTH}");
        }
    }
}
