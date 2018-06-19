using System.Globalization;

namespace Library.Inputs
{
    using System.Collections.Generic;

    /// <summary>
    /// Statistics regarding the current state of a PipeServer.
    /// </summary>
    internal class PipeServerStats
    {
        public ulong BytesRead = 0;

        public bool IsClientConnected = false;
    }
}
