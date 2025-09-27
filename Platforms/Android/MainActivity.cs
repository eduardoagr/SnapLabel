using Android.App;
using Android.Content.PM;
using Android.Runtime;

namespace SnapLabel {

    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity {


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults) {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if(requestCode == 0) {
                bool allGranted = grantResults.All(r => r == Permission.Granted);
                if(allGranted) {
                    var app = IPlatformApplication.Current;
                    if(app != null) {
                        var scanner = app.Services.GetService<IBluetoothService>();
                        scanner?.StartScan(); // Now permission is granted, so scan proceeds
                    }
                    else {
                        Android.Widget.Toast.MakeText(this, "Application services unavailable.", Android.Widget.ToastLength.Short).Show();
                    }
                }
                else {
                    Android.Widget.Toast.MakeText(this, "Bluetooth permissions denied.", Android.Widget.ToastLength.Short).Show();
                }
            }
        }

    }

}
