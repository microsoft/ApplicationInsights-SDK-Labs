namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel.Channels;

    internal sealed class ClientTelemetryBindingElement : BindingElement
    {
        private TelemetryClient telemetryClient;
        private ClientContract operationMap;

        public ClientTelemetryBindingElement(TelemetryClient client, ClientContract map)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            if (map == null)
            {
                throw new ArgumentNullException(nameof(map));
            }

            this.telemetryClient = client;
            this.operationMap = map;
        }

        public string RootOperationIdHeaderName { get; set; }

        public string ParentOperationIdHeaderName { get; set; }

        public string SoapRootOperationIdHeaderName { get; set; }

        public string SoapParentOperationIdHeaderName { get; set; }

        public string SoapHeaderNamespace { get; set; }
        
        public bool IgnoreChannelEvents { get; set; }

        public override BindingElement Clone()
        {
            return new ClientTelemetryBindingElement(this.telemetryClient, this.operationMap);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            return context.GetInnerProperty<T>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (this.IsSupportedChannelShape(typeof(TChannel)))
            {
                return context.CanBuildInnerChannelFactory<TChannel>();
            }

            return false;
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!this.IsSupportedChannelShape(typeof(TChannel)))
            {
                throw new InvalidOperationException("Unsupported channel shape: " + typeof(TChannel));
            }

            var innerFactory = context.BuildInnerChannelFactory<TChannel>();
            var factory = new ClientTelemetryChannelFactory<TChannel>(context.Binding, innerFactory, this.telemetryClient, this.operationMap)
            {
                RootOperationIdHeaderName = this.RootOperationIdHeaderName,
                ParentOperationIdHeaderName = this.ParentOperationIdHeaderName,
                SoapRootOperationIdHeaderName = this.SoapRootOperationIdHeaderName,
                SoapParentOperationIdHeaderName = this.SoapParentOperationIdHeaderName,
                SoapHeaderNamespace = this.SoapHeaderNamespace,
                IgnoreChannelEvents = this.IgnoreChannelEvents
            };
            return factory;
        }

        private bool IsSupportedChannelShape(Type type)
        {
            if (type == typeof(IRequestChannel) || type == typeof(IRequestSessionChannel))
            {
                return true;
            }

            if (type == typeof(IOutputChannel) || type == typeof(IOutputSessionChannel))
            {
                return true;
            }

            if (type == typeof(IDuplexChannel) || type == typeof(IDuplexSessionChannel))
            {
                return true;
            }

            return false;
        }
    }
}
