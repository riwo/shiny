using System;

namespace Shiny.BluetoothLE
{
    public class L2CapException : Exception
    {
        public L2CapException(string message) : base(message)
        {
        }
    }
}