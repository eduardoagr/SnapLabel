
namespace SnapLabel.Controls;

public partial class TileView:ContentView {
    public TileView() {
        InitializeComponent();
    }

    // -------------------------------
    //  Bindable Properties
    // -------------------------------

    public static readonly BindableProperty TileBackgroundProperty =
       BindableProperty.Create(
           nameof(TileBackground),
           typeof(Color),
           typeof(TileView),
           Colors.Transparent,
           BindingMode.TwoWay);

    public Color TileBackground {
        get => (Color)GetValue(TileBackgroundProperty);
        set => SetValue(TileBackgroundProperty,value);
    }


    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),typeof(string),typeof(TileView));

    public string Title {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty,value);
    }



    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(
            nameof(Subtitle),
            typeof(string),
            typeof(TileView),
            string.Empty,
            BindingMode.TwoWay
        );

    public string Subtitle {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty,value);
    }

    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(
            nameof(IconSource),
            typeof(string),  // IMPORTANT: string, not ImageSource
            typeof(TileView),
            default(string),
            BindingMode.TwoWay
        );

    public string IconSource {
        get => (string)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty,value);
    }


    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(
            nameof(TapCommand),
            typeof(ICommand),
            typeof(TileView),
            null,
            BindingMode.TwoWay
        );

    public ICommand TapCommand {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty,value);
    }
}
