using DeviceInfo = Microsoft.Maui.Devices.DeviceInfo;

namespace SnapLabel {
    public partial class App : Application {
        private readonly AppShell _appShell;

        public App(AppShell appShell) {
            _appShell = appShell;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            if(DeviceInfo.Idiom == DeviceIdiom.Desktop) {
                var warningPage = new ContentPage {
                    Content = new Label {
                        Text = $"This application does not suppert {DeviceInfo.Idiom}",
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        FontSize = 18,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                };

                var window = new Window(warningPage);

                Task.Run(async () => {
                    await Task.Delay(3000);
                    QuitApp();
                });

                return window;
            }

            return new Window(_appShell);
        }

        private void QuitApp() {
#if WINDOWS || MACCATALYST
            Environment.Exit(0);
#endif
        }
    }
}