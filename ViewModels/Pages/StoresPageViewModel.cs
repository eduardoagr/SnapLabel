namespace SnapLabel.ViewModels;

public partial class StoresPageViewModel : ObservableObject {

    [RelayCommand]
    private async Task AddStore() {
        await Shell.Current.GoToAsync(nameof(StoresPage));
    }
}
