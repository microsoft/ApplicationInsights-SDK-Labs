namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.Implementation; 

    /// <summary>
    /// Extension class to TelemetryClient to add support for TrackAggregateMetric aggregated metrics.
    /// </summary>
    public static class AggregateMetrics
    {
        #region Members
        internal static ConcurrentDictionary<int, AggregationSet> aggregationSets;
        internal static readonly ConcurrentDictionary<int, AggregateMetricProperties> metricRegistrations = new ConcurrentDictionary<int, AggregateMetricProperties>();

        private static string sdkVersion;

        private static readonly object clientsSyncRoot = new object();
        private static readonly object clientsFlushLock = new object();
        private static System.Threading.Timer aggregationTimer;
        private static bool disposing = false;
        #endregion

        /// <summary>
        /// Optionally registers an aggregate metric to provide additional aggregation metadata.
        /// </summary>
        public static void RegisterAggregateMetric(this TelemetryClient telemetryClient, string name, string p1Name = null, string p2Name = null, string p3Name = null, PercentileCalculation percentileCalculation = PercentileCalculation.DoNotCalculate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                AggregateMetricsEventSource.Log.RegisterAggregateMetricInvalidMetricName();
                return;
            }

            int registrationKey = AggregationSet.GetKey(telemetryClient, name);

            if (!metricRegistrations.TryAdd(registrationKey, new AggregateMetricProperties()
                {
                    P1Name = p1Name,
                    P2Name = p2Name,
                    P3Name = p3Name,
                    PercentileCalculation = percentileCalculation
                }))
            {
                AggregateMetricsEventSource.Log.RegisterAggregateMetricDuplicateMetricName(name);
            }
        }

        /// <summary>
        /// Unregisters an aggregate metric.
        /// </summary>
        public static void UnregisterAggregateMetric(this TelemetryClient telemetryClient, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                AggregateMetricsEventSource.Log.UnregisterAggregateMetricInvalidMetricName();
                return;
            }

            int key = AggregationSet.GetKey(telemetryClient, name);

            AggregateMetricProperties properties;
            metricRegistrations.TryRemove(key, out properties);

            AggregationSet aggregationSet;
            aggregationSets.TryRemove(key, out aggregationSet);
        }

        /// <summary>
        /// Tracks a counter which is aggregated into a MetricTelemetry item.
        /// </summary>
        public static void TrackAggregateMetric(this TelemetryClient telemetryClient, string name, double value, string property1 = null, string property2 = null, string property3 = null)
        {
            if (telemetryClient == null)
            {
                AggregateMetricsEventSource.Log.TelemetryClientRequired();
                return;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                AggregateMetricsEventSource.Log.TrackAggregateMetricInvalidMetricName();
                return;
            }

            if (aggregationSets == null)
            {
                Initialize();
            }

            Debug.Assert(aggregationSets != null);

            // Truncate any names longer than max supported length.
            if (name.Length > Constants.NameMaxLength)
            {
                AggregateMetricsEventSource.Log.TrackAggregateMetricMetricNameTooLong(name, name.Length, Constants.NameMaxLength);
                name = name.Substring(0, Constants.NameMaxLength);
            }

            Debug.Assert(name.Length > 0 && name.Length <= Constants.NameMaxLength, "Invalid name.");

            int aggregationSetKey = AggregationSet.GetKey(telemetryClient, name);

            AggregationSet aggregationSet = aggregationSets.GetOrAdd(aggregationSetKey, (key) =>
            {
                return new AggregationSet(telemetryClient, name);
            });

            aggregationSet.AddAggregation(value, property1, property2, property3);
        }

        /// <summary>
        /// Flush the aggregation buffers and emit MetricTelemetry items.
        /// </summary>
        public static void Flush()
        {
            AggregateMetricsEventSource.Log.ManualFlushActivated();
            TimerFlushCallback(null);
        }

        internal static void Clear()
        {
            while (aggregationSets != null && aggregationSets.Count > 0)
            {
                foreach (int key in aggregationSets.Keys)
                {
                    AggregationSet agg;
                    aggregationSets.TryRemove(key, out agg);
                }
            }
        }

        internal static void FlushImpl()
        {
            foreach (KeyValuePair<int, AggregationSet> aggregationSetPair in aggregationSets)
            {
                if (disposing)
                {
                    return;
                }

                AggregationSet aggregationSet = aggregationSetPair.Value;

                ConcurrentDictionary<int, MetricsBag> aggregations = aggregationSet.RemoveAggregations();
                var periodStartTime = DateTimeOffset.Now.AddSeconds(-AggregateMetricsTelemetryModule.FlushIntervalSeconds);

                foreach (MetricsBag metricsBag in aggregations.Values)
                    {
                        if (disposing)
                        {
                            return;
                        }

                        int registrationKey = aggregationSet.Key;

                        string p1Name = null;
                        string p2Name = null;
                        string p3Name = null;
                        var percentileCalculation = PercentileCalculation.DoNotCalculate;

                        AggregateMetricProperties metricProperties;
                        if (metricRegistrations.TryGetValue(registrationKey, out metricProperties))
                        {
                            p1Name = metricProperties.P1Name;
                            p2Name = metricProperties.P2Name;
                            p3Name = metricProperties.P3Name;
                            percentileCalculation = metricProperties.PercentileCalculation;
                        }
                        else
                        {
                            p1Name = Constants.DefaultP1Name;
                            p2Name = Constants.DefaultP2Name;
                            p3Name = Constants.DefaultP3Name;
                        }

                        AggregationResult aggregation = metricsBag.CalculateAggregation(percentileCalculation);

                        var metric = new MetricTelemetry(aggregationSet.Name, aggregation.Average)
                        {
                            Timestamp = periodStartTime,
                            Count = aggregation.Count,
                            Min = aggregation.Min,
                            Max = aggregation.Max,
                            StandardDeviation = aggregation.StdDev
                        };

                        metric.Context.GetInternalContext().SdkVersion = sdkVersion;

                        metric.AddProperties(metricsBag, p1Name, p2Name, p3Name);

                        if (percentileCalculation != PercentileCalculation.DoNotCalculate)
                        {
                            metric.Properties.Add(Constants.P50Name, aggregation.P50.ToString(CultureInfo.InvariantCulture));
                            MetricTelemetry p50Metric = metric.CreateDerivedMetric(Constants.P50Name, aggregation.P50);
                            p50Metric.AddProperties(metricsBag, p1Name, p2Name, p3Name);
                            aggregationSet.TelemetryClient.TrackMetric(p50Metric);

                            metric.Properties.Add(Constants.P75Name, aggregation.P75.ToString(CultureInfo.InvariantCulture));
                            MetricTelemetry p75Metric = metric.CreateDerivedMetric(Constants.P75Name, aggregation.P75);
                            p75Metric.AddProperties(metricsBag, p1Name, p2Name, p3Name);
                            aggregationSet.TelemetryClient.TrackMetric(p75Metric);

                            metric.Properties.Add(Constants.P90Name, aggregation.P90.ToString(CultureInfo.InvariantCulture));
                            MetricTelemetry p90Metric = metric.CreateDerivedMetric(Constants.P90Name, aggregation.P90);
                            p90Metric.AddProperties(metricsBag, p1Name, p2Name, p3Name);
                            aggregationSet.TelemetryClient.TrackMetric(p90Metric);

                            metric.Properties.Add(Constants.P95Name, aggregation.P95.ToString(CultureInfo.InvariantCulture));
                            MetricTelemetry p95Metric = metric.CreateDerivedMetric(Constants.P95Name, aggregation.P95);
                            p95Metric.AddProperties(metricsBag, p1Name, p2Name, p3Name);
                            aggregationSet.TelemetryClient.TrackMetric(p95Metric);

                            metric.Properties.Add(Constants.P99Name, aggregation.P99.ToString(CultureInfo.InvariantCulture));
                            MetricTelemetry p99Metric = metric.CreateDerivedMetric(Constants.P99Name, aggregation.P99);
                            p99Metric.AddProperties(metricsBag, p1Name, p2Name, p3Name);
                            aggregationSet.TelemetryClient.TrackMetric(p99Metric);
                        }

                        aggregationSet.TelemetryClient.TrackMetric(metric);
                    }
                }
        }

        private static void TimerFlushCallback(object obj)
        {
            AggregateMetricsEventSource.Log.TimerFlushActivated();

            if (disposing || aggregationSets.Count == 0 || !AggregateMetricsTelemetryModule.IsTimerFlushEnabled)
            {
                return;
            }
        
            FlushImpl();
        }

        private static void Initialize()
        {
            lock (clientsSyncRoot)
            {
                if (aggregationSets == null)
                {
                    AggregateMetricsEventSource.Log.ModuleInitializationStarted();

                    sdkVersion = SdkVersionUtils.VersionPrefix + SdkVersionUtils.GetAssemblyVersion();

                    aggregationSets = new ConcurrentDictionary<int, AggregationSet>();

                    if (AggregateMetricsTelemetryModule.IsTimerFlushEnabled)
                    {
                        TimeSpan aggregationWindow = TimeSpan.FromSeconds(AggregateMetricsTelemetryModule.FlushIntervalSeconds);

                        AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;

                        aggregationTimer = new System.Threading.Timer(new System.Threading.TimerCallback(TimerFlushCallback), null, aggregationWindow, aggregationWindow);
                    }

                    AggregateMetricsEventSource.Log.ModuleInitializationStopped();
                }
            }
        }

        private static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            disposing = true;

            if (aggregationTimer != null)
            {
                aggregationTimer.Dispose();
                aggregationTimer = null;
            }
        }
    }
}
