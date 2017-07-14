namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.ApplicationInsights.DataContracts;

    internal abstract class ClientTelemetryChannelBase : IDisposable
    {
        public ClientTelemetryChannelBase(IChannelManager channelManager, IChannel channel)
        {
            if (channelManager == null)
            {
                throw new ArgumentNullException(nameof(channelManager));
            }

            if (channel == null)
            {
                throw new ArgumentNullException(nameof(channel));
            }

            this.ChannelManager = channelManager;
            this.InnerChannel = channel;
        }

        public event EventHandler Closed;

        public event EventHandler Closing;

        public event EventHandler Faulted;

        public event EventHandler Opened;

        public event EventHandler Opening;

        public CommunicationState State
        {
            get { return this.InnerChannel.State; }
        }

        public abstract EndpointAddress RemoteAddress { get; }

        protected IChannel InnerChannel { get; private set; }

        protected IChannelManager ChannelManager { get; private set; }

        public void Open()
        {
            this.Open(this.ChannelManager.OpenTimeout);
        }

        public void Open(TimeSpan timeout)
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.Open));
            this.HookChannelEvents();
            var telemetry = this.StartOpenTelemetry(nameof(this.Open));
            try
            {
                this.InnerChannel.Open(timeout);

                if (!this.ChannelManager.IgnoreChannelEvents)
                {
                    this.StopOpenTelemetry(telemetry, null, nameof(this.Open));
                }
            }
            catch (Exception ex)
            {
                // if an exception happened, we still want to report it
                this.StopOpenTelemetry(telemetry, ex, nameof(this.Open));
                throw;
            }
        }

        public void Close()
        {
            this.Close(this.ChannelManager.CloseTimeout);
        }

        public void Close(TimeSpan timeout)
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.Close));
            try
            {
                this.InnerChannel.Close(timeout);
            }
            finally
            {
                this.Dispose(true);
            }
        }

        public void Abort()
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.Abort));
            try
            {
                this.InnerChannel.Abort();
            }
            finally
            {
                this.Dispose(true);
            }
        }

        public T GetProperty<T>() where T : class
        {
            return this.InnerChannel.GetProperty<T>();
        }

        // -------------------------------------
        // Async Methods
        // -------------------------------------
        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            return this.BeginOpen(this.ChannelManager.OpenTimeout, callback, state);
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.BeginOpen));
            this.HookChannelEvents();
            var telemetry = this.StartOpenTelemetry(nameof(this.BeginOpen));
            try
            {
                return new OpenAsyncResult(this.InnerChannel, timeout, this.OpenCompleted, callback, state, telemetry);
            }
            catch (Exception ex)
            {
                this.StopOpenTelemetry(telemetry, ex, nameof(this.BeginOpen));
                throw;
            }
        }

        public void EndOpen(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.EndOpen));
            OpenAsyncResult.End<OpenAsyncResult>(result);
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            return this.BeginClose(this.ChannelManager.CloseTimeout, callback, state);
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.BeginClose));
            return this.InnerChannel.BeginClose(timeout, callback, state);
        }

        public void EndClose(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            WcfClientEventSource.Log.ChannelCalled(GetType().FullName, nameof(this.EndClose));
            try
            {
                this.InnerChannel.EndClose(result);
            }
            finally
            {
                this.Dispose(true);
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
        }

        // telemetry implementation
        protected DependencyTelemetry StartSendTelemetry(Message request, string method)
        {
            var soapAction = request.Headers.Action;
            ClientOperation operation;
            if (!this.ChannelManager.OperationMap.TryLookupByAction(soapAction, out operation))
            {
                return null;
            }

            try
            {
                var telemetry = new DependencyTelemetry();
                this.ChannelManager.TelemetryClient.Initialize(telemetry);
                telemetry.Start();
                telemetry.Type = DependencyConstants.WcfClientCall;
                telemetry.Target = this.RemoteAddress.Uri.Host;
                telemetry.Data = this.RemoteAddress.Uri.ToString();
                telemetry.Name = operation.Name;
                telemetry.Properties[DependencyConstants.SoapActionProperty] = soapAction;
                if (operation.IsOneWay)
                {
                    telemetry.Properties[DependencyConstants.IsOneWayProperty] = bool.TrueString;
                }

                this.SetCorrelationHeaders(telemetry, request);
                return telemetry;
            }
            catch (Exception ex)
            {
                WcfClientEventSource.Log.ClientTelemetryError(method, ex.ToString());
                return null;
            }
        }

        protected void StopSendTelemetry(DependencyTelemetry telemetry, Message response, Exception error, string method)
        {
            if (telemetry == null)
            {
                return;
            }

            try
            {
                if (error != null)
                {
                    telemetry.Success = false;
                    telemetry.ResultCode = error.ToResultCode();
                }

                if (response != null && response.IsFault)
                {
                    telemetry.Success = false;
                    telemetry.ResultCode = "SoapFault";
                }

                telemetry.Stop();
                this.ChannelManager.TelemetryClient.TrackDependency(telemetry);
            }
            catch (Exception ex)
            {
                WcfClientEventSource.Log.ClientTelemetryError(method, ex.ToString());
            }
        }

        protected bool IsOneWay(DependencyTelemetry telemetry)
        {
            string value;
            if (telemetry.Properties.TryGetValue(DependencyConstants.IsOneWayProperty, out value))
            {
                return bool.Parse(value);
            }

            return false;
        }

        protected void Dispose(bool disposing)
        {
            this.UnhookChannelEvents();
            this.OnClosed();
        }

        protected virtual void OnClosed()
        {
        }

        private static void SetSoapHeader(Message message, string soapNS, string header, string value)
        {
            var currentHeader = message.Headers.FindHeader(header, soapNS);
            if (currentHeader < 0)
            {
                var soapHeader = MessageHeader.CreateHeader(header, soapNS, value);
                message.Headers.Add(soapHeader);
            }
        }

        private static string ValueOrDefault(string value, string defaultValue)
        {
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        private void HookChannelEvents()
        {
            this.InnerChannel.Closed += this.OnChannelClosed;
            this.InnerChannel.Closing += this.OnChannelClosing;
            this.InnerChannel.Opened += this.OnChannelOpened;
            this.InnerChannel.Opening += this.OnChannelOpening;
            this.InnerChannel.Faulted += this.OnChannelFaulted;
        }

        private void UnhookChannelEvents()
        {
            this.InnerChannel.Closed -= this.OnChannelClosed;
            this.InnerChannel.Closing -= this.OnChannelClosing;
            this.InnerChannel.Opened -= this.OnChannelOpened;
            this.InnerChannel.Opening -= this.OnChannelOpening;
            this.InnerChannel.Faulted -= this.OnChannelFaulted;
        }

        private void OpenCompleted(IAsyncResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var oar = (OpenAsyncResult)result;
            bool sendTelemetry = !this.ChannelManager.IgnoreChannelEvents;
            if (oar.LastException != null)
            {
                sendTelemetry = true;
            }

            if (sendTelemetry)
            {
                this.StopOpenTelemetry(oar.Telemetry, oar.LastException, nameof(this.OpenCompleted));
            }
        }

        private void OnChannelOpened(object sender, EventArgs e)
        {
            this.Opened?.Invoke(this, e);
        }

        private void OnChannelOpening(object sender, EventArgs e)
        {
            this.Opening?.Invoke(this, e);
        }

        private void OnChannelClosed(object sender, EventArgs e)
        {
            this.Closed?.Invoke(this, e);
        }

        private void OnChannelClosing(object sender, EventArgs e)
        {
            this.Closing?.Invoke(this, e);
        }

        private void OnChannelFaulted(object sender, EventArgs e)
        {
            this.Faulted?.Invoke(this, e);
        }

        private DependencyTelemetry StartOpenTelemetry(string method)
        {
            try
            {
                var telemetry = new DependencyTelemetry();
                telemetry.Start();
                telemetry.Type = DependencyConstants.WcfChannelOpen;
                telemetry.Target = this.RemoteAddress.Uri.Host;
                telemetry.Data = this.RemoteAddress.Uri.ToString();
                telemetry.Name = this.ChannelManager.OperationMap.ContractType.Name;
                return telemetry;
            }
            catch (Exception ex)
            {
                WcfClientEventSource.Log.ClientTelemetryError(method, ex.ToString());
                return null;
            }
        }

        private void StopOpenTelemetry(DependencyTelemetry telemetry, Exception error, string method)
        {
            if (telemetry == null)
            {
                return;
            }

            try
            {
                telemetry.Success = error == null;
                telemetry.Stop();
                this.ChannelManager.TelemetryClient.TrackDependency(telemetry);
            }
            catch (Exception ex)
            {
                WcfClientEventSource.Log.ClientTelemetryError(method, ex.ToString());
            }
        }

        private void SetCorrelationHeaders(DependencyTelemetry telemetry, Message message)
        {
            var httpHeaders = message.GetHttpRequestHeaders();

            var rootIdHttpHeader = ValueOrDefault(this.ChannelManager.RootOperationIdHeaderName, CorrelationHeaders.HttpStandardRootIdHeader);
            var rootIdSoapHeader = ValueOrDefault(this.ChannelManager.SoapRootOperationIdHeaderName, CorrelationHeaders.SoapStandardRootIdHeader);
            var parentIdHttpHeader = ValueOrDefault(this.ChannelManager.ParentOperationIdHeaderName, CorrelationHeaders.HttpStandardParentIdHeader);
            var parentIdSoapHeader = ValueOrDefault(this.ChannelManager.SoapParentOperationIdHeaderName, CorrelationHeaders.SoapStandardParentIdHeader);

            // "" is a valid value for the namespace
            var soapNS = this.ChannelManager.SoapHeaderNamespace ?? CorrelationHeaders.SoapStandardNamespace;

            var rootId = telemetry.Context.Operation.Id;
            if (!string.IsNullOrEmpty(rootId))
            {
                httpHeaders.Headers[rootIdHttpHeader] = rootId;
                SetSoapHeader(message, soapNS, rootIdSoapHeader, rootId);
            }

            var parentId = telemetry.Id;
            if (!string.IsNullOrEmpty(parentId))
            {
                httpHeaders.Headers[parentIdHttpHeader] = parentId;
                SetSoapHeader(message, soapNS, parentIdSoapHeader, parentId);
            }
        }
    }
}
