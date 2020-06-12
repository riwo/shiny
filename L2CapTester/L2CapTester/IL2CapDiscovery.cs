using System.Threading.Tasks;

namespace L2CapTester
{
    public interface IL2CapDiscovery
    {
        Task StartScanningAsync();
    }
}