namespace Microsoft.ApplicationInsights.Wcf.Tests.Channels
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ApplicationInsights.Channel;

    public sealed class TestTelemetryChannel : ITelemetryChannel
    {
        private static List<ITelemetry> items = new List<ITelemetry>();
        private static object lockobj = new object();

        public bool? DeveloperMode { get; set; }

        public string EndpointAddress { get; set; }

        public static void Clear()
        {
            lock (lockobj)
            {
                items.Clear();
            }
        }

        public static IList<ITelemetry> CollectedData()
        {
            lock (lockobj)
            {
                List<ITelemetry> list = new List<ITelemetry>(items);
                return list;
            }
        }

        public void Flush()
        {
        }

        public void Send(ITelemetry item)
        {
            lock (lockobj)
            {
                items.Add(item);
            }
        }

        public void Dispose()
        {
        }
    }
}
