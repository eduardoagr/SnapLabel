using System.ComponentModel;
using System.Windows.Input;

namespace SnapLabel.Controls;

public partial class TileView : ContentView {
    public TileView() {
        InitializeComponent();

        // Log whenever a property changes
        this.PropertyChanged += TileView_PropertyChanged;
    }

    private void TileView_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        // Log ALL property changes you care about
        switch(e.PropertyName) {
            case nameof(Title):
            Debug.WriteLine($"[TileView] Title = {Title}");
            break;

            case nameof(Subtitle):
            Debug.WriteLine($"[TileView] Subtitle = {Subtitle}");
            break;

            case nameof(TileBackground):
            Debug.WriteLine($"[TileView] TileBackground = {TileBackground}");
            break;

            case nameof(IconSource):
            Debug.WriteLine($"[TileView] IconSource = {IconSource}");
            break;

            case nameof(TapCommand):
            Debug.WriteLine($"[TileView] TapCommand = {TapCommand}");
            break;
        }
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
        set => SetValue(TileBackgroundProperty, value);
    }


    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title), typeof(string), typeof(TileView));

    public string Title {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
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
        set => SetValue(SubtitleProperty, value);
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
        set => SetValue(IconSourceProperty, value);
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
        set => SetValue(TapCommandProperty, value);
    }
}
