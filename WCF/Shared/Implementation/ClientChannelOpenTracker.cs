using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal class ClientChannelOpenTracker : IChannelInitializer
    {
        private TelemetryClient telemetryClient;
        private String contractName;

        public ClientChannelOpenTracker(TelemetryClient client, Type contractType)
        {
            if ( client == null )
            {
                throw new ArgumentNullException(nameof(client));
            }
            if ( contractType == null )
            {
                throw new ArgumentNullException(nameof(contractType));
            }

            telemetryClient = client;
            this.contractName = contractType.FullName;
        }

        public void Initialize(IClientChannel channel)
        {
            if ( channel == null )
            {
                throw new ArgumentNullException(nameof(channel));
            }
        }
    }
}
