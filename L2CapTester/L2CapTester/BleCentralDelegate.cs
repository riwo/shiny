using System.Collections.Generic;
using System.Threading.Tasks;
using Shiny;
using Shiny.BluetoothLE;
using Shiny.BluetoothLE.Central;
using Shiny.Logging;
using Xamarin.Forms;

namespace L2CapTester
{
    public class BleCentralDelegate : IBleCentralDelegate
    {
        public Task OnAdapterStateChanged(AccessState state)
        {
            Log.Write("BleCentralDelegate:OnAdapterStateChanged", $"BLE adapter state changed to {state}", ("State", state.ToString()));
            return Task.CompletedTask;
        }

        public Task OnConnected(IPeripheral peripheral)
        {
            Log.Write("BleCentralDelegate:OnConnected", $"Peripheral connected: {peripheral.Name}", ("Name", peripheral.Name), ("Status", peripheral.Status.ToString()), ("Uuid", peripheral.Uuid.ToString()));
            return Task.CompletedTask;
        }
    }
}