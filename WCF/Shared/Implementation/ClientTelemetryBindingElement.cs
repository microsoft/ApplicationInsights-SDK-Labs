using System;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal sealed class ClientTelemetryBindingElement : BindingElement
    {
        private TelemetryClient telemetryClient;
        private Type contractType;
        private ClientOperationMap operationMap;

        public String RootOperationIdHeaderName { get; set; }
        public String ParentOperationIdHeaderName { get; set; }
        public String SoapRootOperationIdHeaderName { get; set; }
        public String SoapParentOperationIdHeaderName { get; set; }
        public String SoapHeaderNamespace { get; set; }

        public ClientTelemetryBindingElement(TelemetryClient client, Type contract, ClientOperationMap map)
        {
            if ( client == null )
            {
                throw new ArgumentNullException(nameof(client));
            }
            if ( contract == null )
            {
                throw new ArgumentNullException(nameof(contract));
            }
            if ( map == null )
            {
                throw new ArgumentNullException(nameof(map));
            }
            this.telemetryClient = client;
            this.contractType = contract;
            this.operationMap = map;
        }

        public override BindingElement Clone()
        {
            return new ClientTelemetryBindingElement(telemetryClient, contractType, operationMap);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return context.GetInnerProperty<T>();
        }


        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if ( context == null )
            {
                throw new ArgumentNullException(nameof(context));
            }
            if ( IsSupportedChannelShape(typeof(TChannel)) )
            {
                return context.CanBuildInnerChannelFactory<TChannel>();
            }
            return false;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if ( context == null )
            {
                throw new ArgumentNullException(nameof(context));
            }
            if ( !IsSupportedChannelShape(typeof(TChannel)) )
            {
                throw new InvalidOperationException("Unsupported channel shape: " + typeof(TChannel));
            }
            var innerFactory = context.BuildInnerChannelFactory<TChannel>();
            var factory = new ClientTelemetryChannelFactory<TChannel>(context.Binding, innerFactory, telemetryClient, contractType, operationMap);
            factory.RootOperationIdHeaderName = RootOperationIdHeaderName;
            factory.ParentOperationIdHeaderName = ParentOperationIdHeaderName;
            factory.SoapRootOperationIdHeaderName = SoapRootOperationIdHeaderName;
            factory.SoapParentOperationIdHeaderName = SoapParentOperationIdHeaderName;
            factory.SoapHeaderNamespace = SoapHeaderNamespace;

            return factory;
        }

        private bool IsSupportedChannelShape(Type type)
        {
            if ( type == typeof(IRequestChannel) || type == typeof(IRequestSessionChannel) )
            {
                return true;
            }
            if ( type == typeof(IOutputChannel) || type == typeof(IOutputSessionChannel) )
            {
                return true;
            }
            if ( type == typeof(IDuplexChannel) || type == typeof(IDuplexSessionChannel) )
            {
                return true;
            }
            return false;
        }

    }
}
