namespace Microsoft.LocalForwarder.Library.Inputs
{
    /// <summary>
    /// Statistics regarding the current state of a PipeServer.
    /// </summary>
    internal class PipeServerStats
    {
        public ulong BytesRead = 0;

        public bool IsClientConnected = false;
    }
}
