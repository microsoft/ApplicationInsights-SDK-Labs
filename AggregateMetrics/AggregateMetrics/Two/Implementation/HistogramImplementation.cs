namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics.Two
{
    using System;
    using System.Threading;
    using Microsoft.ApplicationInsights.DataContracts;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using System.Globalization;
    internal class HistogramImplementation : NamedCounterValueBase, ICounterValue, IHistogram
    {
        private long compositeValue;

        private int minValue = Int32.MaxValue;
        private int maxValue = Int32.MinValue;

        private List<double> listValues = new List<double>();

        private readonly bool shouldCalculateMinMax;
        private readonly bool shouldCalculatePercentiles;

        public HistogramImplementation(string name, TelemetryContext context, HistogramAggregations aggregations)
            : base(name, context)
        {
            this.shouldCalculateMinMax = (aggregations & HistogramAggregations.MinMax) == HistogramAggregations.MinMax;
            this.shouldCalculatePercentiles = (aggregations & HistogramAggregations.Percentiles) == HistogramAggregations.Percentiles;
        }

        private static void InterlockedExchangeOnCondition(ref int location, int value, Func<int, int, bool> condition)
        {
            int current = location;

            while (condition(current, value))
            {
                var previous = Interlocked.CompareExchange(ref location, value, current);

                // In most cases first condition will break the loop. 
                // Sometimes another thread may set other value. Than we need to retry
                if (previous == current || !condition(previous, value))
                    break;
                
                current = location;
            }
        }

        private int GetPercentileNearestIndex(int percentile)
        {
            Debug.Assert(percentile > 0 && percentile < 100);

            int index = ((int)Math.Round(listValues.Count * (percentile / 100.0), 0)) - 1;

            Debug.Assert(index >= 0 && index < listValues.Count);

            return index;
        }

        private void CalculateMinMax(int value) {
            InterlockedExchangeOnCondition(ref this.minValue, value, (currentValue, newValue) => { return currentValue > newValue; });
            InterlockedExchangeOnCondition(ref this.maxValue, value, (currentValue, newValue) => { return currentValue < newValue; });
        }

        private PercentileAggregations CalculatePercentiles()
        {
            PercentileAggregations percentiles = new PercentileAggregations();

            listValues.OrderBy(i => i);

            int percentile50Index = GetPercentileNearestIndex(50);
            int percentile75Index = GetPercentileNearestIndex(75);
            int percentile90Index = GetPercentileNearestIndex(90);
            int percentile95Index = GetPercentileNearestIndex(95);
            int percentile99Index = GetPercentileNearestIndex(99);

            int listValueIndex = 0;
            foreach (double listValue in listValues)
            {
                if (listValueIndex == percentile50Index)
                {
                    percentiles.P50 = listValue;
                }
                else if (listValueIndex == percentile75Index)
                {
                    percentiles.P75 = listValue;
                }
                else if (listValueIndex == percentile90Index)
                {
                    percentiles.P90 = listValue;
                }
                else if (listValueIndex == percentile95Index)
                {
                    percentiles.P95 = listValue;
                }
                else if (listValueIndex == percentile99Index)
                {
                    percentiles.P99 = listValue;
                }

                listValueIndex++;
            }

            return percentiles;
        }

        public MetricTelemetry GetValueAndReset()
        {
            long curValue = Interlocked.Exchange(ref this.compositeValue, 0);
            int curMinValue = this.minValue;
            int curMaxValue = this.maxValue;

            minValue = Int32.MaxValue;
            maxValue = Int32.MinValue;


            var count = (int)(curValue & ((1 << 24) - 1));
            double value = curValue >> 24;

            var metric = this.GetInitializedMetricTelemetry();
            if (count != 0)
            {
                metric.Value = value / count;
                metric.Count = count;

                if (this.shouldCalculateMinMax)
                {
                    metric.Min = curMinValue;
                    metric.Max = curMaxValue;
                }
                else if (this.shouldCalculatePercentiles)
                {
                    PercentileAggregations percentiles = CalculatePercentiles();

                    metric.Properties.Add(Constants.P50Name, percentiles.P50.ToString(CultureInfo.InvariantCulture));
                    metric.Properties.Add(Constants.P75Name, percentiles.P75.ToString(CultureInfo.InvariantCulture));
                    metric.Properties.Add(Constants.P90Name, percentiles.P90.ToString(CultureInfo.InvariantCulture));
                    metric.Properties.Add(Constants.P95Name, percentiles.P95.ToString(CultureInfo.InvariantCulture));
                    metric.Properties.Add(Constants.P99Name, percentiles.P99.ToString(CultureInfo.InvariantCulture));

                    listValues.Clear();
                }
            }
            else
            {
                metric.Value = 0;
                metric.Count = 0;
            }

            return metric;
        }

        public void Update(int value)
        {
            long delta = ((value) << 24) + 1;
            Interlocked.Add(ref this.compositeValue, delta);

            if (this.shouldCalculateMinMax)
            {
                CalculateMinMax(value);
            }
            else if(this.shouldCalculatePercentiles)
            {
                listValues.Add(value);
            }
        }
    }
}
