namespace SnapLabel.Controls {
    public partial class SmartImage : Grid {

        private readonly Image _image;
        private readonly ActivityIndicator _spinner;

        private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(10) };
        private static readonly Dictionary<string, byte[]> _cache = [];
        private static readonly Dictionary<string, Task<byte[]>> _pendingDownloads = [];

        // For cancellation
        private CancellationTokenSource? _cts;

        public SmartImage() {
            _image = new Image { Aspect = Aspect.AspectFill };
            _spinner = new ActivityIndicator {
                IsVisible = false,
                IsRunning = false,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            Children.Add(_image);
            Children.Add(_spinner);
        }

        #region PlaceholderProperty
        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(ImageSource), typeof(SmartImage), default(ImageSource), propertyChanged: OnPlaceholderChanged);

        public ImageSource Placeholder {
            get => (ImageSource)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue) {
            var control = (SmartImage)bindable;
            if(control._image.Source == null && newValue is ImageSource img)
                control._image.Source = img;
        }
        #endregion

        #region ErrorProperty
        public static readonly BindableProperty ErrorProperty =
            BindableProperty.Create(nameof(Error), typeof(ImageSource), typeof(SmartImage), default(ImageSource));

        public ImageSource Error {
            get => (ImageSource)GetValue(ErrorProperty);
            set => SetValue(ErrorProperty, value);
        }
        #endregion

        #region DynamicSourceProperty
        public static readonly BindableProperty DynamicSourceProperty =
            BindableProperty.Create(nameof(DynamicSource), typeof(object), typeof(SmartImage), default, propertyChanged: async (b, o, n) => await OnDynamicSourceChanged(b, o, n));

        public object DynamicSource {
            get => GetValue(DynamicSourceProperty);
            set => SetValue(DynamicSourceProperty, value);
        }
        #endregion

        private static async Task OnDynamicSourceChanged(BindableObject bindable, object oldValue, object newValue) {
            var control = (SmartImage)bindable;

            // Cancel any previous download
            control._cts?.Cancel();
            control._cts = new CancellationTokenSource();
            var ct = control._cts.Token;

            control._spinner.IsVisible = true;
            control._spinner.IsRunning = true;
            control._image.Source = control.Placeholder;

            try {
                switch(newValue) {
                    case ImageSource imgSource:
                        control._image.Source = imgSource;
                        break;

                    case byte[] bytes when bytes.Length > 0:
                        control._image.Source = ImageSource.FromStream(() => new MemoryStream(bytes, writable: false));
                        break;

                    case string str when !string.IsNullOrWhiteSpace(str):
                        if(Uri.TryCreate(str, UriKind.Absolute, out var uri) &&
                            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)) {
                            var remote = await control.LoadRemoteImage(uri, ct);
                            control._image.Source = remote ?? control.Error;
                        }
                        else {
                            TryBase64(control, str);
                        }
                        break;

                    default:
                        control._image.Source = control.Placeholder;
                        break;
                }
            } catch(OperationCanceledException) {
                // Ignore canceled requests
            } catch {
                control._image.Source = control.Error;
            } finally {
                control._spinner.IsVisible = false;
                control._spinner.IsRunning = false;
            }
        }

        private static void TryBase64(SmartImage control, string str) {
            try {
                var bytes = Convert.FromBase64String(str);
                control._image.Source = ImageSource.FromStream(() => new MemoryStream(bytes, writable: false));
            } catch {
                control._image.Source = control.Error;
            }
        }

        private async Task<ImageSource?> LoadRemoteImage(Uri uri, CancellationToken ct) {
            try {
                var key = uri.ToString();

                if(_cache.TryGetValue(key, out var cachedBytes))
                    return ImageSource.FromStream(() => new MemoryStream(cachedBytes, writable: false));

                // Avoid duplicate downloads
                Task<byte[]>? downloadTask;
                lock(_pendingDownloads) {
                    if(!_pendingDownloads.TryGetValue(key, out downloadTask) || downloadTask is null) {
                        downloadTask = DownloadImageAsync(uri, ct);
                        _pendingDownloads[key] = downloadTask;
                    }
                }

                var bytes = await downloadTask;

                lock(_pendingDownloads)
                    _pendingDownloads.Remove(key);

                _cache[key] = bytes;

                return ImageSource.FromStream(() => new MemoryStream(bytes, writable: false));
            } catch(OperationCanceledException) {
                throw; // let caller ignore
            } catch {
                return null;
            }
        }

        private static async Task<byte[]> DownloadImageAsync(Uri uri, CancellationToken ct) {
            using var stream = await _httpClient.GetStreamAsync(uri, ct);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, ct);
            return ms.ToArray();
        }
    }
}
