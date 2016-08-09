using System;
using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Integration
{
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
            binding = new NetTcpBinding();
            baseAddress = new Uri(String.Format(CultureInfo.InvariantCulture, "net.tcp://localhost:57669/{0}/", Guid.NewGuid()));

            host = new ServiceHost(typeof(TServiceImpl), baseAddress);
            endpoint = host.AddServiceEndpoint(typeof(TServiceIFace), binding, "svc");
        }

        public String GetServiceAddress()
        {
            return endpoint.Address.Uri.ToString();
        }

        public HostingContext<TServiceImpl, TServiceIFace> IncludeDetailsInFaults()
        {
            ServiceDebugBehavior sdb = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if ( sdb == null )
            {
                sdb = new ServiceDebugBehavior();
                host.Description.Behaviors.Add(sdb);
            }
            sdb.IncludeExceptionDetailInFaults = true;
            return this;
        }

        public HostingContext<TServiceImpl, TServiceIFace> ShouldWaitForCompletion()
        {
            completionWait = new EventWaitHandle(false, EventResetMode.ManualReset);
            endpoint.Behaviors.Add(new CompletionBehavior(completionWait));
            return this;
        }

        public HostingContext<TServiceImpl, TServiceIFace> ExpectFailure()
        {
            this.expectFailure = true;
            return this;
        }

        public void Open()
        {
            host.Open();

            channelFactory = new ChannelFactory<TServiceIFace>(binding, new Uri(baseAddress, new Uri("svc", UriKind.Relative)).ToString());
            channelFactory.Open();
        }

        public TServiceIFace GetChannel()
        {
            if ( channel == null )
            {
                channel = channelFactory.CreateChannel();
            }
            return channel;
        }

        public void Dispose()
        {
            if ( channel != null )
            {
                Dispose((ICommunicationObject)channel);
                channel = default(TServiceIFace);
            }
            if ( channelFactory != null )
            {
                Dispose(channelFactory);
                channelFactory = null;
            }
            if ( host != null )
            {
                Dispose(host);
                host = null;
            }
            // WCF calls IErrorHandler on a background there
            // by the time we've processed the reply and the
            // ServiceHost has closed, it might not have gotten
            // around to calling IErrorHander.HandleError()

            if ( completionWait != null )
                completionWait.WaitOne();
        }

        private void Dispose(ICommunicationObject obj)
        {
            if ( expectFailure )
            {
                obj.Abort();
                return;
            }
            switch ( obj.State )
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
                completion = handle;
            }

            public bool HandleError(Exception error)
            {
                completion.Set();
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
