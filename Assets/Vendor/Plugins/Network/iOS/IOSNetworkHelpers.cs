#if UNITY_IOS
using System.Runtime.InteropServices;

public static class IOSNetworkHelpers
{
    [DllImport("__Internal")]
    private static extern string _iOSGetSsidName();

    public static string GetCurrentSsidName() => _iOSGetSsidName();
}
#endif