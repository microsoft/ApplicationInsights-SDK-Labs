using AI;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vent
{
    public static class TelemetryFactory
    {
        public static ITelemetry ConvertTelemetryItemToITelemetry(Envelope telemetryItem)
        {
            if (telemetryItem is AI.TelemetryItem<AI.MetricData>)
            {
                var metricItem = (AI.TelemetryItem<AI.MetricData>)telemetryItem;
                MetricTelemetry result = new MetricTelemetry();
                result.Name = metricItem.data.baseData.metrics[0].name;
                result.Sum = metricItem.data.baseData.metrics[0].value;
                result.Count = metricItem.data.baseData.metrics[0].count;
                result.StandardDeviation = metricItem.data.baseData.metrics[0].stdDev;
                result.Min = metricItem.data.baseData.metrics[0].min;
                result.Max = metricItem.data.baseData.metrics[0].max;

                result.Context.InstrumentationKey = metricItem.iKey;

                return result;
            }

            return null;
        }
    }
}
