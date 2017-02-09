using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class ClientTelemetryChannel : IRequestChannel
    {
        private IRequestChannel innerChannel;
        private TelemetryClient telemetryClient;
        private Type contractType;
        private ClientOperationMap operationMap;

        public EndpointAddress RemoteAddress { get { return innerChannel.RemoteAddress; } }

        public CommunicationState State { get { return innerChannel.State; } }

        public Uri Via { get { return innerChannel.Via; } }

        public event EventHandler Closed;
        public event EventHandler Closing;
        public event EventHandler Faulted;
        public event EventHandler Opened;
        public event EventHandler Opening;

        public ClientTelemetryChannel(TelemetryClient client, IRequestChannel channel, Type contractType, ClientOperationMap map)
        {
            if ( client == null )
            {
                throw new ArgumentNullException(nameof(client));
            }
            if ( channel == null )
            {
                throw new ArgumentNullException(nameof(channel));
            }
            if ( contractType == null )
            {
                throw new ArgumentNullException(nameof(contractType));
            }
            if ( map == null )
            {
                throw new ArgumentNullException(nameof(map));
            }
            this.telemetryClient = client;
            this.innerChannel = channel;
            this.contractType = contractType;
            this.operationMap = map;
        }

        public void Open()
        {
            HookChannelEvents();
            var telemetry = this.StartOpenTelemetry(nameof(Open));
            try
            {
                innerChannel.Open();
                this.StopOpenTelemetry(telemetry, null, nameof(Open));
            } catch ( Exception ex )
            {
                this.StopOpenTelemetry(telemetry, ex, nameof(Open));
                throw;
            }
        }

        public void Open(TimeSpan timeout)
        {
            HookChannelEvents();
            var telemetry = this.StartOpenTelemetry(nameof(Open));
            try
            {
                innerChannel.Open(timeout);
                this.StopOpenTelemetry(telemetry, null, nameof(Open));
            } catch ( Exception ex )
            {
                this.StopOpenTelemetry(telemetry, ex, nameof(Open));
                throw;
            }
        }

        public void Close()
        {
            try
            {
                innerChannel.Close();
            } finally
            {
                UnhookChannelEvents();
            }
        }

        public void Close(TimeSpan timeout)
        {
            try
            {
                innerChannel.Close(timeout);
            } finally
            {
                UnhookChannelEvents();
            }
        }

        public void Abort()
        {
            try
            {
                innerChannel.Abort();
            } finally
            {
                UnhookChannelEvents();
            }
        }

        public T GetProperty<T>() where T : class
        {
            return innerChannel.GetProperty<T>();
        }

        public Message Request(Message message)
        {
            var telemetry = StartSendTelemetry(message, nameof(Request));
            try
            {
                var response = innerChannel.Request(message);
                StopSendTelemetry(telemetry, response, null, nameof(message));
                return response;
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(message));
                throw;
            }
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            var telemetry = StartSendTelemetry(message, nameof(Request));
            try
            {
                var response = innerChannel.Request(message, timeout);
                StopSendTelemetry(telemetry, response, null, nameof(message));
                return response;
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(message));
                throw;
            }
        }


        //
        // Async Methods
        //
        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            HookChannelEvents();
            var telemetry = StartOpenTelemetry(nameof(BeginOpen));
            try
            {
                var result = innerChannel.BeginOpen(callback, state);
                return new NestedAsyncResult(result, telemetry);
            } catch ( Exception ex )
            {
                StopOpenTelemetry(telemetry, ex, nameof(BeginOpen));
                throw;
            }
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            HookChannelEvents();
            var telemetry = StartOpenTelemetry(nameof(BeginOpen));
            try
            {
                var result = innerChannel.BeginOpen(timeout, callback, state);
                return new NestedAsyncResult(result, telemetry);
            } catch ( Exception ex )
            {
                StopOpenTelemetry(telemetry, ex, nameof(BeginOpen));
                throw;
            }
        }

        public void EndOpen(IAsyncResult result)
        {
            NestedAsyncResult nar = (NestedAsyncResult)result;
            try
            {
                innerChannel.EndOpen(nar.Inner);
                StopOpenTelemetry(nar.Telemetry, null, nameof(EndOpen));
            } catch ( Exception ex )
            {
                StopOpenTelemetry(nar.Telemetry, ex, nameof(EndOpen));
                throw;
            }
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return innerChannel.BeginClose(callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return innerChannel.BeginClose(timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            try
            {
                innerChannel.EndClose(result);
            } finally
            {
                UnhookChannelEvents();
            }
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            var telemetry = StartSendTelemetry(message, nameof(BeginRequest));
            try
            {
                var result = innerChannel.BeginRequest(message, callback, state);
                return new NestedAsyncResult(result, telemetry);
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(BeginRequest));
                throw;
            }
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            var telemetry = StartSendTelemetry(message, nameof(BeginRequest));
            try
            {
                var result = innerChannel.BeginRequest(message, timeout, callback, state);
                return new NestedAsyncResult(result, telemetry);
            } catch ( Exception ex )
            {
                StopSendTelemetry(telemetry, null, ex, nameof(BeginRequest));
                throw;
            }
        }

        public Message EndRequest(IAsyncResult result)
        {
            var nar = (NestedAsyncResult)result;
            try
            {
                var response = innerChannel.EndRequest(nar.Inner);
                StopSendTelemetry(nar.Telemetry, response, null, nameof(EndRequest));
                return response;
            } catch ( Exception ex )
            {
                StopSendTelemetry(nar.Telemetry, null, ex, nameof(EndRequest));
                throw;
            }
        }


        private void HookChannelEvents()
        {
            innerChannel.Closed += OnChannelClosed;
            innerChannel.Closing += OnChannelClosing;
            innerChannel.Opened += OnChannelOpened;
            innerChannel.Opening += OnChannelOpening;
            innerChannel.Faulted += OnChannelFaulted;
        }
        private void UnhookChannelEvents()
        {
            innerChannel.Closed -= OnChannelClosed;
            innerChannel.Closing -= OnChannelClosing;
            innerChannel.Opened -= OnChannelOpened;
            innerChannel.Opening -= OnChannelOpening;
            innerChannel.Faulted -= OnChannelFaulted;
        }
        private void OnChannelOpened(object sender, EventArgs e)
        {
            Opened?.Invoke(this, e);
        }
        private void OnChannelOpening(object sender, EventArgs e)
        {
            Opening?.Invoke(this, e);
        }
        private void OnChannelClosed(object sender, EventArgs e)
        {
            Closed?.Invoke(sender, e);
        }
        private void OnChannelClosing(object sender, EventArgs e)
        {
            Closing?.Invoke(sender, e);
        }
        private void OnChannelFaulted(object sender, EventArgs e)
        {
            Faulted?.Invoke(sender, e);
        }


        // telemetry implementation
        private DependencyTelemetry StartOpenTelemetry(String method)
        {
            try
            {
                var telemetry = new DependencyTelemetry();
                telemetry.Start();
                telemetry.Type = DependencyConstants.WcfChannelOpen;
                telemetry.Target = RemoteAddress.Uri.Host;
                telemetry.Name = RemoteAddress.Uri.ToString();
                telemetry.Data = contractType.FullName;
                return telemetry;
            } catch ( Exception ex )
            {
                WcfEventSource.Log.ClientInspectorError(method, ex.ToString());
                return null;
            }
        }
        private void StopOpenTelemetry(DependencyTelemetry telemetry, Exception error, String method)
        {
            if ( telemetry == null )
            {
                return;
            }
            try
            {
                telemetry.Success = error == null;
                telemetry.Stop();
                telemetryClient.TrackDependency(telemetry);
            } catch ( Exception ex )
            {
                WcfEventSource.Log.ClientInspectorError(method, ex.ToString());
            }
        }

        private DependencyTelemetry StartSendTelemetry(Message request, String method)
        {
            var soapAction = request.Headers.Action;
            ClientOpDescription operation;
            if ( !this.operationMap.TryLookupByAction(soapAction, out operation) )
            {
                return null;
            }

            try
            {
                var telemetry = new DependencyTelemetry();
                telemetry.Start();
                telemetry.Type = DependencyConstants.WcfClientCall;
                telemetry.Target = RemoteAddress.Uri.Host;
                telemetry.Name = RemoteAddress.Uri.ToString();
                telemetry.Data = contractType.Name + "." + operation.Name;
                telemetry.Properties["soapAction"] = soapAction;
                return telemetry;
            } catch ( Exception ex )
            {
                WcfEventSource.Log.ClientInspectorError(method, ex.ToString());
                return null;
            }
        }
        private void StopSendTelemetry(DependencyTelemetry telemetry, Message response, Exception error, String method)
        {
            if ( telemetry == null )
            {
                return;
            }
            try
            {
                if ( error != null )
                {
                    telemetry.Success = false;
                }
                if ( response != null && response.IsFault )
                {
                    telemetry.Success = false;
                }
                telemetry.Stop();
                telemetryClient.TrackDependency(telemetry);
            } catch ( Exception ex )
            {
                WcfEventSource.Log.ClientInspectorError(method, ex.ToString());
            }
        }

        class NestedAsyncResult : IAsyncResult
        {

            public object AsyncState { get { return Inner.AsyncState; } }

            public WaitHandle AsyncWaitHandle { get { return Inner.AsyncWaitHandle; } }

            public bool CompletedSynchronously { get { return Inner.CompletedSynchronously; } }

            public bool IsCompleted { get { return Inner.IsCompleted; } }

            public IAsyncResult Inner { get; private set; }
            public DependencyTelemetry Telemetry { get; private set; }

            public NestedAsyncResult(IAsyncResult innerResult, DependencyTelemetry telemetry)
            {
                this.Inner = innerResult;
                this.Telemetry = telemetry;
            }
        }
    }
}
