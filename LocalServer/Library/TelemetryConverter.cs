namespace Library
{
    using Google.Protobuf.Collections;
    using Inputs.Contracts;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public static class TelemetryConverter
    {
        private static readonly ContextTagKeys TagKeys = new ContextTagKeys();

        public static EventTelemetry ConvertEventToSdkApi(Telemetry inputTelemetry)
        {
            var result = new EventTelemetry();

            result.Name = inputTelemetry.Event.Name;
            result.Properties.PopulateFromProtobuf(inputTelemetry.Event.Properties);
            result.Metrics.PopulateFromProtobuf(inputTelemetry.Event.Measurements);

            TelemetryConverter.CopyCommonFields(inputTelemetry, result);
            TelemetryConverter.CopySamplingFields(inputTelemetry, result);

            return result;
        }

        public static TraceTelemetry ConvertTraceToSdkApi(Telemetry inputTelemetry)
        {
            var result = new TraceTelemetry();

            result.Message = inputTelemetry.Message.Message_;

            result.SeverityLevel = TelemetryConverter.ConvertSeverityLevel(inputTelemetry.Message.SeverityLevel);

            result.Properties.PopulateFromProtobuf(inputTelemetry.Message.Properties);

            TelemetryConverter.CopyCommonFields(inputTelemetry, result);
            TelemetryConverter.CopySamplingFields(inputTelemetry, result);

            return result;
        }

        public static MetricTelemetry ConvertMetricToSdkApi(Telemetry inputTelemetry)
        {
            // only one metric in the list is currently supported by Application Insights storage. If multiple data points were sent only the first one will be used
            if (!inputTelemetry.Metric.Metrics.Any())
            {
                throw new ArgumentException("Metrics list can't be empty");
            }

            DataPoint firstMetric = inputTelemetry.Metric.Metrics[0];

            MetricTelemetry result;

            switch (firstMetric.Kind)
            {
                case DataPointType.Measurement:
                    result = new MetricTelemetry(firstMetric.Name, firstMetric.Value);
                    break;
                case DataPointType.Aggregation:
                    if (firstMetric.Count == null || firstMetric.Min == null || firstMetric.Max == null || firstMetric.StdDev == null)
                    {
                        throw new ArgumentNullException(FormattableString.Invariant($"For an aggregation metric, all of the following must be specified: Count, Min, Max, StdDev."));
                    }

                    result = new MetricTelemetry(firstMetric.Name, firstMetric.Count.Value, firstMetric.Value, firstMetric.Min.Value, firstMetric.Max.Value, firstMetric.StdDev.Value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(FormattableString.Invariant($"Unknown aggregation Kind: {firstMetric.Kind}"));
            }

            result.Properties.PopulateFromProtobuf(inputTelemetry.Metric.Properties);

            TelemetryConverter.CopyCommonFields(inputTelemetry, result);
            
            return result;
        }

        public static ExceptionTelemetry ConvertExceptionToSdkApi(Telemetry inputTelemetry)
        {
            var result = new ExceptionTelemetry();

            
            result.ProblemId = inputTelemetry.Exception.ProblemId;

            result.SeverityLevel = TelemetryConverter.ConvertSeverityLevel(inputTelemetry.Exception.SeverityLevel);

            //!!!
            throw new NotImplementedException("Exceptions are not implemented yet");
            //var stackFrames = new List<System.Diagnostics.StackFrame>();
            //foreach (var exception in inputTelemetry.Exceptions)
            //{
            //    var ex = new System.Exception();
            //    ex.
            //    //stackFrames.Add(System.Diagnostics.StackFrame);
            //}

            //telemetry.SetParsedStack(inputTelemetry.Exceptions.First().)


            result.Properties.PopulateFromProtobuf(inputTelemetry.Exception.Properties);
            result.Metrics.PopulateFromProtobuf(inputTelemetry.Exception.Measurements);

            TelemetryConverter.CopyCommonFields(inputTelemetry, result);
            TelemetryConverter.CopySamplingFields(inputTelemetry, result);


            return result;
        }

        public static DependencyTelemetry ConvertDependencyToSdkApi(Telemetry inputTelemetry)
        {
            var result = new DependencyTelemetry();

            Dependency item = inputTelemetry.Dependency;

            result.Type = item.Type;
            result.Target = item.Target;
            result.Name = item.Name;
            result.Data = item.Data;

            result.Timestamp = DateTimeOffset.ParseExact(inputTelemetry.DateTime, "0", CultureInfo.InvariantCulture);

            result.Duration = item.Duration.ToTimeSpan();
            result.ResultCode = item.ResultCode;
            result.Success = item.Success?.Value ?? true;
            result.Id = item.Id;
            
            result.Properties.PopulateFromProtobuf(item.Properties);
            result.Metrics.PopulateFromProtobuf(item.Measurements);

            TelemetryConverter.CopyCommonFields(inputTelemetry, result);
            TelemetryConverter.CopySamplingFields(inputTelemetry, result);

            return result;
        }

        public static AvailabilityTelemetry ConvertAvailabilityToSdkApi(Telemetry inputTelemetry)
        {
            var result = new AvailabilityTelemetry();

            Availability item = inputTelemetry.Availability;

            result.Name = item.Name;

            result.Timestamp = DateTimeOffset.ParseExact(inputTelemetry.DateTime, "0", CultureInfo.InvariantCulture);

            result.Duration = item.Duration.ToTimeSpan();
            result.RunLocation = item.RunLocation;
            result.Success = item.Success;
            result.Message = item.Message;
            
            result.Properties.PopulateFromProtobuf(item.Properties);
            result.Metrics.PopulateFromProtobuf(item.Measurements);

            TelemetryConverter.CopyCommonFields(inputTelemetry, result);
            
            return result;
        }

        public static PageViewTelemetry ConvertPageViewToSdkApi(Telemetry inputTelemetry)
        {
            var result = new PageViewTelemetry();

            PageView item = inputTelemetry.PageView;

            item.Event = item.Event ?? new Event();

            result.Url = new Uri(item.Url);
            result.Duration = item.Duration.ToTimeSpan();
            
            result.Name = item.Event.Name;

            result.Properties.PopulateFromProtobuf(item.Event?.Properties);
            result.Metrics.PopulateFromProtobuf(item.Event?.Measurements);

            TelemetryConverter.CopyCommonFields(inputTelemetry, result);
            TelemetryConverter.CopySamplingFields(inputTelemetry, result);

            return result;
        }

        public static RequestTelemetry ConvertRequestToSdkApi(Telemetry inputTelemetry)
        {
            var result = new RequestTelemetry();

            Request item = inputTelemetry.Request;

            result.Name = item.Name;

            result.Timestamp = DateTimeOffset.ParseExact(inputTelemetry.DateTime, "0", CultureInfo.InvariantCulture);

            result.Duration = item.Duration.ToTimeSpan();
            result.ResponseCode = item.ResponseCode;
            result.Success = item.Success?.Value;

            result.Properties.PopulateFromProtobuf(item.Properties);
            result.Metrics.PopulateFromProtobuf(item.Measurements);

            TelemetryConverter.CopyCommonFields(inputTelemetry, result);
            TelemetryConverter.CopySamplingFields(inputTelemetry, result);

            return result;
        }

        private static void CopyCommonFields(Telemetry inputTelemetry, ITelemetry telemetry)
        {
            telemetry.Sequence = inputTelemetry.SequenceNumber;
            telemetry.Timestamp = DateTimeOffset.ParseExact(inputTelemetry.DateTime, "0", CultureInfo.InvariantCulture);
            telemetry.Context.InstrumentationKey = inputTelemetry.InstrumentationKey;

            TelemetryConverter.PopulateContext(inputTelemetry, telemetry);
        }

        private static void CopySamplingFields(Telemetry inputTelemetry, ISupportSampling telemetry)
        {
            telemetry.SamplingPercentage = inputTelemetry.SamplingRate?.Value ?? 100;
        }

        private static void PopulateContext(Telemetry telemetryItem, ITelemetry telemetry)
        {
            foreach (var tag in telemetryItem.Tags)
            {
                if (string.Equals(tag.Key, TelemetryConverter.TagKeys.ApplicationVersion, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Component.Version = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.CloudRole, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Cloud.RoleName = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.CloudRoleInstance, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Cloud.RoleInstance = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.DeviceId, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Device.Id = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.DeviceLocale, StringComparison.InvariantCulture))
                {
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.DeviceModel, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Device.Model = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.DeviceOEMName, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Device.OemName = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.DeviceOSVersion, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Device.OperatingSystem = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.DeviceType, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Device.Type = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.InternalAgentVersion, StringComparison.InvariantCulture))
                {
                    telemetry.Context.GetInternalContext().AgentVersion = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.InternalNodeName, StringComparison.InvariantCulture))
                {
                    telemetry.Context.GetInternalContext().NodeName = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.InternalSdkVersion, StringComparison.InvariantCulture))
                {
                    telemetry.Context.GetInternalContext().SdkVersion = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.LocationIp, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Location.Ip = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.OperationCorrelationVector, StringComparison.InvariantCulture))
                {
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.OperationId, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Operation.Id = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.OperationName, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Operation.Name = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.OperationParentId, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Operation.ParentId = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.OperationSyntheticSource, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Operation.SyntheticSource = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.SessionId, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Session.Id = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.SessionIsFirst, StringComparison.InvariantCulture))
                {
                    telemetry.Context.Session.IsFirst = false;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.UserAccountId, StringComparison.InvariantCulture))
                {
                    telemetry.Context.User.AccountId = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.UserAgent, StringComparison.InvariantCulture))
                {
                    telemetry.Context.User.UserAgent = tag.Value; //TODO: Deprecated???
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.UserAuthUserId, StringComparison.InvariantCulture))
                {
                    telemetry.Context.User.AuthenticatedUserId = tag.Value;
                }
                else if (string.Equals(tag.Key, TelemetryConverter.TagKeys.UserId, StringComparison.InvariantCulture))
                {
                    telemetry.Context.User.Id = tag.Value;
                }
                else
                {
                    // unknown tag, log and ignore
                    Common.Diagnostics.Log(FormattableString.Invariant($"Unknown tag. Ignoring. {tag.Key}"));
                }
            }
        }

        private static void PopulateFromProtobuf<TKey, TValue>(this IDictionary<TKey, TValue> destination, MapField<TKey, TValue> source)
        {
            foreach (var keyValuePair in source ?? new MapField<TKey, TValue>())
            {
                destination.Add(keyValuePair);
            }
        }

        private static Microsoft.ApplicationInsights.DataContracts.SeverityLevel? ConvertSeverityLevel(Inputs.Contracts.SeverityLevel inputSeverityLevel)
        {
            switch (inputSeverityLevel)
            {
                case Inputs.Contracts.SeverityLevel.Verbose:
                    return Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose;
                case Inputs.Contracts.SeverityLevel.Information:
                    return Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information;
                case Inputs.Contracts.SeverityLevel.Warning:
                    return Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Warning;
                case Inputs.Contracts.SeverityLevel.Error:
                    return Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Error;
                case Inputs.Contracts.SeverityLevel.Critical:
                    return Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Critical;
                default:
                    return null;
            }
        }
    }
}
