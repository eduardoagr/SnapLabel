namespace SnapLabel.Views;

public partial class NewStorePage : ContentPage {
    public NewStorePage(NewStorePageViewModel newStorePageViewModel) {
        InitializeComponent();

        BindingContext = newStorePageViewModel;
    }
}