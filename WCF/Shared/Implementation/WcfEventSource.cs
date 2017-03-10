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

        public const String InitializationFailure_Message = "ServiceTelemetry module failed to initialize with exception: {0}";
        [Event(1, Keywords = Keywords.WcfModule, Message = InitializationFailure_Message, Level = EventLevel.Verbose)]
        public void InitializationFailure(String exception, String appDomainName = "Invalid")
        {
            this.WriteEvent(1, exception, ApplicationName);
        }

        public const String TelemetryModuleExecutionStarted_Message = "WCF Telemetry Module {0} started stage {1}";
        [Event(4, Keywords = Keywords.WcfModule, Message = TelemetryModuleExecutionStarted_Message, Level = EventLevel.Verbose)]
        public void TelemetryModuleExecutionStarted(String typeName, String stageName, String appDomainName = "Invalid")
        {
            this.WriteEvent(4, typeName, stageName, ApplicationName);
        }

        public const String TelemetryModuleExecutionStopped_Message = "WCF Telemetry Module {0} stopped stage {1}";
        [Event(5, Keywords = Keywords.WcfModule, Message = TelemetryModuleExecutionStopped_Message, Level = EventLevel.Verbose)]
        public void TelemetryModuleExecutionStopped(String typeName, String stageName, String appDomainName = "Invalid")
        {
            this.WriteEvent(5, typeName, stageName, ApplicationName);
        }

        public const String TelemetryModuleExecutionFailed_Message = "WCF Telemetry Module {0} failed stage {1} with exception: {2}";
        [Event(6, Keywords = Keywords.WcfModule, Message = TelemetryModuleExecutionFailed_Message, Level = EventLevel.Error)]
        public void TelemetryModuleExecutionFailed(String typeName, String stageName, String exception, String appDomainName = "Invalid")
        {
            this.WriteEvent(6, typeName, stageName, exception, ApplicationName);
        }

        public const String NoOperationContextFound_Message = "No OperationContext found on thread";
        [Event(7, Keywords = Keywords.WcfModule, Message = NoOperationContextFound_Message, Level = EventLevel.Warning)]
        public void NoOperationContextFound(String appDomainName = "Invalid")
        {
            this.WriteEvent(7, ApplicationName);
        }

        public const String OperationIgnored_Message = "Request ignored because operation is not marked with [OperationTelemetry]: {0}#{1} - {2}";
        [Event(8, Keywords = Keywords.WcfModule, Message = OperationIgnored_Message, Level = EventLevel.Verbose)]
        public void OperationIgnored(String contractName, String contractNamespace, String operationName, String appDomainName = "Invalid")
        {
            this.WriteEvent(8, contractName, contractNamespace, operationName, ApplicationName);
        }

        public const String WcfTelemetryInitializerLoaded_Message = "WCF TelemetryInitializer loaded: {0}";
        [Event(9, Keywords = Keywords.RequestTelemetry, Message = WcfTelemetryInitializerLoaded_Message, Level = EventLevel.Verbose)]
        public void WcfTelemetryInitializerLoaded(String typeName, String appDomainName = "Invalid")
        {
            this.WriteEvent(9, typeName, ApplicationName);
        }

        public const String LocationIdSet_Message = "Location.Id set to: {0}";
        [Event(15, Keywords = Keywords.WcfModule, Message = LocationIdSet_Message, Level = EventLevel.Verbose)]
        public void LocationIdSet(String ip, String appDomainName = "Invalid")
        {
            this.WriteEvent(15, ip ?? "NULL", this.ApplicationName);
        }

        public const String OperationContextCreated_Message = "WcfOperationContext created. OpId={0}; OwnRequest={1}";
        [Event(30, Keywords = Keywords.OperationContext, Message = OperationContextCreated_Message, Level = EventLevel.Verbose)]
        public void OperationContextCreated(String operationId, bool ownsRequest, String appDomainName = "Invalid")
        {
            this.WriteEvent(30, operationId ?? "NULL", ownsRequest, this.ApplicationName);
        }

        public const String RequestMessageClosed_Message = "Request message closed while reading property {0}";
        [Event(35, Keywords = Keywords.OperationContext, Message = RequestMessageClosed_Message, Level = EventLevel.Warning)]
        public void RequestMessageClosed(String property, String appDomainName = "Invalid")
        {
            this.WriteEvent(35, property, this.ApplicationName);
        }

        public const String ResponseMessageClosed_Message = "Response message closed while reading property {0}";
        [Event(36, Keywords = Keywords.OperationContext, Message = ResponseMessageClosed_Message, Level = EventLevel.Warning)]
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
