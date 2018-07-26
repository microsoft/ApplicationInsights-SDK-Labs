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
    using Google.Protobuf;
    using Microsoft.ApplicationInsights;
    using Opencensus.Proto.Trace;

    static class OpenCensusTelemetryConverter
    {
        private const string LINK_PROPERTY_NAME = "link";
        private const string LINK_SPAN_ID_PROPERTY_NAME = "spanId";
        private const string LINK_TRACE_ID_PROPERTY_NAME = "traceId";
        private const string LINK_TYPE_PROPERTY_NAME = "type";

        public static void TrackSpan(Span span, TelemetryClient telemetryClient)
        {
            if (span.Kind == Span.Types.SpanKind.Client ||
                (span.Kind == Span.Types.SpanKind.Unspecified && span.SameProcessAsParentSpan.GetValueOrDefault()))
            {
                TrackDependencyFromSpan(span, telemetryClient);
            }
            else
            {
                TrackRequestFromSpan(span, telemetryClient);
            }

            foreach (var evnt in span.TimeEvents.TimeEvent)
            {
                TrackTraceFromTimeEvent(evnt, span, telemetryClient);
            }
        }

        private static void TrackRequestFromSpan(Span span, TelemetryClient telemetryClient)
        {
            RequestTelemetry request = new RequestTelemetry();
            SetOperationContext(span, request.Context.Operation);

            //TODO:
            request.Id = ToStr(span.SpanId);
            request.Timestamp = span.StartTime.ToDateTime();
            request.Duration = span.EndTime.ToDateTime() - request.Timestamp;
            request.Success = span.Status.Code == 0;

            string host = null;
            string method = null;
            string path = null;
            string route = null;
            int port = -1;
            bool isResultSet = false;

            foreach (var attribute in span.Attributes.AttributeMap)
            {
                switch (attribute.Key)
                {
                    case "http.status_code":
                        request.ResponseCode = attribute.Value.StringValue.Value;
                        isResultSet = true;
                        break;
                    case "http.user_agent":
                        request.Context.User.UserAgent = attribute.Value.StringValue.Value;
                        break;
                    case "http.route":
                        route = attribute.Value.StringValue.Value;
                        break;
                    case "http.path":
                        path = attribute.Value.StringValue.Value;
                        break;
                    case "http.method":
                        method = attribute.Value.StringValue.Value;
                        break;
                    case "http.host":
                        host = attribute.Value.StringValue.Value;
                        break;
                    case "http.port":
                        port = (int)attribute.Value.IntValue;
                        break;
                    default:
                        if (!request.Properties.ContainsKey(attribute.Key))
                        {
                            request.Properties[attribute.Key] = attribute.Value.StringValue.Value;
                        }

                        break;
                }

                if (host != null)
                {
                    request.Url = GetUrl(host, port, path);
                    request.Name = $"{method} {route ?? path}";
                }
                else
                { // perhaps not http
                    request.Name = span.Name.Value;
                }

                if (!isResultSet)
                {
                    request.ResponseCode = span.Status.Message;
                }
            }

            SetLinks(span.Links, request.Properties);

            telemetryClient.TrackRequest(request);
        }

        private static void TrackDependencyFromSpan(Span span, TelemetryClient telemetryClient)
        {
            String host = null;
            if (span.Attributes.AttributeMap.ContainsKey("http.host"))
            {
                host = span.Attributes.AttributeMap["http.host"].StringValue.Value;
                if (IsApplicationInsightsUrl(host))
                {
                    return;
                }
            }

            DependencyTelemetry dependency = new DependencyTelemetry();
            SetOperationContext(span, dependency.Context.Operation);

            dependency.Id = ToStr(span.SpanId);
            dependency.Timestamp = span.StartTime.ToDateTime();
            dependency.Duration = span.EndTime.ToDateTime() - dependency.Timestamp;
            dependency.Success = span.Status.Code == 0;

            dependency.ResultCode = span.Status.Message;

            string method = null;
            string path = null;
            int port = -1;

            bool isHttp = false;
            bool isResultSet = false;
            foreach (var attribute in span.Attributes.AttributeMap)
            {
                switch (attribute.Key)
                {
                    case "http.status_code":
                        dependency.ResultCode = attribute.Value.StringValue.Value;
                        isHttp = true;
                        isResultSet = true;
                        break;
                    case "http.path":
                        path = attribute.Value.StringValue.Value;
                        isHttp = true;
                        break;
                    case "http.method":
                        method = attribute.Value.StringValue.Value;
                        isHttp = true;
                        break;
                    case "http.host":
                        break;
                    case "http.port":
                        port = (int)attribute.Value.IntValue;
                        break;
                    default:
                        if (!dependency.Properties.ContainsKey(attribute.Key))
                        {
                            dependency.Properties[attribute.Key] = attribute.Value.StringValue.Value;
                        }

                        break;
                }
            }

            dependency.Target = host;
            if (isHttp)
            {
                dependency.Type = "HTTP";
            }

            if (!isResultSet)
            {
                dependency.ResultCode = span.Status.Message;
            }

            if (host != null)
            {
                dependency.Data = GetUrl(host, port, path).ToString();
            }

            if (method != null && path != null)
            {
                dependency.Name = $"{method} {path}";
            }
            else
            {
                dependency.Name = span.Name.Value;
            }

            SetLinks(span.Links, dependency.Properties);

            telemetryClient.TrackDependency(dependency);
        }

        private static bool IsApplicationInsightsUrl(string host)
        {
            return host.StartsWith("dc.services.visualstudio.com")
                   || host.StartsWith("rt.services.visualstudio.com");
        }

        private static void TrackTraceFromTimeEvent(Span.Types.TimeEvent evnt, Span span, TelemetryClient telemetryClient)
        {
            Span.Types.TimeEvent.Types.Annotation annotation = evnt.Annotation;
            if (annotation != null)
            {
                TraceTelemetry trace = new TraceTelemetry();
                SetParentOperationContext(span, trace.Context.Operation);
                trace.Timestamp = evnt.Time.ToDateTime();

                trace.Message = annotation.Description.Value;
                SetAttributes(annotation.Attributes.AttributeMap, trace.Properties);
                telemetryClient.TrackTrace(trace);
            }

            Span.Types.TimeEvent.Types.MessageEvent message = evnt.MessageEvent;
            if (message != null)
            {
                TraceTelemetry trace = new TraceTelemetry();
                SetParentOperationContext(span, trace.Context.Operation);
                trace.Timestamp = evnt.Time.ToDateTime();

                trace.Message = $"MessageEvent. messageId: '{message.Id}', type: '{message.Type}', compressed size: '{message.CompressedSize}', uncompressed size: '{message.UncompressedSize}'";
                telemetryClient.TrackTrace(trace);
            }
        }

        private static void SetOperationContext(Span span, OperationContext context)
        {
            context.Id = ToStr(span.TraceId);
            context.ParentId = ToStr(span.ParentSpanId);
        }

        private static void SetParentOperationContext(Span span, OperationContext context)
        {
            context.Id = ToStr(span.TraceId);
            context.ParentId = ToStr(span.SpanId);
        }

        private static string ToStr(ByteString str)
        {
            return BitConverter.ToString(str.ToByteArray()).Replace("-", "").ToLower();
        }

        private static Uri GetUrl(String host, int port, String path)
        {
            // todo: better way to determine scheme?
            String schema = port == 80 ? "http" : "https";
            if (port == 80 || port == 443)
            {
                return new Uri(string.Format("{0}://{1}{2}", schema, host, path));
            }

            return new Uri($"{schema}://{host}:{port}{path}");
        }

        private static void SetLinks(Span.Types.Links spanLinks, IDictionary<string, string> telemetryProperties)
        {
            // for now, we just put links to telemetry properties
            // link0_spanId = ...
            // link0_traceId = ...
            // link0_type = child | parent | other
            // link0_<attributeKey> = <attributeValue>
            // this is not convenient for querying data
            // We'll consider adding Links to operation telemetry schema

            int num = 0;
            foreach (var link in spanLinks.Link)
            {
                string prefix = $"{LINK_PROPERTY_NAME}{num++}_";
                telemetryProperties[prefix + LINK_SPAN_ID_PROPERTY_NAME] = ToStr(link.SpanId);
                telemetryProperties[prefix + LINK_TRACE_ID_PROPERTY_NAME] = ToStr(link.TraceId);
                telemetryProperties[prefix + LINK_TYPE_PROPERTY_NAME] = link.Type.ToString();

                foreach (var attribute in link.Attributes.AttributeMap)
                {
                    if (!telemetryProperties.ContainsKey(attribute.Key))
                    {
                        telemetryProperties[attribute.Key] = attribute.Value.StringValue.Value;
                    }
                }
            }
        }

        private static void SetAttributes(IDictionary<string, AttributeValue> attributes, IDictionary<string, string> telemetryProperties)
        {
            foreach (var attribute in attributes)
            {
                if (!telemetryProperties.ContainsKey(attribute.Key))
                {
                    telemetryProperties[attribute.Key] = attribute.Value.StringValue.Value;
                }
            }
        }
    }
}