using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Concurrent;
using System.Xml;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal sealed class MessageCorrelator
    {
        private ConcurrentDictionary<String, DependencyTelemetry> pendingMessages = new ConcurrentDictionary<String, DependencyTelemetry>();

        public void Add(UniqueId messageId, DependencyTelemetry telemetry)
        {
            pendingMessages[messageId.ToString()] = telemetry;
        }

        public bool TryLookup(UniqueId messageId, out DependencyTelemetry telemetry)
        {
            return pendingMessages.TryRemove(messageId.ToString(), out telemetry);
        }

        public void Remove(UniqueId messageId)
        {
            DependencyTelemetry telemetry = null;
            pendingMessages.TryRemove(messageId.ToString(), out telemetry);
        }

        public void Clear()
        {
            pendingMessages.Clear();
        }
    }
}
