using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReleControl.Platforms.Android
{
    public class RequestPermission : Permissions.BasePlatformPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
       new List<(string androidPermission, bool isRuntime)>
       {
        (global::Android.Manifest.Permission.Bluetooth, true),
        (global::Android.Manifest.Permission.BluetoothAdmin, true),
        (global::Android.Manifest.Permission.BluetoothConnect, true),
        (global::Android.Manifest.Permission.BluetoothAdvertise, true),
        (global::Android.Manifest.Permission.BluetoothPrivileged, true),
        (global::Android.Manifest.Permission.BluetoothScan, true),
        (global::Android.Manifest.Permission.WriteExternalStorage, true),
       }.ToArray();
    }
}
