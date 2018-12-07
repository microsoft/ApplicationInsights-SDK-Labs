namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Xml;
    using Microsoft.ApplicationInsights.DataContracts;

    internal sealed class MessageCorrelator : IDisposable
    {
        private Dictionary<string, PendingMessage> pendingMessages;
        private Action<UniqueId, DependencyTelemetry> timeoutCallback;
        private object lockObject;
        private bool disposed;

        public MessageCorrelator(Action<UniqueId, DependencyTelemetry> callback = null)
        {
            this.timeoutCallback = callback;
            this.pendingMessages = new Dictionary<string, PendingMessage>();
            this.lockObject = new object();
            this.disposed = false;
        }

        public void Add(UniqueId messageId, DependencyTelemetry telemetry, TimeSpan timeout)
        {
            if (messageId == null)
            {
                throw new ArgumentNullException(nameof(messageId));
            }

            if (telemetry == null)
            {
                throw new ArgumentNullException(nameof(telemetry));
            }

            lock (this.lockObject)
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(nameof(MessageCorrelator));
                }

                var pending = new PendingMessage(messageId, telemetry);
                this.pendingMessages[messageId.ToString()] = pending;
                pending.Start(timeout, this.OnTimeout);
            }
        }

        public bool TryLookup(UniqueId messageId, out DependencyTelemetry telemetry)
        {
            telemetry = null;
            lock (this.lockObject)
            {
                PendingMessage pending = null;
                if (this.pendingMessages.TryGetValue(messageId.ToString(), out pending))
                {
                    this.pendingMessages.Remove(messageId.ToString());
                    telemetry = pending.Telemetry;
                    pending.Dispose();
                    return true;
                }
            }

            return false;
        }

        public void Remove(UniqueId messageId)
        {
            DependencyTelemetry telemetry;
            this.TryLookup(messageId, out telemetry);
        }

        public void Dispose()
        {
            lock (this.lockObject)
            {
                foreach (var obj in this.pendingMessages.Values)
                {
                    obj.Dispose();
                }

                this.pendingMessages.Clear();
                this.disposed = true;
            }
        }

        private void OnTimeout(object state)
        {
            var pending = (PendingMessage)state;
            this.Remove(pending.Id);
            this.timeoutCallback?.Invoke(pending.Id, pending.Telemetry);
        }

        private class PendingMessage : IDisposable
        {
            public PendingMessage(UniqueId id, DependencyTelemetry telemetry)
            {
                if (id == null)
                {
                    throw new ArgumentNullException(nameof(id));
                }

                if (telemetry == null)
                {
                    throw new ArgumentNullException(nameof(telemetry));
                }

                this.Id = id;
                this.Telemetry = telemetry;
            }

            public UniqueId Id { get; private set; }

            public DependencyTelemetry Telemetry { get; private set; }

            public Timer TimeoutTimer { get; private set; }

            public void Start(TimeSpan timeout, TimerCallback callback)
            {
                if (this.TimeoutTimer != null)
                {
                    throw new InvalidOperationException("Start cannot be called twice");
                }

                if (timeout > TimeSpan.Zero)
                {
                    this.TimeoutTimer = new Timer(callback, this, timeout, TimeSpan.FromMilliseconds(-1));
                }
            }

            public void Dispose()
            {
                if (this.TimeoutTimer != null)
                {
                    this.TimeoutTimer.Dispose();
                    this.TimeoutTimer = null;
                }
            }
        }
    }
}
