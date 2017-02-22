#if NET40
using Microsoft.Diagnostics.Tracing;
#else
using System.Diagnostics.Tracing;
#endif
using System;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    [EventSource(Name = "Microsoft-ApplicationInsights-Wcf-Client")]
    internal sealed class WcfClientEventSource : EventSource
    {
        public sealed class Keywords
        {
            public const EventKeywords DependencyTracking = (EventKeywords)0x10;
        }

        public static readonly WcfClientEventSource Log = new WcfClientEventSource();

        public String ApplicationName { [NonEvent] get; [NonEvent] private set; }

        public WcfClientEventSource()
        {
            this.ApplicationName = GetApplicationName();
        }

        [Event(1, Keywords = Keywords.DependencyTracking, Message = "Client Telemetry applied to contract: {0}", Level = EventLevel.Informational)]
        public void ClientTelemetryApplied(String contract, String appDomainName = "Invalid")
        {
            this.WriteEvent(1, contract, this.ApplicationName);
        }

        [Event(2, Keywords = Keywords.DependencyTracking, Message = "{0}", Level = EventLevel.Informational)]
        public void ClientDependencyTrackingInfo(String info, String appDomainName = "Invalid")
        {
            this.WriteEvent(2, info, this.ApplicationName);
        }

        [Event(3, Keywords = Keywords.DependencyTracking, Message = "ChannelFactory created for channel shape {0}", Level = EventLevel.Informational)]
        public void ChannelFactoryCreated(String channelShape, String appDomainName = "Invalid")
        {
            this.WriteEvent(3, channelShape, this.ApplicationName);
        }

        [Event(4, Keywords = Keywords.DependencyTracking, Message = "{0}.{1} callback called.", Level = EventLevel.Informational)]
        public void ChannelCalled(String channel, String method, String appDomainName = "Invalid")
        {
            this.WriteEvent(4, channel, method, this.ApplicationName);
        }

        [Event(5, Keywords = Keywords.DependencyTracking, Message = "Exception while processing {0}: {1}", Level = EventLevel.Error)]
        public void ClientTelemetryError(String method, String exception, String appDomainName = "Invalid")
        {
            this.WriteEvent(5, method, exception, this.ApplicationName);
        }

        [Event(6, Keywords = Keywords.DependencyTracking, Message = "Callback '{1}' will not run for id = '{0}'. Reason: {2}", Level = EventLevel.Warning)]
        public void NotExpectedCallback(long id, string callbackName, string reason, string appDomainName = "Incorrect")
        {
            this.WriteEvent(6, id, callbackName ?? string.Empty, reason ?? string.Empty, this.ApplicationName);
        }

        [NonEvent]
        private String GetApplicationName()
        {
            try
            {
                return AppDomain.CurrentDomain.FriendlyName;
            } catch
            {
                return "Undefined";
            }
        }
    }
}
