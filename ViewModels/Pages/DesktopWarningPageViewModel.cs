namespace SnapLabel.ViewModels;

public partial class DesktopWarningPageViewModel : ObservableObject {

    [RelayCommand]
    void CloseApp() {
        Environment.Exit(0);
    }
}
