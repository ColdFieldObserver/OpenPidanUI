using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace HomeSettings;

public partial class App : Application
{
    private static Mutex? _mutex;
    private const string MutexName = @"Global\HomeSettings";

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
        if (_mutex != null)
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
        }
        base.OnExit(e);
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    private static void ActivateExistingInstance()
    {
        var proc = Process.GetProcessesByName("HomeSettings")
            .FirstOrDefault(p => p.MainWindowHandle != IntPtr.Zero);
        if (proc == null) return;

        var h = proc.MainWindowHandle;
        if (IsIconic(h)) ShowWindowAsync(h, 9); // SW_RESTORE
        SetForegroundWindow(h);
    }
}
