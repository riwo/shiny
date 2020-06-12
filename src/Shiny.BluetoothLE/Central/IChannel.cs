using System;
using System.IO;


namespace Shiny.BluetoothLE.Central
{
    public interface IChannel : IDisposable
    {
        Guid PeerUuid { get; }
        int Psm { get; } //=> 0x25;

        Stream Stream { get; }
    }
}