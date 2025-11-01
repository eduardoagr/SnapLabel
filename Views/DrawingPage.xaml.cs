namespace SnapLabel.Views;

public partial class DrawingPage : ContentPage {

    public DrawingPage(DrawingPageViewModel drawingPageViewModel) {

        BindingContext = drawingPageViewModel;

        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args) {
        base.OnNavigatedTo(args);

        if(BindingContext is DrawingPageViewModel vm) {
            vm.RefreshBluetoothStatus();
        }
    }
}