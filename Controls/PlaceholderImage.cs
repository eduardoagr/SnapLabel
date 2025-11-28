namespace SnapLabel.Controls;

public partial class PlaceholderImage : Image {
    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(ImageSource),
            typeof(PlaceholderImage),
            default(ImageSource),
            propertyChanged: OnPlaceholderChanged);

    public ImageSource Placeholder {
        get => (ImageSource)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (PlaceholderImage)bindable;

        // If Source is null or empty, apply placeholder
        if(control.Source == null || control.Source is FileImageSource { File: "" }) {
            control.Source = (ImageSource)newValue;
        }
    }

    protected override void OnPropertyChanged(string? propertyName = null) {
        base.OnPropertyChanged(propertyName);

        if(propertyName == nameof(Source)) {
            // If Source is null or empty, apply placeholder
            if(Source == null || Source is FileImageSource { File: "" }) {
                Source = Placeholder;
            }
        }
    }
}