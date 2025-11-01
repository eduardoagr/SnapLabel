namespace SnapLabel.Views;

public partial class NewProductPage : ContentPage {

    public NewProductPage(NewProductPageViewModel newProductPageViewModel) {
        InitializeComponent();

        BindingContext = newProductPageViewModel;
    }
}