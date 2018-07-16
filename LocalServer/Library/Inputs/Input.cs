namespace Library.Inputs
{
    using System;
    using System.Threading.Tasks;
    using Contracts;

    /// <summary>
    /// Describes an input through which telemetry items are coming in.
    /// </summary>
    interface IInput
    {
        void Start(Action<TelemetryBatch> onBatchReceived);

        void Stop();

        bool IsRunning { get; }

        InputStats GetStats();
    }
}
