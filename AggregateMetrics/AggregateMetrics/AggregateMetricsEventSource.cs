namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics
{
    using System;
    using System.Diagnostics.Tracing;
    using EventLevel = System.Diagnostics.Tracing.EventLevel;
    using EventOpcode = System.Diagnostics.Tracing.EventOpcode;

    [EventSource(Name = "Microsoft-ApplicationInsights-Extensibility-AggregateMetrics")]
    internal sealed class AggregateMetricsEventSource : EventSource
    {
        public static readonly AggregateMetricsEventSource Log = new AggregateMetricsEventSource();

        private AggregateMetricsEventSource()
        {
            this.ApplicationName = this.GetApplicationName();
        }

        public string ApplicationName { get; private set; }

        [Event(
            2,
            Message = "AggregateMetricsModule initialization start",
            Level = EventLevel.Verbose,
            Keywords = Keywords.ModuleInitialization)]
        public void ModuleInitializationBegin()
        {
            this.WriteEvent(2, this.ApplicationName);
        }

        [Event(
            3,
            Message = "AggregateMetricsModule initialization stop",
            Level = EventLevel.Verbose,
            Keywords = Keywords.ModuleInitialization)]
        public void ModuleInitializationEnd()
        {
            this.WriteEvent(3, this.ApplicationName);
        }

        [Event(
            4,
            Message = "Aggregate Metric name was not provided to RegisterAggregateMetric. Ignoring registration.",
            Level = EventLevel.Error,
            Opcode = EventOpcode.Info,
            Keywords = Keywords.AggregateMetricRegistration | Keywords.UserActionable)]
        public void RegisterAggregateMetricInvalidMetricName()
        {
            this.WriteEvent(4, this.ApplicationName);
        }

        [Event(
            5,
            Message = "Aggregate Metric name provided to RegisterAggregateMetric already exists. Ignoring registration.",
            Level = EventLevel.Error,
            Opcode = EventOpcode.Info,
            Keywords = Keywords.AggregateMetricRegistration | Keywords.UserActionable)]
        public void RegisterAggregateMetricDuplicateMetricName(string metricName)
        {
            this.WriteEvent(5, metricName, this.ApplicationName);
        }

        [Event(
            6,
            Message = "Aggregate Metric name was not provided to UnregisterAggregateMetric. Ignoring registration.",
            Level = EventLevel.Error,
            Opcode = EventOpcode.Info,
            Keywords = Keywords.AggregateMetricRegistration | Keywords.UserActionable)]
        public void UnregisterAggregateMetricInvalidMetricName()
        {
            this.WriteEvent(6, this.ApplicationName);
        }

        [Event(
            7,
            Message = "TelemetryClient is required. Metric will not be tracked.",
            Level = EventLevel.Error,
            Opcode = EventOpcode.Info,
            Keywords = Keywords.AggregateMetricTrack | Keywords.UserActionable)]
        public void TelemetryClientRequired()
        {
            this.WriteEvent(7, this.ApplicationName);
        }

        [Event(
            8,
            Message = "Aggregate Metric name was not provided to TrackAggregateMetric. Metric will not be tracked.",
            Level = EventLevel.Error,
            Opcode = EventOpcode.Info,
            Keywords = Keywords.AggregateMetricTrack | Keywords.UserActionable)]
        public void TrackAggregateMetricInvalidMetricName()
        {
            this.WriteEvent(8, this.ApplicationName);
        }

        [Event(
            9,
            Message = "Aggregate Metric name '{0}' length {1} is longer than maxmimum allowed length. Name will be truncated to length {2}.",
            Level = EventLevel.Error,
            Opcode = EventOpcode.Info,
            Keywords = Keywords.AggregateMetricTrack | Keywords.UserActionable)]
        public void TrackAggregateMetricMetricNameTooLong(string metricName, int length, int maxLength)
        {
            this.WriteEvent(9, metricName, length, maxLength, this.ApplicationName);
        }

        [Event(
            10,
            Message = "Flush was called manually.",
            Level = EventLevel.Informational,
            Opcode = EventOpcode.Info)]
        public void ManualFlushActivated()
        {
            this.WriteEvent(10, this.ApplicationName);
        }

        [Event(
            11,
            Message = "Flush was called on timer.",
            Level = EventLevel.Informational,
            Opcode = EventOpcode.Info)]
        public void TimerFlushActivated()
        {
            this.WriteEvent(11, this.ApplicationName);
        }

        [Event(
            12,
            Message = "FlushIntervalSeconds of {0} was set out of range. Minimum interval is {1} and maxmimum interval is {2}. Setting to default of {3}.",
            Level = EventLevel.Error,
            Opcode = EventOpcode.Info)]
        public void FlushIntervalSecondsOutOfRange(double intervalSeconds)
        {
            this.WriteEvent(12, intervalSeconds, Constants.MinimumTimerFlushInterval, Constants.MaximumTimerFlushInterval, Constants.DefaultTimerFlushInterval, this.ApplicationName);
        }

        private string GetApplicationName()
        {
            string name;
            try
            {
                name = AppDomain.CurrentDomain.FriendlyName;
            }
            catch (Exception exp)
            {
                name = "Undefined " + exp.Message ?? exp.ToString();
            }

            return name;
        }

        /// <summary>
        /// Keywords for the PlatformEventSource. Those keywords should match keywords in Core.
        /// </summary>
        public static class Keywords
        {
            /// <summary>
            /// Key word for user actionable events.
            /// </summary>
            internal const EventKeywords UserActionable = (EventKeywords)0x1;

            /// <summary>
            /// Diagnostics tracing keyword.
            /// </summary>
            internal const EventKeywords Diagnostics = (EventKeywords)0x2;

            /// <summary>
            /// Keyword for errors that trace at Verbose level.
            /// </summary>
            internal const EventKeywords VerboseFailure = (EventKeywords)0x4;

            /// <summary>
            /// Module initialization event group.
            /// </summary>
            internal const EventKeywords ModuleInitialization = (EventKeywords)0x10;

            /// <summary>
            /// Aggregate Metric registration event group.
            /// </summary>
            internal const EventKeywords AggregateMetricRegistration = (EventKeywords)((int)ModuleInitialization << 1);

            /// <summary>
            /// Aggregate Metric track event group.
            /// </summary>
            internal const EventKeywords AggregateMetricTrack = (EventKeywords)((int)AggregateMetricRegistration << 1);
        }
    }
}
