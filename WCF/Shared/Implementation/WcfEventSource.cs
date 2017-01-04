#if NET40
using Microsoft.Diagnostics.Tracing;
#else
using System.Diagnostics.Tracing;
#endif
using System;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    [EventSource(Name = "Microsoft-ApplicationInsights-Wcf")]
    internal sealed class WcfEventSource : EventSource
    {
        // Keywords must be powers of 2 since they are used as bitmasks
        public sealed class Keywords
        {
            public const EventKeywords WcfModule = (EventKeywords)0x10;
            public const EventKeywords RequestTelemetry = (EventKeywords)0x20;
            public const EventKeywords ExceptionTelemetry = (EventKeywords)0x40;
            public const EventKeywords OperationContext = (EventKeywords)0x80;
        }

        public static readonly WcfEventSource Log = new WcfEventSource();

        public String ApplicationName { [NonEvent] get; [NonEvent] private set; }

        public WcfEventSource()
        {
            this.ApplicationName = GetApplicationName();
        }


        [Event(1, Keywords = Keywords.WcfModule, Message = "ServiceTelemetry module failed to initialize with exception: {0}", Level = EventLevel.Verbose)]
        public void InitializationFailure(String exception, String appDomainName = "Invalid")
        {
            this.WriteEvent(1, exception, ApplicationName);
        }

        [Event(4, Keywords = Keywords.WcfModule, Message = "WCF Telemetry Module {0} started stage {1}", Level = EventLevel.Verbose)]
        public void TelemetryModuleExecutionStarted(String typeName, String stageName, String appDomainName = "Invalid")
        {
            this.WriteEvent(4, typeName, stageName, ApplicationName);
        }

        [Event(5, Keywords = Keywords.WcfModule, Message = "WCF Telemetry Module {0} stopped stage {1}", Level = EventLevel.Verbose)]
        public void TelemetryModuleExecutionStopped(String typeName, String stageName, String appDomainName = "Invalid")
        {
            this.WriteEvent(5, typeName, stageName, ApplicationName);
        }

        [Event(6, Keywords = Keywords.WcfModule, Message = "WCF Telemetry Module {0} failed stage {1} with exception: {3}", Level = EventLevel.Error)]
        public void TelemetryModuleExecutionFailed(String typeName, String stageName, String exception, String appDomainName = "Invalid")
        {
            this.WriteEvent(5, typeName, stageName, exception, ApplicationName);
        }

        [Event(7, Keywords = Keywords.WcfModule, Message = "No OperationContext found on thread", Level = EventLevel.Warning)]
        public void NoOperationContextFound(String appDomainName = "Invalid")
        {
            this.WriteEvent(7, ApplicationName);
        }

        [Event(8, Keywords = Keywords.WcfModule, Message = "Request ignored because operation is not marked with [OperationTelemetry]: {0}#{1} - {2}", Level = EventLevel.Verbose)]
        public void OperationIgnored(String contractName, String contractNamespace, String operationName, String appDomainName = "Invalid")
        {
            this.WriteEvent(8, contractName, contractNamespace, operationName, ApplicationName);
        }

        [Event(9, Keywords = Keywords.RequestTelemetry, Message = "WCF TelemetryInitializer loaded: {0}", Level = EventLevel.Verbose)]
        public void WcfTelemetryInitializerLoaded(String typeName, String appDomainName = "Invalid")
        {
            this.WriteEvent(9, typeName, ApplicationName);
        }

        [Event(15, Keywords = Keywords.WcfModule, Message = "Location.Id set to: {0}", Level = EventLevel.Verbose)]
        public void LocationIdSet(String ip, String appDomainName = "Invalid")
        {
            this.WriteEvent(15, ip ?? "NULL", this.ApplicationName);
        }

        [Event(30, Keywords = Keywords.OperationContext, Message = "WcfOperationContext created. OpId={0}; OwnRequest={1}", Level = EventLevel.Verbose)]
        public void OperationContextCreated(String operationId, bool ownsRequest, String appDomainName = "Invalid")
        {
            this.WriteEvent(30, operationId ?? "NULL", ownsRequest, this.ApplicationName);
        }

        [Event(35, Keywords = Keywords.OperationContext, Message = "Request message closed while reading property {0}", Level = EventLevel.Warning)]
        public void RequestMessageClosed(String property, String appDomainName = "Invalid")
        {
            this.WriteEvent(35, property, this.ApplicationName);
        }

        [Event(36, Keywords = Keywords.OperationContext, Message = "Response message closed while reading property {0}", Level = EventLevel.Warning)]
        public void ResponseMessageClosed(String property, String appDomainName = "Invalid")
        {
            this.WriteEvent(36, property, this.ApplicationName);
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
