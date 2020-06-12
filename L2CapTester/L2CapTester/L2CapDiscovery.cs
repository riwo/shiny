using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Shiny.BluetoothLE.Central;

namespace L2CapTester
{
    public class L2CapDiscovery : IL2CapDiscovery, IDisposable
    {
        private static readonly ScanConfig ScanConfig = new ScanConfig();
        private readonly ICentralManager bleManager;
        private IDisposable _scanSubscription;


        public L2CapDiscovery(ICentralManager bleManager)
        {
            this.bleManager = bleManager;
        }

        public void Dispose()
        {
            _scanSubscription?.Dispose();
        }

        public async Task StartScanningAsync()
        {
            var connectedPeripherals = await this.bleManager.GetConnectedPeripherals().ToTask().ConfigureAwait(true);
            foreach (var peripheral in connectedPeripherals)
                Console.WriteLine($"Device '{peripheral.Name}': Status '{peripheral.Status}'");

            var deviceFound = false;

            this._scanSubscription = this.bleManager.Scan(ScanConfig).ObserveOn(App.MainThreadScheduler).Subscribe(scanResult =>
            {
                var peripheral = scanResult.Peripheral;
                if (peripheral.Name == "ble-linux")
                {
                    if (!deviceFound)
                    {
                        deviceFound = true;
                        peripheral.ConnectWait(new ConnectionConfig { AutoConnect = false }).Subscribe(p => 
                        { 
                            p.OpenChannel(37).Subscribe(async channel =>
                            {
                                var buffer = new byte[1024];
                                while (true)
                                {
                                    var bytesRead = await channel.Stream.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None).ConfigureAwait(false);
                                    Console.WriteLine($"Read {bytesRead} bytes.");
                                    if (bytesRead == 0)
                                        return;
                                }
                            });
                        });
                    }
                }
            });
        }
    }
}