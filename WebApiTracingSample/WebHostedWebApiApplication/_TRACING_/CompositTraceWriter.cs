namespace Microsoft.ApplicationInsights.Sample
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Tracing;

    internal sealed class CompositTraceWriter : ITraceWriter, IDisposable
    {
        private ITraceWriter[] writers;

        public CompositTraceWriter(IEnumerable<ITraceWriter> writers)
        {
            if (writers == null)
            {
                throw new ArgumentNullException("writers");
            }

            this.writers = writers.ToArray();
        }

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            foreach (var writer in this.writers)
            {
                writer.Trace(request, category, level, traceAction);
            }
        }

        public void Dispose()
        {
            if (this.writers != null)
            {
                for (int i = 0; i < this.writers.Length; i++)
                {
                    var disposable = this.writers[0] as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }

                this.writers = null;
            }
        }
    }
}
