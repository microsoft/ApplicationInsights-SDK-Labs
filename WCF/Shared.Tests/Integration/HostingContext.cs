namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    public sealed class HostingContext<TServiceImpl, TServiceIFace> : IDisposable
        where TServiceImpl : TServiceIFace
    {
        private ServiceHost host;
        private ChannelFactory<TServiceIFace> channelFactory;
        private ServiceEndpoint endpoint;
        private TServiceIFace channel;
        private NetTcpBinding binding;
        private Uri baseAddress;
        private EventWaitHandle completionWait;
        private bool expectFailure;

        public HostingContext()
        {
            this.binding = new NetTcpBinding();
            this.baseAddress = new Uri(string.Format(CultureInfo.InvariantCulture, "net.tcp://localhost:57669/{0}/", Guid.NewGuid()));

            this.host = new ServiceHost(typeof(TServiceImpl), this.baseAddress);
            this.endpoint = this.host.AddServiceEndpoint(typeof(TServiceIFace), this.binding, "svc");
        }

        public string GetServiceAddress()
        {
            return new Uri(this.baseAddress, new Uri("svc", UriKind.Relative)).ToString(); // endpoint.Address.Uri.ToString();
        }

        public HostingContext<TServiceImpl, TServiceIFace> IncludeDetailsInFaults()
        {
            var sdb = this.host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (sdb == null)
            {
                sdb = new ServiceDebugBehavior();
                this.host.Description.Behaviors.Add(sdb);
            }

            sdb.IncludeExceptionDetailInFaults = true;
            return this;
        }

        public HostingContext<TServiceImpl, TServiceIFace> ShouldWaitForCompletion()
        {
            this.completionWait = new EventWaitHandle(false, EventResetMode.ManualReset);
            this.endpoint.Behaviors.Add(new CompletionBehavior(this.completionWait));
            return this;
        }

        public HostingContext<TServiceImpl, TServiceIFace> ExpectFailure()
        {
            this.expectFailure = true;
            return this;
        }

        public void Open()
        {
            this.host.Open();

            this.channelFactory = new ChannelFactory<TServiceIFace>(this.binding, this.GetServiceAddress());
            this.channelFactory.Open();
        }

        public TServiceIFace GetChannel()
        {
            if (this.channel == null)
            {
                this.channel = this.channelFactory.CreateChannel();
            }

            return this.channel;
        }

        public void Dispose()
        {
            if (this.channel != null)
            {
                this.Dispose((ICommunicationObject)this.channel);
                this.channel = default(TServiceIFace);
            }

            if (this.channelFactory != null)
            {
                this.Dispose(this.channelFactory);
                this.channelFactory = null;
            }

            if (this.host != null)
            {
                this.Dispose(this.host);
                this.host = null;
            }

            // WCF calls IErrorHandler on a background there
            // by the time we've processed the reply and the
            // ServiceHost has closed, it might not have gotten
            // around to calling IErrorHander.HandleError()
            if (this.completionWait != null)
            {
                this.completionWait.WaitOne();
            }
        }

        private void Dispose(ICommunicationObject obj)
        {
            if (this.expectFailure)
            {
                obj.Abort();
                return;
            }

            switch (obj.State)
            {
                case CommunicationState.Created:
                case CommunicationState.Opened:
                    obj.Close();
                    break;
                default:
                    obj.Abort();
                    break;
            }
        }

        private class CompletionBehavior : IErrorHandler, IEndpointBehavior
        {
            private EventWaitHandle completion;

            public CompletionBehavior(EventWaitHandle handle)
            {
                this.completion = handle;
            }

            public bool HandleError(Exception error)
            {
                this.completion.Set();
                return false;
            }

            public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
            {
            }

            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
            {
            }

            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
            }

            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
            {
                endpointDispatcher.ChannelDispatcher.ErrorHandlers.Add(this);
            }

            public void Validate(ServiceEndpoint endpoint)
            {
            }
        }
    }
}
