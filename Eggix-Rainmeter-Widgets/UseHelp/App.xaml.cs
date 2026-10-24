using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace UseHelp;

public partial class App : Application
{
    private static Mutex? _mutex;
    private const string MutexName = @"Global\UseHelp";

    protected override void OnStartup(StartupEventArgs e)
    {
        var mutex = new Mutex(true, MutexName, out bool createdNew);
        if (!createdNew)
        {
            ActivateExistingInstance();
            mutex.Dispose();
            Shutdown();
            return;
        }

        _mutex = mutex;
        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_mutex is not null)
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
        base.OnExit(e);
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool IsIconic(IntPtr hWnd);

    private static void ActivateExistingInstance()
    {
        var proc = Process.GetProcessesByName("UseHelp")
            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
        if (proc is null)
        {
            return;
        }

        var h = proc.MainWindowHandle;
        if (IsIconic(h))
        {
            ShowWindowAsync(h, 9); // SW_RESTORE
        }

        SetForegroundWindow(h);
    }
}
