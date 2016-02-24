using System;
using Microsoft.ApplicationInsights.DataContracts;
using System.Diagnostics;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class RequestHolder : IRequestHolder
    {
        private Stopwatch watch;
        public RequestTelemetry Request { get; private set;  }

        public DateTimeOffset StartedAt { get; private set; }

        public RequestHolder()
        {
            watch = new Stopwatch();
        }

        public void Start()
        {
            if ( watch.IsRunning )
                throw new InvalidOperationException("Start() has already been called");

            Request = new RequestTelemetry();
            StartedAt = DateTimeOffset.Now;
            watch.Start();
        }

        public TimeSpan Stop()
        {
            watch.Stop();
            return watch.Elapsed;
        }
    }
}
