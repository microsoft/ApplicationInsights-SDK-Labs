using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestExceptionTracking
{
    class Program
    {
        static void Main(string[] args)
        {
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(10);
            }

            TelemetryClient client = new TelemetryClient();
            var module = Microsoft.ApplicationInsights.Extensibility.Implementation.TelemetryModules.Instance;

            client.TrackEvent("Started");

            client.Flush();

        }
    }
}
