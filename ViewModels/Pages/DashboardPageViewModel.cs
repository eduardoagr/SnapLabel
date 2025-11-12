namespace SnapLabel.ViewModels;

public partial class DashboardPageViewModel(IShellService shellService, Client client) : ObservableObject {

    [RelayCommand]
    async Task GoToManageStores() {

        await shellService.NavigateToAsync(nameof(StoresPage));

    }


}
