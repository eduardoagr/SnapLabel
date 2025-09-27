using Android;
using Android.Content.PM;

using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace SnapLabel.Platforms.Android {
    public class BluetoothPermissionHelper {

        public static bool EnsureBluetoothScanPermission() {
            var activity = Platform.CurrentActivity!;
            var context = Platform.AppContext;

            if(ContextCompat.CheckSelfPermission(context, Manifest.Permission.BluetoothScan) != Permission.Granted) {
                ActivityCompat.RequestPermissions(activity, [
                Manifest.Permission.BluetoothScan,
                Manifest.Permission.BluetoothConnect,
                Manifest.Permission.AccessFineLocation
            ], 1000);

                return false;
            }

            return true;
        }

    }
}
