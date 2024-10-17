using System.Runtime.InteropServices;

namespace CoreAPI.AL.DataAccess;

public interface IPcService
{
    void Hibernate();
    void Sleep();
}

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
}
