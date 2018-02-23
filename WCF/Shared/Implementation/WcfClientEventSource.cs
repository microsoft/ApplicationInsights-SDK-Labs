namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Diagnostics.Tracing;

    [EventSource(Name = "Microsoft-ApplicationInsights-Wcf-Client")]
    internal sealed class WcfClientEventSource : EventSource
    {
        public static readonly WcfClientEventSource Log = new WcfClientEventSource();

        public WcfClientEventSource()
        {
            this.ApplicationName = this.GetApplicationName();
        }

        public string ApplicationName { [NonEvent] get; [NonEvent] private set; }

        [Event(1, Keywords = Keywords.DependencyTracking, Message = "Client Telemetry applied to contract: {0}", Level = EventLevel.Informational)]
        public void ClientTelemetryApplied(string contract, string appDomainName = "Invalid")
        {
            this.WriteEvent(1, contract, this.ApplicationName);
        }

        [Event(2, Keywords = Keywords.DependencyTracking, Message = "{0}", Level = EventLevel.Informational)]
        public void ClientDependencyTrackingInfo(string info, string appDomainName = "Invalid")
        {
            this.WriteEvent(2, info, this.ApplicationName);
        }

        [Event(3, Keywords = Keywords.DependencyTracking, Message = "ChannelFactory created for channel shape {0}", Level = EventLevel.Informational)]
        public void ChannelFactoryCreated(string channelShape, string appDomainName = "Invalid")
        {
            this.WriteEvent(3, channelShape, this.ApplicationName);
        }

        [Event(4, Keywords = Keywords.DependencyTracking, Message = "{0}.{1} callback called.", Level = EventLevel.Informational)]
        public void ChannelCalled(string channel, string method, string appDomainName = "Invalid")
        {
            this.WriteEvent(4, channel, method, this.ApplicationName);
        }

        [Event(5, Keywords = Keywords.DependencyTracking, Message = "Exception while processing {0}: {1}", Level = EventLevel.Error)]
        public void ClientTelemetryError(string method, string exception, string appDomainName = "Invalid")
        {
            this.WriteEvent(5, method, exception, this.ApplicationName);
        }

        [Event(6, Keywords = Keywords.DependencyTracking, Message = "Callback '{1}' will not run for id = '{0}'. Reason: {2}", Level = EventLevel.Warning)]
        public void NotExpectedCallback(long id, string callbackName, string reason, string appDomainName = "Incorrect")
        {
            this.WriteEvent(6, id, callbackName ?? string.Empty, reason ?? string.Empty, this.ApplicationName);
        }

        [Event(7, Keywords = Keywords.DependencyTracking, Message = "Client Telemetry ignoring non-SOAP contract: {0}", Level = EventLevel.Informational)]
        public void ClientTelemetryIgnoreContract(string contract, string appDomainName = "Invalid")
        {
            this.WriteEvent(7, contract, this.ApplicationName);
        }

        [NonEvent]
        private string GetApplicationName()
        {
            try
            {
                return AppDomain.CurrentDomain.FriendlyName;
            }
            catch
            {
                return "Undefined";
            }
        }

        public sealed class Keywords
        {
            public const EventKeywords DependencyTracking = (EventKeywords)0x10;
        }
    }
}
