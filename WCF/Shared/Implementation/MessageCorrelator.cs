using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Xml;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal sealed class MessageCorrelator : IDisposable
    {
        private Dictionary<String, PendingMessage> pendingMessages;
        private Action<UniqueId, DependencyTelemetry> timeoutCallback;
        private object lockObject;
        private bool disposed;

        public MessageCorrelator(Action<UniqueId, DependencyTelemetry> callback = null)
        {
            this.timeoutCallback = callback;
            this.pendingMessages = new Dictionary<String, PendingMessage>();
            this.lockObject = new object();
            this.disposed = false;
        }

        public void Add(UniqueId messageId, DependencyTelemetry telemetry, TimeSpan timeout)
        {
            if ( messageId == null )
            {
                throw new ArgumentNullException(nameof(messageId));
            }
            if ( telemetry == null )
            {
                throw new ArgumentNullException(nameof(telemetry));
            }
            lock ( lockObject )
            {
                if ( disposed )
                {
                    throw new ObjectDisposedException(nameof(MessageCorrelator));
                }
                var pending = new PendingMessage(messageId, telemetry);
                pendingMessages[messageId.ToString()] = pending;
                pending.Start(timeout, this.OnTimeout);
            }
        }

        public bool TryLookup(UniqueId messageId, out DependencyTelemetry telemetry)
        {
            telemetry = null;
            lock ( this.lockObject )
            {
                PendingMessage pending = null;
                if ( pendingMessages.TryGetValue(messageId.ToString(), out pending) )
                {
                    pendingMessages.Remove(messageId.ToString());
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
            TryLookup(messageId, out telemetry);
        }

        public void Dispose()
        {
            lock ( this.lockObject )
            {
                foreach ( var obj in pendingMessages.Values )
                {
                    obj.Dispose();
                }
                pendingMessages.Clear();
                disposed = true;
            }
        }

        private void OnTimeout(object state)
        {
            var pending = (PendingMessage)state;
            Remove(pending.Id);
            this.timeoutCallback?.Invoke(pending.Id, pending.Telemetry);
        }

        private class PendingMessage : IDisposable
        {
            public UniqueId Id { get; private set; }
            public DependencyTelemetry Telemetry { get; private set; }
            public Timer TimeoutTimer { get; private set; }

            public PendingMessage(UniqueId id, DependencyTelemetry telemetry)
            {
                if ( id == null )
                {
                    throw new ArgumentNullException(nameof(id));
                }
                if ( telemetry == null )
                {
                    throw new ArgumentNullException(nameof(telemetry));
                }
                this.Id = id;
                this.Telemetry = telemetry;
            }

            public void Start(TimeSpan timeout, TimerCallback callback)
            {
                if ( TimeoutTimer != null )
                {
                    throw new InvalidOperationException("Start cannot be called twice");
                }
                if ( timeout > TimeSpan.Zero )
                {
                    this.TimeoutTimer = new Timer(callback, this, TimeSpan.Zero, timeout);
                }
            }
            public void Dispose()
            {
                if ( TimeoutTimer != null )
                {
                    TimeoutTimer.Dispose();
                    TimeoutTimer = null;
                }
            }
        }
    }
}
