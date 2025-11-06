namespace SnapLabel.Views;

public partial class DesktopWarningPage : ContentPage {

    public DesktopWarningPage(DesktopWarningPageViewModel desktopWarningPageViewModel) {
        InitializeComponent();

        BindingContext = desktopWarningPageViewModel;
    }
}