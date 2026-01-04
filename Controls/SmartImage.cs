namespace SnapLabel.Controls;

public partial class SmartImage : Image {
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };

    // Simple in-memory cache: URL → image bytes
    private static readonly Dictionary<string, byte[]> _cache = [];

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(ImageSource),
            typeof(SmartImage),
            default(ImageSource),
            propertyChanged: OnPlaceholderChanged);

    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (SmartImage)bindable;
        control.Source ??= (ImageSource)newValue;
    }

    public ImageSource Placeholder {
        get => (ImageSource)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly BindableProperty ErrorProperty =
        BindableProperty.Create(
            nameof(Error),
            typeof(ImageSource),
            typeof(SmartImage),
            default(ImageSource));

    /// <summary>
    /// Image to show when download or conversion fails
    /// </summary>
    public ImageSource Error {
        get => (ImageSource)GetValue(ErrorProperty);
        set => SetValue(ErrorProperty, value);
    }

    public static readonly BindableProperty DynamicSourceProperty =
        BindableProperty.Create(
            nameof(DynamicSource),
            typeof(object),
            typeof(SmartImage),
            default,
            propertyChanged: OnDynamicSourceChanged);

    public object DynamicSource {
        get => GetValue(DynamicSourceProperty);
        set => SetValue(DynamicSourceProperty, value);
    }

    private static async void OnDynamicSourceChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (SmartImage)bindable;

        try {
            switch(newValue) {
                case ImageSource imgSource:
                    control.Source = imgSource;
                    break;

                case byte[] bytes when bytes.Length > 0:
                    control.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
                    break;

                case string str when !string.IsNullOrWhiteSpace(str):
                    if(Uri.TryCreate(str, UriKind.Absolute, out var uri) &&
                        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) {
                        // Remote image with caching
                        control.Source = await LoadRemoteImage(uri) ?? control.Error ?? control.Placeholder;
                    } else {
                        // Try Base64
                        TryBase64(control, str);
                    }
                    break;

                default:
                    control.Source = control.Placeholder;
                    break;
            }
        } catch {
            control.Source = control.Error ?? control.Placeholder;
        }
    }

    private static void TryBase64(SmartImage control, string str) {
        try {
            var bytes = Convert.FromBase64String(str);
            control.Source = ImageSource.FromStream(() => new MemoryStream(bytes));
        } catch {
            control.Source = control.Error ?? control.Placeholder;
        }
    }

    private static async Task<ImageSource?> LoadRemoteImage(Uri uri) {
        try {
            var key = uri.ToString();


            if(_cache.TryGetValue(key, out var cachedBytes))
                return ImageSource.FromStream(() => new MemoryStream(cachedBytes, writable: false));

            var bytes = await DownloadImageAsync(uri);

            _cache[key] = bytes;

            return ImageSource.FromStream(() => new MemoryStream(bytes, writable: false));
        } catch {
            return null;
        }
    }

    private static async Task<byte[]> DownloadImageAsync(Uri uri) {
        using var stream = await _httpClient.GetStreamAsync(uri);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
}