using AI;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vent
{
    public static class TelemetryFactory
    {
        private static ContextTagKeys TagKeys = new ContextTagKeys();

        private static void PopulateContext(Envelope telemetryItem, ITelemetry result)
        {
            result.Context.InstrumentationKey = telemetryItem.iKey;
            foreach (var tag in telemetryItem.tags)
            {
                if (tag.Key == TagKeys.ApplicationVersion)
                {
                    result.Context.Component.Version = tag.Value;
                }
                else if (tag.Key == TagKeys.CloudRole)
                {
                    result.Context.Cloud.RoleName = tag.Value;
                }
                else if (tag.Key == TagKeys.CloudRoleInstance)
                {
                    result.Context.Cloud.RoleInstance = tag.Value;
                }
                else if (tag.Key == TagKeys.DeviceId)
                {
                    result.Context.Device.Id = tag.Value;
                }
                else if (tag.Key == TagKeys.DeviceLocale)
                {
                }
                else if (tag.Key == TagKeys.DeviceModel)
                {
                    result.Context.Device.Model = tag.Value;
                }
                else if (tag.Key == TagKeys.DeviceOEMName)
                {
                    result.Context.Device.OemName = tag.Value;
                }
                else if (tag.Key == TagKeys.DeviceOSVersion)
                {
                    result.Context.Device.OperatingSystem = tag.Value;
                }
                else if (tag.Key == TagKeys.DeviceType)
                {
                    result.Context.Device.Type = tag.Value;
                }
                else if (tag.Key == TagKeys.InternalAgentVersion)
                {
                    result.Context.GetInternalContext().AgentVersion = tag.Value;
                }
                else if (tag.Key == TagKeys.InternalNodeName)
                {
                    result.Context.GetInternalContext().NodeName = tag.Value;
                }
                else if (tag.Key == TagKeys.InternalSdkVersion)
                {
                    result.Context.GetInternalContext().SdkVersion = tag.Value;
                }
                else if (tag.Key == TagKeys.LocationIp)
                {
                    result.Context.Location.Ip = tag.Value;
                }
                else if (tag.Key == TagKeys.OperationCorrelationVector)
                {
                }
                else if (tag.Key == TagKeys.OperationId)
                {
                    result.Context.Operation.Id = tag.Value;
                }
                else if (tag.Key == TagKeys.OperationName)
                {
                    result.Context.Operation.Name = tag.Value;
                }
                else if (tag.Key == TagKeys.OperationParentId)
                {
                    result.Context.Operation.ParentId = tag.Value;
                }
                else if (tag.Key == TagKeys.OperationSyntheticSource)
                {
                    result.Context.Operation.SyntheticSource = tag.Value;
                }
                else if (tag.Key == TagKeys.SessionId)
                {
                    result.Context.Session.Id = tag.Value;
                }
                else if (tag.Key == TagKeys.SessionIsFirst)
                {
                    result.Context.Session.IsFirst = false;
                }
                else if (tag.Key == TagKeys.UserAccountId)
                {
                    result.Context.User.AccountId = tag.Value;
                }
                else if (tag.Key == TagKeys.UserAgent)
                {
                    result.Context.User.UserAgent = tag.Value; //TODO: Deprecated???
                }
                else if (tag.Key == TagKeys.UserAuthUserId)
                {
                    result.Context.User.AuthenticatedUserId = tag.Value;
                }
                else if (tag.Key == TagKeys.UserId)
                {
                    result.Context.User.Id = tag.Value;
                }
                else
                {
                    throw new InvalidOperationException("tag is not supported"); //TODO: should we? Maybe just swallow for now?
                }
            }

            // deprecated: result.Context.Device.Language = tag.Value;
            // deprecated: result.Context.Device.NetworkType = tag.Value;
            // deprecated: result.Context.Device.ScreenResolution = tag.Value;
        }

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
