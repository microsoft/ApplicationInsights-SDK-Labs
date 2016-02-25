using Microsoft.ApplicationInsights.Channel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Channels
{
    public sealed class TestTelemetryChannel : ITelemetryChannel
    {
        private static List<ITelemetry> items = new List<ITelemetry>();
        private static object lockobj = new object();

        public bool? DeveloperMode { get; set; }

        public string EndpointAddress { get; set; }

        public void Flush()
        {
        }

        public void Send(ITelemetry item)
        {
            lock ( lockobj )
            {
                items.Add(item);
            }
        }

        public void Dispose()
        {
        }

        public static void Clear()
        {
            lock ( lockobj )
            {
                items.Clear();
            }
        }
        public static IList<ITelemetry> CollectedData()
        {
            lock ( lockobj )
            {
                List<ITelemetry> list = new List<ITelemetry>(items);
                return list;
            }
        }
    }
}
