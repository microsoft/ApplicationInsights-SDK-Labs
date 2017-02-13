using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal sealed class ClientTelemetryChannelFactory<TChannel> : ChannelFactoryBase<TChannel>, IChannelManager
    {
        private IChannelFactory<TChannel> innerFactory;
        public TelemetryClient TelemetryClient { get; private set; }
        public Type ContractType { get; private set; }
        public ClientOperationMap OperationMap { get; private set; }

        public ClientTelemetryChannelFactory(Binding binding, IChannelFactory<TChannel> factory, TelemetryClient client, Type contractType, ClientOperationMap map)
            : base(binding)
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
            this.TelemetryClient = client;
            this.ContractType = contractType;
            this.OperationMap = map;
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
            if ( result == null )
            {
                throw new ArgumentNullException(nameof(result));
            }
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
            if ( result == null )
            {
                throw new ArgumentNullException(nameof(result));
            }
            this.innerFactory.EndClose(result);
        }
        protected override void OnAbort()
        {
            this.innerFactory.Abort();
        }

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            if ( address == null )
            {
                throw new ArgumentNullException(nameof(address));
            }

            var channel = this.innerFactory.CreateChannel(address, via);
            IChannel newChannel = null;
            if ( typeof(TChannel) == typeof(IRequestChannel) || typeof(TChannel) == typeof(IRequestSessionChannel) )
            {
                newChannel = new ClientTelemetryRequestChannel(this, (IChannel)channel);
            } else if ( typeof(TChannel) == typeof(IOutputChannel) || typeof(TChannel) == typeof(IOutputSessionChannel) )
            {
                newChannel = new ClientTelemetryOutputChannel(this, (IChannel)channel);
            } else if ( typeof(TChannel) == typeof(IDuplexChannel) || typeof(TChannel) == typeof(IDuplexSessionChannel) )
            {
                newChannel = new ClientTelemetryDuplexChannel(this, (IChannel)channel);
            } else
            {
                throw new NotSupportedException("Channel shape is not supported: " + typeof(TChannel));
            }
            return (TChannel)(object)newChannel;
        }

    }
}
