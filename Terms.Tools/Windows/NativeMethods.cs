using System;
using System.Runtime.InteropServices;

namespace Terms.Tools.Windows;

public static class NativeMethods
{
    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool InitiateSystemShutdownEx(string lpMachineName, string lpMessage, uint dwTimeout, bool bForceAppsClosed, bool bRebootAfterShutdown, ShutdownReason dwReason);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr handle, int command);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

    public const int SW_SHOWNORMAL = 1;
    public const int SW_SHOWMINIMIZED = 2;
    public const int SW_SHOWMAXIMIZED = 3;
    public const int SW_RESTORE = 9;

    public const int LOGON32_LOGON_NETWORK_CLEARTEXT = 0;
    public const int LOGON32_PROVIDER_DEFAULT = 0;
}