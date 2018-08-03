namespace Microsoft.LocalForwarder.Library.Inputs
{
    /// <summary>
    /// Statistics regarding the current state of an Input.
    /// </summary>
    class InputStats
    {
        public int ConnectionCount = 0;

        public long BatchesReceived = 0;

        public long BatchesFailed = 0;
    }
}
