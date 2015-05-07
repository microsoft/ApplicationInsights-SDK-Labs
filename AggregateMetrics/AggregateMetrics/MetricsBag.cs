namespace Microsoft.ApplicationInsights.Extensibility.AggregateMetrics
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal class MetricsBag : ConcurrentBag<double>
    {
        internal MetricsBag()
            : this(null, null, null)
        {
        }

        internal MetricsBag(string property1, string property2, string property3)
            : base()
        {
            this.Property1 = property1;
            this.Property2 = property2;
            this.Property3 = property3;
        }

        internal string Property1 { get; private set; }
        internal string Property2 { get; private set; }
        internal string Property3 { get; private set; }

        internal AggregationResult CalculateAggregation(PercentileCalculation percentileCalculation = PercentileCalculation.DoNotCalculate)
        {
            if (this.Count == 0)
            {
                return new AggregationResult();
            }

            // Use sorted enumerables if we need percentiles.
            IEnumerable<double> values;

            if (percentileCalculation == PercentileCalculation.OrderByLargest)
            {
                values = this.OrderBy(i => i);
            }
            else if (percentileCalculation == PercentileCalculation.OrderBySmallest)
            {
                values = this.OrderByDescending(i => i);
            }
            else
            {
                values = this;
            }

            double first = values.First();
            double sum = 0;
            double min = first;
            double max = first;

            int p50Index = 0;
            int p75Index = 0;
            int p90Index = 0;
            int p95Index = 0;
            int p99Index = 0;

            bool shouldCalculatePercentile = percentileCalculation != PercentileCalculation.DoNotCalculate;

            if (shouldCalculatePercentile)
            {
                p50Index = this.GetPercentileNearestIndex(50);
                p75Index = this.GetPercentileNearestIndex(75);
                p90Index = this.GetPercentileNearestIndex(90);
                p95Index = this.GetPercentileNearestIndex(95);
                p99Index = this.GetPercentileNearestIndex(99);
            }

            var aggregation = new AggregationResult()
            {
                Count = this.Count
            };

            int valueIndex = 0;

            foreach (double value in values)
            {
                if (value < min)
                {
                    min = value;
                }
                else if (value > max)
                {
                    max = value;
                }

                sum += value;

                if (shouldCalculatePercentile)
                {
                    // Note: This only works if sample size >= 100 (which we enforce with Constants.PercentileMinimumCount).
                    if (valueIndex == p50Index)
                    {
                        aggregation.P50 = value;
                    }
                    else if (valueIndex == p75Index)
                    {
                        aggregation.P75 = value;
                    }
                    else if (valueIndex == p90Index)
                    {
                        aggregation.P90 = value;
                    }
                    else if (valueIndex == p95Index)
                    {
                        aggregation.P95 = value;
                    }
                    else if (valueIndex == p99Index)
                    {
                        aggregation.P99 = value;
                    }
                }

                valueIndex++;
            }

            aggregation.Sum = sum;
            aggregation.Min = min;
            aggregation.Max = max;

            aggregation.StdDev = Math.Sqrt(this.Average(v => Math.Pow(v - aggregation.Average, 2)));

            return aggregation;
        }

        private int GetPercentileNearestIndex(int percentile)
        {
            Debug.Assert(percentile > 0 && percentile < 100);

            // Use nearest rank method.
            int index = ((int)Math.Round(this.Count * (percentile / 100.00), 0)) - 1;

            Debug.Assert(index >= 0 && index < this.Count);

            return index;
        }
    }
}
