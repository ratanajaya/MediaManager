using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SharedLibrary;

public class PcService : IPcService
{
    [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

    public void Sleep() {
        SetSuspendState(false, true, true);
    }

    public void Hibernate() {
        SetSuspendState(true, true, true);
    }

    public void Shutdown() {
        var psi = new ProcessStartInfo("shutdown", "/s /t 0");
        psi.CreateNoWindow = true;
        psi.UseShellExecute = false;
        Process.Start(psi);
    }
}
