namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Diagnostics.Tracing;

    [EventSource(Name = "Microsoft-ApplicationInsights-Wcf")]
    internal sealed class WcfEventSource : EventSource
    {
        public const string InitializationFailureMessage = "ServiceTelemetry module failed to initialize with exception: {0}";
        public const string TelemetryModuleExecutionStartedMessage = "WCF Telemetry Module {0} started stage {1}";
        public const string TelemetryModuleExecutionStoppedMessage = "WCF Telemetry Module {0} stopped stage {1}";
        public const string TelemetryModuleExecutionFailedMessage = "WCF Telemetry Module {0} failed stage {1} with exception: {2}";
        public const string NoOperationContextFoundMessage = "No OperationContext found on thread";
        public const string OperationIgnoredMessage = "Request ignored because operation is not marked with [OperationTelemetry]: {0}#{1} - {2}";
        public const string WcfTelemetryInitializerLoadedMessage = "WCF TelemetryInitializer loaded: {0}";
        public const string LocationIdSetMessage = "Location.Id set to: {0}";
        public const string OperationContextCreatedMessage = "WcfOperationContext created. OpId={0}; OwnRequest={1}";
        public const string RequestMessageClosedMessage = "Request message closed while attempting action '{0}' ({1})";
        public const string ResponseMessageClosedMessage = "Response message closed while attempting action '{0}' ({1})";

        public static readonly WcfEventSource Log = new WcfEventSource();

        public WcfEventSource()
        {
            this.ApplicationName = this.GetApplicationName();
        }

        public string ApplicationName { [NonEvent] get; [NonEvent] private set; }

        [Event(1, Keywords = Keywords.WcfModule, Message = InitializationFailureMessage, Level = EventLevel.Verbose)]
        public void InitializationFailure(string exception, string appDomainName = "Invalid")
        {
            this.WriteEvent(1, exception, this.ApplicationName);
        }

        [Event(4, Keywords = Keywords.WcfModule, Message = TelemetryModuleExecutionStartedMessage, Level = EventLevel.Verbose)]
        public void TelemetryModuleExecutionStarted(string typeName, string stageName, string appDomainName = "Invalid")
        {
            this.WriteEvent(4, typeName, stageName, this.ApplicationName);
        }

        [Event(5, Keywords = Keywords.WcfModule, Message = TelemetryModuleExecutionStoppedMessage, Level = EventLevel.Verbose)]
        public void TelemetryModuleExecutionStopped(string typeName, string stageName, string appDomainName = "Invalid")
        {
            this.WriteEvent(5, typeName, stageName, this.ApplicationName);
        }

        [Event(6, Keywords = Keywords.WcfModule, Message = TelemetryModuleExecutionFailedMessage, Level = EventLevel.Error)]
        public void TelemetryModuleExecutionFailed(string typeName, string stageName, string exception, string appDomainName = "Invalid")
        {
            this.WriteEvent(6, typeName, stageName, exception, this.ApplicationName);
        }

        [Event(7, Keywords = Keywords.WcfModule, Message = NoOperationContextFoundMessage, Level = EventLevel.Warning)]
        public void NoOperationContextFound(string appDomainName = "Invalid")
        {
            this.WriteEvent(7, this.ApplicationName);
        }

        [Event(8, Keywords = Keywords.WcfModule, Message = OperationIgnoredMessage, Level = EventLevel.Verbose)]
        public void OperationIgnored(string contractName, string contractNamespace, string operationName, string appDomainName = "Invalid")
        {
            this.WriteEvent(8, contractName, contractNamespace, operationName, this.ApplicationName);
        }

        [Event(9, Keywords = Keywords.RequestTelemetry, Message = WcfTelemetryInitializerLoadedMessage, Level = EventLevel.Verbose)]
        public void WcfTelemetryInitializerLoaded(string typeName, string appDomainName = "Invalid")
        {
            this.WriteEvent(9, typeName, this.ApplicationName);
        }

        [Event(15, Keywords = Keywords.WcfModule, Message = LocationIdSetMessage, Level = EventLevel.Verbose)]
        public void LocationIdSet(string ip, string appDomainName = "Invalid")
        {
            this.WriteEvent(15, ip ?? "NULL", this.ApplicationName);
        }

        [Event(30, Keywords = Keywords.OperationContext, Message = OperationContextCreatedMessage, Level = EventLevel.Verbose)]
        public void OperationContextCreated(string operationId, bool ownsRequest, string appDomainName = "Invalid")
        {
            this.WriteEvent(30, operationId ?? "NULL", ownsRequest, this.ApplicationName);
        }

        [Event(35, Keywords = Keywords.OperationContext, Message = RequestMessageClosedMessage, Level = EventLevel.Warning)]
        public void RequestMessageClosed(string action, string argument, string appDomainName = "Invalid")
        {
            this.WriteEvent(35, action, argument, this.ApplicationName);
        }

        [Event(36, Keywords = Keywords.OperationContext, Message = ResponseMessageClosedMessage, Level = EventLevel.Warning)]
        public void ResponseMessageClosed(string action, string argument, string appDomainName = "Invalid")
        {
            this.WriteEvent(36, action, argument, this.ApplicationName);
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

        // Keywords must be powers of 2 since they are used as bitmasks
        public sealed class Keywords
        {
            public const EventKeywords WcfModule = (EventKeywords)0x10;
            public const EventKeywords RequestTelemetry = (EventKeywords)0x20;
            public const EventKeywords ExceptionTelemetry = (EventKeywords)0x40;
            public const EventKeywords OperationContext = (EventKeywords)0x80;
        }
    }
}
