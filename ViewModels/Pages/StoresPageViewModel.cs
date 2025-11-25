namespace SnapLabel.ViewModels;

public partial class StoresPageViewModel : ObservableObject {

    private readonly IShellService _shellService;
    private readonly IDatabaseService<Store> _databaseService;




    public StoresPageViewModel(IShellService shellService, IDatabaseService<Store> databaseService) {

        _databaseService = databaseService;
        _shellService = shellService;
    }

    [RelayCommand]
    private async Task AddStore() {
        await _shellService.NavigateToAsync(nameof(NewStorePage));
    }
}
