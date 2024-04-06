using static Microsoft.Maui.ApplicationModel.Permissions;

namespace MauiNotifications.Utils;

internal class ForegroundServicePermission : BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new (string, bool)[]
            {
                    ("android.permission.FOREGROUND_SERVICE", true)
            };
#endif
}