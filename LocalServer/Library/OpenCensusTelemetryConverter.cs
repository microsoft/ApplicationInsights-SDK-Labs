namespace Microsoft.LocalForwarder.Library
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using ApplicationInsights;
    using ApplicationInsights.Channel;
    using ApplicationInsights.DataContracts;
    using ApplicationInsights.Extensibility.Implementation;
    using Google.Protobuf;
    using Opencensus.Proto.Trace;

    static class OpenCensusTelemetryConverter
    {
        public static class SpanAttributeConstants
        {
            public const string SpanKindKey = "span.kind";

            public const string ServerSpanKind = "server";
            public const string ClientSpanKind = "client";
            public const string ProducerSpanKind = "producer";
            public const string ConsumerSpanKind = "consumer";

            public const string HttpUrlKey = "http.url";
            public const string HttpMethodKey = "http.method";
            public const string HttpStatusCodeKey = "http.status_code";
            public const string HttpPathKey = "http.path";
            public const string HttpHostKey = "http.host";
            public const string HttpPortKey = "http.port";
            public const string HttpRouteKey = "http.route";
            public const string HttpUserAgentKey = "http.user_agent";

            public const string ErrorKey = "error";
            public const string ErrorStackTrace = "error.stack.trace";
        }

        private const string StatusDescriptionPropertyName = "statusDescription";
        private const string LinkPropertyName = "link";
        private const string LinkSpanIdPropertyName = "spanId";
        private const string LinkTraceIdPropertyName = "traceId";
        private const string LinkTypePropertyName = "type";
        private const string SdkVersion = "oclf"; // todo version
        private static readonly uint[] Lookup32 = CreateLookup32();

        public static void TrackSpan(this TelemetryClient telemetryClient, Span span, string ikey)
        {
            if (span == null)
            {
                return;
            }

            if (GetSpanKind(span) == Span.Types.SpanKind.Client)
            {
                telemetryClient.TrackDependencyFromSpan(span, ikey);
            }
            else
            {
                telemetryClient.TrackRequestFromSpan(span, ikey);
            }

            if (span.TimeEvents != null)
            {
                foreach (var evnt in span.TimeEvents.TimeEvent)
                {
                    telemetryClient.TrackTraceFromTimeEvent(evnt, span, ikey);
                }
            }
        }

        private static Span.Types.SpanKind GetSpanKind(Span span)
        {
            if (span.Attributes?.AttributeMap != null && span.Attributes.AttributeMap.TryGetValue(SpanAttributeConstants.SpanKindKey, out var value))
            {
                return value.StringValue?.Value == SpanAttributeConstants.ClientSpanKind ? Span.Types.SpanKind.Client : Span.Types.SpanKind.Server;
            }

            if (span.Kind == Span.Types.SpanKind.Unspecified)
            {
                if (span.SameProcessAsParentSpan.HasValue && !span.SameProcessAsParentSpan.Value)
                {
                    return Span.Types.SpanKind.Server;
                }

                return Span.Types.SpanKind.Client;
            }

            return span.Kind;
        }

        private static void TrackRequestFromSpan(this TelemetryClient telemetryClient, Span span, string ikey)
        {
            RequestTelemetry request = new RequestTelemetry();

            InitializeOperationTelemetry(request, span);
            request.ResponseCode = span.Status?.Message;

            string host = null, method = null, path = null, route = null, url = null;
            int port = -1;

            if (span.Attributes?.AttributeMap != null)
            {
                foreach (var attribute in span.Attributes.AttributeMap)
                {
                    switch (attribute.Key)
                    {
                        case SpanAttributeConstants.HttpUrlKey:
                            url = attribute.Value.StringValue?.Value;
                            break;
                        case SpanAttributeConstants.HttpStatusCodeKey:
                            request.ResponseCode = attribute.Value.IntValue.ToString();
                            break;
                        case SpanAttributeConstants.HttpUserAgentKey:
                            request.Context.User.UserAgent = attribute.Value.StringValue?.Value;
                            break;
                        case SpanAttributeConstants.HttpRouteKey:
                            route = attribute.Value.StringValue?.Value;
                            break;
                        case SpanAttributeConstants.HttpPathKey:
                            path = attribute.Value.StringValue?.Value;
                            break;
                        case SpanAttributeConstants.HttpMethodKey:
                            method = attribute.Value.StringValue?.Value;
                            break;
                        case SpanAttributeConstants.HttpHostKey:
                            host = attribute.Value.StringValue?.Value;
                            break;
                        case SpanAttributeConstants.HttpPortKey:
                            port = (int) attribute.Value?.IntValue;
                            break;
                        case SpanAttributeConstants.ErrorKey:
                            if (attribute.Value != null && attribute.Value.BoolValue)
                            {
                                request.Success = false;
                            }
                            break;
                        default:
                            SetCustomProperty(request, attribute);

                            break;
                    }
                }

                if (url != null)
                {
                    request.Url = new Uri(url);
                    request.Name = GetHttpTelemetryName(method, request.Url.AbsolutePath, route);
                }
                else
                {
                    request.Url = GetUrl(host, port, path);
                    request.Name = GetHttpTelemetryName(method, path, route);
                }
            }

            request.Context.InstrumentationKey = ikey;
            telemetryClient.TrackRequest(request);
        }

        private static void TrackDependencyFromSpan(this TelemetryClient telemetryClient, Span span, string ikey)
        {
            string host = GetHost(span.Attributes?.AttributeMap);
            if (IsApplicationInsightsUrl(host))
            {
                return;
            }

            DependencyTelemetry dependency = new DependencyTelemetry();

            // https://github.com/Microsoft/ApplicationInsights-dotnet/issues/876
            dependency.Success = null;

            InitializeOperationTelemetry(dependency, span);

            dependency.ResultCode = span.Status?.Code.ToString();

            if (span.Attributes?.AttributeMap != null)
            {
                string method = null, path = null, url = null;
                int port = -1;

                bool isHttp = false;
                foreach (var attribute in span.Attributes.AttributeMap)
                {
                    switch (attribute.Key)
                    {
                        case SpanAttributeConstants.HttpUrlKey:
                            url = attribute.Value.StringValue?.Value;
                            break;
                        case SpanAttributeConstants.HttpStatusCodeKey:
                            dependency.ResultCode = attribute.Value?.IntValue.ToString();
                            isHttp = true;
                            break;
                        case SpanAttributeConstants.HttpPathKey:
                            path = attribute.Value.StringValue.Value;
                            isHttp = true;
                            break;
                        case SpanAttributeConstants.HttpMethodKey:
                            method = attribute.Value.StringValue.Value;
                            isHttp = true;
                            break;
                        case SpanAttributeConstants.HttpHostKey:
                            break;
                        case SpanAttributeConstants.HttpPortKey:
                            port = (int) attribute.Value.IntValue;
                            break;
                        case SpanAttributeConstants.ErrorKey:
                            if (attribute.Value != null && attribute.Value.BoolValue)
                            {
                                dependency.Success = false;
                            }

                            break;
                        default:
                            SetCustomProperty(dependency, attribute);
                            break;
                    }
                }

                dependency.Target = host;
                if (isHttp)
                {
                    dependency.Type = "Http";
                }

                if (url != null)
                {
                    dependency.Data = url;
                    if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
                    {
                        dependency.Name = GetHttpTelemetryName(method, uri.AbsolutePath, null);
                    }
                }
                else
                {
                    dependency.Data = GetUrl(host, port, path)?.ToString();
                    dependency.Name = GetHttpTelemetryName(method, path, null);
                }
            }

            dependency.Context.InstrumentationKey = ikey;
            telemetryClient.TrackDependency(dependency);
        }

        private static bool IsApplicationInsightsUrl(string host)
        {
            return host != null && (host.StartsWith("dc.services.visualstudio.com")
                   || host.StartsWith("rt.services.visualstudio.com"));
        }

        private static void TrackTraceFromTimeEvent(this TelemetryClient telemetryClient, Span.Types.TimeEvent evnt, Span span, string ikey)
        {
            Span.Types.TimeEvent.Types.Annotation annotation = evnt.Annotation;
            if (annotation != null)
            {
                telemetryClient.TrackTrace(span, evnt, annotation.Description.Value, ikey,
                    annotation.Attributes?.AttributeMap);
            }

            Span.Types.TimeEvent.Types.MessageEvent message = evnt.MessageEvent;
            if (message != null)
            {
                telemetryClient.TrackTrace(span, evnt,
                    $"MessageEvent. messageId: '{message.Id}', type: '{message.Type}', compressed size: '{message.CompressedSize}', uncompressed size: '{message.UncompressedSize}'", ikey);
            }
        }

        private static void TrackTrace(this TelemetryClient telemetryClient, 
            Span span, 
            Span.Types.TimeEvent evnt,
            string message,
            string ikey,
            IDictionary<string, AttributeValue> attributes = null)
        {
            TraceTelemetry trace = new TraceTelemetry(message);
            SetSdkVersion(trace);
            SetParentOperationContext(span, trace.Context.Operation);
            trace.Timestamp = evnt.Time?.ToDateTime() ?? DateTime.UtcNow;
            if (attributes != null)
            {
                foreach (var attribute in attributes)
                {
                    SetCustomProperty(trace, attribute);
                }
            }

            trace.Context.InstrumentationKey = ikey;
            telemetryClient.TrackTrace(trace);
        }

        private static void InitializeOperationTelemetry(OperationTelemetry telemetry, Span span)
        {
            telemetry.Name = span.Name?.Value;
            SetSdkVersion(telemetry);

            var now = DateTime.UtcNow;
            telemetry.Timestamp = span.StartTime?.ToDateTime() ?? now;
            var endTime = span.EndTime?.ToDateTime() ?? now;

            SetOperationContext(span, telemetry);
            telemetry.Duration = endTime - telemetry.Timestamp;

            if (span.Status != null)
            {
                telemetry.Success = span.Status.Code == 0;
                if (!string.IsNullOrEmpty(span.Status.Message))
                {
                    telemetry.Properties[StatusDescriptionPropertyName] = span.Status.Message;
                }
            }

            SetLinks(span.Links, telemetry.Properties);
        }

        private static void SetOperationContext(Span span, OperationTelemetry telemetry)
        {
            string traceId = BytesStringToHexString(span.TraceId);
            telemetry.Context.Operation.Id = BytesStringToHexString(span.TraceId);
            telemetry.Context.Operation.ParentId = BytesStringToHexString(span.ParentSpanId);
            telemetry.Id = $"|{traceId}.{BytesStringToHexString(span.SpanId)}.";
        }

        private static void SetParentOperationContext(Span span, OperationContext context)
        {
            context.Id = BytesStringToHexString(span.TraceId);
            context.ParentId = $"|{context.Id}.{BytesStringToHexString(span.SpanId)}.";
        }

        private static Uri GetUrl(String host, int port, String path)
        {
            if (host == null)
            {
                return null;
            }

            String scheme = port == 80 ? "http" : "https";
            if (port < 0 || port == 80 || port == 443)
            {
                return new Uri($"{scheme}://{host}/{path}");
            }

            return new Uri($"{scheme}://{host}:{port}/{path}");
        }

        private static string GetHttpTelemetryName(string method, string path, string route)
        {
            if (method == null && path == null && route == null)
            {
                return null;
            }

            if (path == null && route == null)
            {
                return method;
            }

            if (method == null)
            {
                return route ?? path;
            }

            return method + " " + (route ?? path);
        }

        private static void SetLinks(Span.Types.Links spanLinks, IDictionary<string, string> telemetryProperties)
        {
            if (spanLinks == null)
            {
                return;
            }

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
                string prefix = $"{LinkPropertyName}{num++}_";
                telemetryProperties[prefix + LinkSpanIdPropertyName] = BytesStringToHexString(link.SpanId);
                telemetryProperties[prefix + LinkTraceIdPropertyName] = BytesStringToHexString(link.TraceId);
                telemetryProperties[prefix + LinkTypePropertyName] = link.Type.ToString();

                if (link.Attributes?.AttributeMap != null)
                {
                    foreach (var attribute in link.Attributes.AttributeMap)
                    {
                        telemetryProperties[prefix + attribute.Key] = attribute.Value.StringValue.Value;
                    }
                }
            }
        }

        private static void SetSdkVersion(ITelemetry telemetry)
        {
            telemetry.Context.GetInternalContext().SdkVersion = SdkVersion;
        }

        private static string GetHost(IDictionary<string, AttributeValue> attributes)
        {
            if (attributes != null)
            {
                if (attributes.TryGetValue(SpanAttributeConstants.HttpUrlKey, out var urlAttribute))
                {
                    if (urlAttribute != null &&
                        Uri.TryCreate(urlAttribute.StringValue.Value, UriKind.Absolute, out var uri))
                    {
                        return uri.Host;
                    }
                }

                if (attributes.TryGetValue(SpanAttributeConstants.HttpHostKey, out var hostAttribute))
                {
                    return hostAttribute.StringValue?.Value;
                }
            }

            return null;
        }

        private static void SetCustomProperty(ISupportProperties telemetry, KeyValuePair<string, AttributeValue> attribute)
        {
            if (telemetry.Properties.ContainsKey(attribute.Key))
            {
                return;
            }

            switch (attribute.Value.ValueCase)
            {
                case AttributeValue.ValueOneofCase.StringValue:
                    telemetry.Properties[attribute.Key] = attribute.Value.StringValue?.Value;
                    break;
                case AttributeValue.ValueOneofCase.BoolValue:
                    telemetry.Properties[attribute.Key] = attribute.Value.BoolValue.ToString();
                    break;
                case AttributeValue.ValueOneofCase.IntValue:
                    telemetry.Properties[attribute.Key] = attribute.Value.IntValue.ToString();
                    break;
            }
        }

        /// <summary>
        /// Converts protobuf ByteString to hex-encoded low string
        /// </summary>
        /// <returns>Hex string</returns>
        private static string BytesStringToHexString(ByteString bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            // See https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/24343727#24343727
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = Lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[(2 * i) + 1] = (char)(val >> 16);
            }

            return new string(result);
        }

        private static uint[] CreateLookup32()
        {
            // See https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/24343727#24343727
            var result = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                string s = i.ToString("x2", CultureInfo.InvariantCulture);
                result[i] = s[0] + ((uint)s[1] << 16);
            }

            return result;
        }
    }
}