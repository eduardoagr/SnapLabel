using Syncfusion.Licensing;

namespace SnapLabel {
    public partial class App : Application {

        readonly AppShell AppShell;

        public App(AppShell appShell) {

            SyncfusionLicenseProvider.RegisterLicense(Constants.Constants.SYNCFUSION);

            AppShell = appShell;

            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState) {
            return new Window(AppShell);
        }
    }
}