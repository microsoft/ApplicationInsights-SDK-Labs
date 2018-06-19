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
        Task Start(Action<TelemetryBatch> onBatchReceived);

        void Stop();

        InputStats GetStats();
    }
}
