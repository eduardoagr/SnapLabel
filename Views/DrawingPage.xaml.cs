namespace SnapLabel.Views;

public partial class DrawingPage : ContentPage {

    public DrawingPage(DrawingPageViewModel drawingPageViewModel) {

        BindingContext = drawingPageViewModel;

        InitializeComponent();
    }
}