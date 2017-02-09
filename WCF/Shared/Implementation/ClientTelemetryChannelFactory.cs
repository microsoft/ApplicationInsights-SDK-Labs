using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class ClientTelemetryChannelFactory<TChannel> : ChannelFactoryBase<TChannel>
    {
        private IChannelFactory<TChannel> innerFactory;
        private TelemetryClient telemetryClient;
        private Type contractType;
        private ClientOperationMap operationMap;

        public ClientTelemetryChannelFactory(IChannelFactory<TChannel> factory, TelemetryClient client, Type contractType, ClientOperationMap map)
        {
            if ( factory == null )
            {
                throw new ArgumentNullException(nameof(factory));
            }
            if ( client == null )
            {
                throw new ArgumentNullException(nameof(client));
            }
            if ( contractType == null )
            {
                throw new ArgumentNullException(nameof(contractType));
            }
            if ( map == null )
            {
                throw new ArgumentNullException(nameof(map));
            }
            this.innerFactory = factory;
            this.telemetryClient = client;
            this.contractType = contractType;
            this.operationMap = map;
        }

        public override T GetProperty<T>()
        {
            return innerFactory.GetProperty<T>();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerFactory.Open(timeout);
        }
        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerFactory.BeginOpen(timeout, callback, state);
        }
        protected override void OnEndOpen(IAsyncResult result)
        {
            this.innerFactory.EndOpen(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.innerFactory.Close(timeout);
        }
        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.innerFactory.BeginClose(timeout, callback, state);
        }
        protected override void OnEndClose(IAsyncResult result)
        {
            this.innerFactory.EndClose(result);
        }
        protected override void OnAbort()
        {
            this.innerFactory.Abort();
        }

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            var channel = this.innerFactory.CreateChannel(address, via);
            var newChannel = new ClientTelemetryRequestChannel(telemetryClient, (IRequestChannel)channel, contractType, operationMap);
            return (TChannel)(object)newChannel;
        }

    }
}
