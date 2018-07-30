namespace Test.Library
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using global::Library;
    using Google.Protobuf;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Opencensus.Proto.Trace;
    using static Opencensus.Proto.Trace.Span.Types;

    [TestClass]
    public class OpenCensusTelemetryConverterTests
    {
        private const string TestTraceId = "d79bdda7eb9c4a9fa9bda52fe7b48b95";
        private const string TestSpanId = "d7ddeb4aa9a5e78b";
        private const string TestParentSpanId = "9ba79c9fbd2fb495";

        private readonly byte[] testTraceIdBytes = {0xd7, 0x9b, 0xdd, 0xa7, 0xeb, 0x9c, 0x4a, 0x9f, 0xa9, 0xbd, 0xa5, 0x2f, 0xe7, 0xb4, 0x8b, 0x95};
        private readonly byte[] testSpanIdBytes = {0xd7, 0xdd, 0xeb, 0x4a, 0xa9, 0xa5, 0xe7, 0x8b};
        private readonly byte[] testParentSpanIdBytes = {0x9b, 0xa7, 0x9c, 0x9f, 0xbd, 0x2f, 0xb4, 0x95};

        private TelemetryConfiguration configuration;
        private StubTelemetryChannel channel;
        private TelemetryClient client;
        private readonly ConcurrentQueue<ITelemetry> sentItems = new ConcurrentQueue<ITelemetry>();

        [TestInitialize]
        public void Setup()
        {
            configuration = new TelemetryConfiguration();
            channel = new StubTelemetryChannel
            {
                OnSend = t => sentItems.Enqueue(t)
            };

            configuration.TelemetryChannel = channel;
            client = new TelemetryClient(configuration);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequest()
        {
            // ARRANGE
            var now = DateTime.UtcNow;

            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.StartTime = now.AddSeconds(-1).ToTimestamp();
            span.EndTime = now.ToTimestamp();

            // ACT
            client.TrackSpan(span);

            // ASSERT
            Assert.AreEqual(1, sentItems.Count);
            Assert.IsInstanceOfType(sentItems.Single(), typeof(RequestTelemetry));

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual("spanName", request.Name);
            Assert.AreEqual(now.AddSeconds(-1), request.Timestamp);
            Assert.AreEqual(1, request.Duration.TotalSeconds);

            Assert.AreEqual(TestTraceId, request.Context.Operation.Id);
            Assert.IsNull(request.Context.Operation.ParentId);
            Assert.AreEqual($"|{TestTraceId}.{TestSpanId}.", request.Id);

            Assert.IsFalse(request.Success.HasValue);
            Assert.IsTrue(string.IsNullOrEmpty(request.ResponseCode));

            Assert.AreEqual("oclf", request.Context.GetInternalContext().SdkVersion);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithParent()
        {
            // ARRANGE
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.ParentSpanId = ByteString.CopyFrom(testParentSpanIdBytes, 0, 8);

            // ACT
            client.TrackSpan(span);

            // ASSERT
            Assert.AreEqual(TestParentSpanId, ((RequestTelemetry)sentItems.Single()).Context.Operation.ParentId);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithStatus()
        {
            // ARRANGE
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.Status = new Status {Code = 0};

            // ACT
            client.TrackSpan(span);

            // ASSERT
            var request = (RequestTelemetry)sentItems.Single();

            Assert.IsTrue(request.Success.HasValue);
            Assert.IsTrue(request.Success.Value);
            Assert.IsTrue(string.IsNullOrEmpty(request.ResponseCode));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithStatusAndDescription()
        {
            // ARRANGE
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.Status = new Status {Code = 0, Message = "all good"};

            // ACT
            client.TrackSpan(span);

            // ASSERT
            var request = (RequestTelemetry)sentItems.Single();

            Assert.IsTrue(request.Success.HasValue);
            Assert.IsTrue(request.Success.Value);
            Assert.AreEqual("all good", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithNonSuccessStatusAndDescription()
        {
            // ARRANGE
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.Status = new Status { Code = 1, Message = "all bad" };

            // ACT
            client.TrackSpan(span);

            // ASSERT
            var request = (RequestTelemetry)sentItems.Single();

            Assert.IsTrue(request.Success.HasValue);
            Assert.IsFalse(request.Success.Value);
            Assert.AreEqual("all bad", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestErrorAttribute()
        {
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.Attributes = new Attributes
            {
                AttributeMap = { ["error"] = CreateAttributeValue(true) }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.IsTrue(request.Success.HasValue);
            Assert.IsFalse(request.Success.Value);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependency()
        {
            // ARRANGE
            var now = DateTime.UtcNow;

            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.StartTime = now.AddSeconds(-1).ToTimestamp();
            span.EndTime = now.ToTimestamp();

            // ACT
            client.TrackSpan(span);

            // ASSERT
            Assert.AreEqual(1, sentItems.Count);
            Assert.IsInstanceOfType(sentItems.Single(), typeof(DependencyTelemetry));

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual("spanName", dependency.Name);
            Assert.AreEqual(now.AddSeconds(-1), dependency.Timestamp);
            Assert.AreEqual(1, dependency.Duration.TotalSeconds);

            Assert.AreEqual(TestTraceId, dependency.Context.Operation.Id);
            Assert.IsNull(dependency.Context.Operation.ParentId);
            Assert.AreEqual($"|{TestTraceId}.{TestSpanId}.", dependency.Id);

            Assert.IsTrue(string.IsNullOrEmpty(dependency.ResultCode));
            Assert.IsFalse(dependency.Success.HasValue);

            Assert.AreEqual("oclf", dependency.Context.GetInternalContext().SdkVersion);

            Assert.IsTrue(string.IsNullOrEmpty(dependency.Type));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithParent()
        {
            // ARRANGE
            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.ParentSpanId = ByteString.CopyFrom(testParentSpanIdBytes, 0, 8);

            // ACT
            client.TrackSpan(span);

            // ASSERT
            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual(TestParentSpanId, dependency.Context.Operation.ParentId);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithStatus()
        {
            // ARRANGE
            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.Status = new Status {Code = 0};

            // ACT
            client.TrackSpan(span);

            // ASSERT
            var dependency = (DependencyTelemetry)sentItems.Single();

            Assert.IsTrue(dependency.Success.HasValue);
            Assert.IsTrue(dependency.Success.Value);
            Assert.AreEqual("0", dependency.ResultCode);
            Assert.IsFalse(dependency.Properties.ContainsKey("StatusDescription"));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithStatusAndDescription()
        {
            // ARRANGE
            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.Status = new Status {Code = 0, Message = "all good"};

            // ACT
            client.TrackSpan(span);

            // ASSERT
            var dependency = (DependencyTelemetry)sentItems.Single();

            Assert.IsTrue(dependency.Success.HasValue);
            Assert.IsTrue(dependency.Success.Value);

            Assert.AreEqual("0", dependency.ResultCode);
            Assert.IsTrue(dependency.Properties.ContainsKey("statusDescription"));
            Assert.AreEqual("all good", dependency.Properties["statusDescription"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithNonSuccessStatusAndDescription()
        {
            // ARRANGE
            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.Status = new Status { Code = 1, Message = "all bad" };

            // ACT
            client.TrackSpan(span);

            // ASSERT
            var dependency = (DependencyTelemetry)sentItems.Single();

            Assert.IsTrue(dependency.Success.HasValue);
            Assert.IsFalse(dependency.Success.Value);
            Assert.AreEqual("1", dependency.ResultCode);
            Assert.IsTrue(dependency.Properties.ContainsKey("statusDescription"));
            Assert.AreEqual("all bad", dependency.Properties["statusDescription"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyErrorAttribute()
        {
            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.Attributes = new Attributes
            {
                AttributeMap = { ["error"] = CreateAttributeValue(true) }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.IsTrue(dependency.Success.HasValue);
            Assert.IsFalse(dependency.Success.Value);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestBasedOnSpanKindAttribute()
        {
            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.Attributes = new Attributes()
            {
                AttributeMap = { ["span.kind"] = CreateAttributeValue("server") }
            };

            client.TrackSpan(span);

            Assert.IsInstanceOfType(sentItems.Single(), typeof(RequestTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestBasedOnSpanKindProperty()
        {
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.SameProcessAsParentSpan = null;
            span.ParentSpanId = ByteString.CopyFrom(testParentSpanIdBytes, 0, 8);
            client.TrackSpan(span);

            Assert.IsInstanceOfType(sentItems.Single(), typeof(RequestTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyBasedOnSpanKindProperty()
        {
            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.SameProcessAsParentSpan = null;
            span.ParentSpanId = ByteString.CopyFrom(testParentSpanIdBytes, 0, 8);

            client.TrackSpan(span);

            Assert.IsInstanceOfType(sentItems.Single(), typeof(DependencyTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependenciesBasedOnSpanKindAttribute()
        {
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.Attributes = new Attributes
            {
                AttributeMap = { ["span.kind"] = CreateAttributeValue("client") }
            };

            client.TrackSpan(span);

            Assert.IsInstanceOfType(sentItems.Single(), typeof(DependencyTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestBasedOnSameProcessAsParentFlag()
        {
            var span = CreateBasicSpan(SpanKind.Unspecified, "spanName");
            span.SameProcessAsParentSpan = false;
            client.TrackSpan(span);

            Assert.IsInstanceOfType(sentItems.Single(), typeof(RequestTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDepednencyBasedOnSameProcessAsParentFlag()
        {
            var span = CreateBasicSpan(SpanKind.Unspecified, "spanName");
            span.SameProcessAsParentSpan = true;
            client.TrackSpan(span);

            Assert.IsInstanceOfType(sentItems.Single(), typeof(DependencyTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDepednencyBasedOnSameProcessAsParentFlagNotSet()
        {
            var span = CreateBasicSpan(SpanKind.Unspecified, "spanName");
            span.SameProcessAsParentSpan = null;
            client.TrackSpan(span);

            Assert.IsInstanceOfType(sentItems.Single(), typeof(DependencyTelemetry));
        }


        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithoutName()
        {
            var span = new Span
            {
                Kind = SpanKind.Server,
                TraceId = ByteString.CopyFrom(testTraceIdBytes, 0, 16),
                SpanId = ByteString.CopyFrom(testSpanIdBytes, 0, 8),
            };

            client.TrackSpan(span);

            Assert.IsNull(sentItems.OfType<RequestTelemetry>().Single().Name);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithoutKind()
        {
            var span = new Span
            {
                TraceId = ByteString.CopyFrom(testTraceIdBytes, 0, 16),
                SpanId = ByteString.CopyFrom(testSpanIdBytes, 0, 8),
                Name = new TruncatableString { Value = "spanName" }
            };

            client.TrackSpan(span);

            Assert.IsInstanceOfType(sentItems.Single(), typeof(DependencyTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithoutStartAndEndTime()
        {
            var span = new Span
            {
                Kind = SpanKind.Server,
                TraceId = ByteString.CopyFrom(testTraceIdBytes, 0, 16),
                SpanId = ByteString.CopyFrom(testSpanIdBytes, 0, 8),
                Name = new TruncatableString { Value = "spanName" }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.IsTrue(Math.Abs((request.Timestamp - DateTime.UtcNow).TotalSeconds) < 1);
            Assert.AreEqual(0, request.Duration.TotalSeconds);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestWithUrl()
        {
            var url = new Uri("https://host:123/path?query");
            var span = CreateBasicSpan(SpanKind.Server, "HttpIn");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = CreateAttributeValue(url),
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.status_code"] = CreateAttributeValue(409),
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(url.ToString(), request.Url.ToString());
            Assert.AreEqual("POST /path", request.Name);
            Assert.AreEqual("409", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestWithUrlAndRoute()
        {
            var url = new Uri("https://host:123/path?query");
            var span = CreateBasicSpan(SpanKind.Server, "HttpIn");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = CreateAttributeValue(url),
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.route"] = CreateAttributeValue("route"),
                    ["http.status_code"] = CreateAttributeValue(503),
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(url.ToString(), request.Url.ToString());
            Assert.AreEqual("POST route", request.Name);
            Assert.AreEqual("503", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestWithUrlAndNoMethod()
        {
            var url = new Uri("https://host:123/path?query");
            var span = CreateBasicSpan(SpanKind.Server, "HttpIn");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = CreateAttributeValue(url),
                    ["http.status_code"] = CreateAttributeValue(200),
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(url.ToString(), request.Url.ToString());
            Assert.AreEqual("/path", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestWithUrlOtherAttributesAreIgnored()
        {
            var url = new Uri("https://host:123/path?query");
            var span = CreateBasicSpan(SpanKind.Server, "HttpIn");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = CreateAttributeValue(url),
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.path"] = CreateAttributeValue("another path"),
                    ["http.host"] = CreateAttributeValue("another host"),
                    ["http.port"] = CreateAttributeValue(8080),
                    ["http.status_code"] = CreateAttributeValue(200),
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(url.ToString(), request.Url.ToString());
            Assert.AreEqual("POST /path", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestHostPortPathAttributes()
        {
            var span = CreateBasicSpan(SpanKind.Server, "HttpIn");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.path"] = CreateAttributeValue("path"),
                    ["http.host"] = CreateAttributeValue("host"),
                    ["http.port"] = CreateAttributeValue(123),
                    ["http.status_code"] = CreateAttributeValue(200),
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual("https://host:123/path", request.Url.ToString());
            Assert.AreEqual("POST path", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestHostPathAttributes()
        {
            var span = CreateBasicSpan(SpanKind.Server, "HttpIn");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.path"] = CreateAttributeValue("path"),
                    ["http.host"] = CreateAttributeValue("host"),
                    ["http.status_code"] = CreateAttributeValue(200),
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual("https://host/path", request.Url.ToString());
            Assert.AreEqual("POST path", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestHostAttributes()
        {
            var span = CreateBasicSpan(SpanKind.Server, "HttpIn");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.host"] = CreateAttributeValue("host"),
                    ["http.status_code"] = CreateAttributeValue(200),
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual("https://host/", request.Url.ToString());
            Assert.AreEqual("POST", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestOnlyMethodAttributes()
        {
            var span = CreateBasicSpan(SpanKind.Server, "HttpIn");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.status_code"] = CreateAttributeValue(200),
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.IsNull(request.Url);
            Assert.AreEqual("POST", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestUserAgent()
        {
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";

            var span = CreateBasicSpan(SpanKind.Server, "HttpIn");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = CreateAttributeValue("https://host/path"),
                    ["http.user_agent"] = CreateAttributeValue(userAgent)
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(userAgent, request.Context.User.UserAgent);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithUrl()
        {
            var url = new Uri("https://host:123/path?query");
            var span = CreateBasicSpan(SpanKind.Client, "HttpOut");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = CreateAttributeValue(url),
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.status_code"] = CreateAttributeValue(200)
                }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual(url.ToString(), dependency.Data);
            Assert.AreEqual("POST /path", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.AreEqual("host", dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithUrlIgnoresHostPortPath()
        {
            var url = new Uri("https://host:123/path?query");
            var span = CreateBasicSpan(SpanKind.Client, "HttpOut");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = CreateAttributeValue(url),
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.path"] = CreateAttributeValue("another path"),
                    ["http.host"] = CreateAttributeValue("another host"),
                    ["http.port"] = CreateAttributeValue(8080),
                    ["http.status_code"] = CreateAttributeValue(200)
                }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual(url.ToString(), dependency.Data);
            Assert.AreEqual("POST /path", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.AreEqual("host", dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithHostPortPath()
        {
            var span = CreateBasicSpan(SpanKind.Client, "HttpOut");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.path"] = CreateAttributeValue("path"),
                    ["http.host"] = CreateAttributeValue("host"),
                    ["http.port"] = CreateAttributeValue(123),
                    ["http.status_code"] = CreateAttributeValue(200)
                }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual("https://host:123/path", dependency.Data);
            Assert.AreEqual("POST path", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.AreEqual("host", dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithHostPort()
        {
            var span = CreateBasicSpan(SpanKind.Client, "HttpOut");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {

                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.host"] = CreateAttributeValue("host"),
                    ["http.port"] = CreateAttributeValue(123),
                    ["http.status_code"] = CreateAttributeValue(200)
                }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual("https://host:123/", dependency.Data);
            Assert.AreEqual("POST", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.AreEqual("host", dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithHost()
        {
            var span = CreateBasicSpan(SpanKind.Client, "HttpOut");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {

                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.host"] = CreateAttributeValue("host"),
                    ["http.status_code"] = CreateAttributeValue(200)
                }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual("https://host/", dependency.Data);
            Assert.AreEqual("POST", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.AreEqual("host", dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithMethod()
        {
            var span = CreateBasicSpan(SpanKind.Client, "HttpOut");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = CreateAttributeValue("POST"),
                    ["http.status_code"] = CreateAttributeValue(200)
                }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.IsNull(dependency.Data);
            Assert.AreEqual("POST", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.IsNull(dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithStatusCodeOnly()
        {
            var span = CreateBasicSpan(SpanKind.Client, "HttpOut");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["http.status_code"] = CreateAttributeValue(200)
                }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.IsNull(dependency.Data);
            Assert.IsNull(dependency.Name);
            Assert.IsNull(dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
            Assert.AreEqual("200", dependency.ResultCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithCustomAttributes()
        {
            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["custom.stringAttribute"] = CreateAttributeValue("string"),
                    ["custom.longAttribute"] = CreateAttributeValue(long.MaxValue),
                    ["custom.boolAttribute"] = CreateAttributeValue(true)
                }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual(3, dependency.Properties.Count);
            Assert.IsTrue(dependency.Properties.ContainsKey("custom.stringAttribute"));
            Assert.AreEqual("string", dependency.Properties["custom.stringAttribute"]);

            Assert.IsTrue(dependency.Properties.ContainsKey("custom.longAttribute"));
            Assert.AreEqual(long.MaxValue.ToString(), dependency.Properties["custom.longAttribute"]);

            Assert.IsTrue(dependency.Properties.ContainsKey("custom.boolAttribute"));
            Assert.AreEqual(bool.TrueString, dependency.Properties["custom.boolAttribute"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestsWithCustomAttributes()
        {
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.Attributes = new Attributes
            {
                AttributeMap =
                {
                    ["custom.stringAttribute"] = CreateAttributeValue("string"),
                    ["custom.longAttribute"] = CreateAttributeValue(long.MaxValue),
                    ["custom.boolAttribute"] = CreateAttributeValue(true)
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(3, request.Properties.Count);
            Assert.IsTrue(request.Properties.ContainsKey("custom.stringAttribute"));
            Assert.AreEqual("string", request.Properties["custom.stringAttribute"]);

            Assert.IsTrue(request.Properties.ContainsKey("custom.longAttribute"));
            Assert.AreEqual(long.MaxValue.ToString(), request.Properties["custom.longAttribute"]);

            Assert.IsTrue(request.Properties.ContainsKey("custom.boolAttribute"));
            Assert.AreEqual(bool.TrueString, request.Properties["custom.boolAttribute"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithLinks()
        {
            var (link0TraceId, link0TraceIdBytes) = GenerateRandomId(16);
            var (link1TraceId, link1TraceIdBytes) = GenerateRandomId(16);
            var (link2TraceId, link2TraceIdBytes) = GenerateRandomId(16);

            var (link0SpanId, link0SpanIdBytes) = GenerateRandomId(8);
            var (link1SpanId, link1SpanIdBytes) = GenerateRandomId(8);
            var (link2SpanId, link2SpanIdBytes) = GenerateRandomId(8);

            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.Links = new Links
            {
                Link =
                {
                    new Link
                    {
                        SpanId = ByteString.CopyFrom(link0SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link0TraceIdBytes),
                        Type = Link.Types.Type.ChildLinkedSpan
                    },
                    new Link
                    {
                        SpanId = ByteString.CopyFrom(link1SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link1TraceIdBytes),
                        Type = Link.Types.Type.ParentLinkedSpan
                    },
                    new Link
                    {
                        SpanId = ByteString.CopyFrom(link2SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link2TraceIdBytes),
                        Type = Link.Types.Type.Unspecified
                    }
                }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual(9, dependency.Properties.Count);

            Assert.IsTrue(dependency.Properties.ContainsKey("link0_traceId"));
            Assert.IsTrue(dependency.Properties.ContainsKey("link1_traceId"));
            Assert.IsTrue(dependency.Properties.ContainsKey("link2_traceId"));

            Assert.AreEqual(link0TraceId, dependency.Properties["link0_traceId"]);
            Assert.AreEqual(link1TraceId, dependency.Properties["link1_traceId"]);
            Assert.AreEqual(link2TraceId, dependency.Properties["link2_traceId"]);

            Assert.IsTrue(dependency.Properties.ContainsKey("link0_spanId"));
            Assert.IsTrue(dependency.Properties.ContainsKey("link1_spanId"));
            Assert.IsTrue(dependency.Properties.ContainsKey("link2_spanId"));

            Assert.AreEqual(link0SpanId, dependency.Properties["link0_spanId"]);
            Assert.AreEqual(link1SpanId, dependency.Properties["link1_spanId"]);
            Assert.AreEqual(link2SpanId, dependency.Properties["link2_spanId"]);

            Assert.IsTrue(dependency.Properties.ContainsKey("link0_type"));
            Assert.IsTrue(dependency.Properties.ContainsKey("link1_type"));
            Assert.IsTrue(dependency.Properties.ContainsKey("link2_type"));

            Assert.AreEqual("ChildLinkedSpan", dependency.Properties["link0_type"]);
            Assert.AreEqual("ParentLinkedSpan", dependency.Properties["link1_type"]);
            Assert.AreEqual("Unspecified", dependency.Properties["link2_type"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithLinksAndAttributes()
        {
            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.Links = new Links
            {
                Link =
                {
                    new Link
                    {
                        SpanId = ByteString.CopyFrom(GenerateRandomId(16).Item2),
                        TraceId = ByteString.CopyFrom(GenerateRandomId(8).Item2),
                        Type = Link.Types.Type.ChildLinkedSpan,
                        Attributes = new Attributes { AttributeMap = { ["some.attribute"] = CreateAttributeValue("foo")} }
                    }
                }
            };

            client.TrackSpan(span);

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual(4, dependency.Properties.Count);

            Assert.IsTrue(dependency.Properties.ContainsKey("link0_some.attribute"));
            Assert.AreEqual("foo", dependency.Properties["link0_some.attribute"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithLinks()
        {
            var (link0TraceId, link0TraceIdBytes) = GenerateRandomId(16);
            var (link1TraceId, link1TraceIdBytes) = GenerateRandomId(16);
            var (link2TraceId, link2TraceIdBytes) = GenerateRandomId(16);

            var (link0SpanId, link0SpanIdBytes) = GenerateRandomId(8);
            var (link1SpanId, link1SpanIdBytes) = GenerateRandomId(8);
            var (link2SpanId, link2SpanIdBytes) = GenerateRandomId(8);

            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.Links = new Links
            {
                Link =
                {
                    new Link
                    {
                        SpanId = ByteString.CopyFrom(link0SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link0TraceIdBytes),
                        Type = Link.Types.Type.ChildLinkedSpan
                    },
                    new Link
                    {
                        SpanId = ByteString.CopyFrom(link1SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link1TraceIdBytes),
                        Type = Link.Types.Type.ParentLinkedSpan
                    },
                    new Link
                    {
                        SpanId = ByteString.CopyFrom(link2SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link2TraceIdBytes),
                        Type = Link.Types.Type.Unspecified
                    }
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(9, request.Properties.Count);

            Assert.IsTrue(request.Properties.ContainsKey("link0_traceId"));
            Assert.IsTrue(request.Properties.ContainsKey("link1_traceId"));
            Assert.IsTrue(request.Properties.ContainsKey("link2_traceId"));

            Assert.AreEqual(link0TraceId, request.Properties["link0_traceId"]);
            Assert.AreEqual(link1TraceId, request.Properties["link1_traceId"]);
            Assert.AreEqual(link2TraceId, request.Properties["link2_traceId"]);

            Assert.IsTrue(request.Properties.ContainsKey("link0_spanId"));
            Assert.IsTrue(request.Properties.ContainsKey("link1_spanId"));
            Assert.IsTrue(request.Properties.ContainsKey("link2_spanId"));

            Assert.AreEqual(link0SpanId, request.Properties["link0_spanId"]);
            Assert.AreEqual(link1SpanId, request.Properties["link1_spanId"]);
            Assert.AreEqual(link2SpanId, request.Properties["link2_spanId"]);

            Assert.IsTrue(request.Properties.ContainsKey("link0_type"));
            Assert.IsTrue(request.Properties.ContainsKey("link1_type"));
            Assert.IsTrue(request.Properties.ContainsKey("link2_type"));

            Assert.AreEqual("ChildLinkedSpan", request.Properties["link0_type"]);
            Assert.AreEqual("ParentLinkedSpan", request.Properties["link1_type"]);
            Assert.AreEqual("Unspecified", request.Properties["link2_type"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithLinksAndAttributes()
        {
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.Links = new Links
            {
                Link =
                {
                    new Link
                    {
                        SpanId = ByteString.CopyFrom(GenerateRandomId(16).Item2),
                        TraceId = ByteString.CopyFrom(GenerateRandomId(8).Item2),
                        Type = Link.Types.Type.ChildLinkedSpan,
                        Attributes = new Attributes { AttributeMap = { ["some.attribute"] = CreateAttributeValue("foo")} }
                    }
                }
            };

            client.TrackSpan(span);

            var request = sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(4, request.Properties.Count);

            Assert.IsTrue(request.Properties.ContainsKey("link0_some.attribute"));
            Assert.AreEqual("foo", request.Properties["link0_some.attribute"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithAnnotations()
        {
            var now = DateTime.UtcNow;
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.TimeEvents = new TimeEvents
            {
                TimeEvent =
                {
                    new TimeEvent
                    {
                        Time = now.ToTimestamp(),
                        Annotation = new TimeEvent.Types.Annotation
                        {
                            Description = new TruncatableString {Value = "test message1"}
                        }
                    },
                    new TimeEvent
                    {
                        Annotation = new TimeEvent.Types.Annotation
                        {
                            Description = new TruncatableString {Value = "test message2"},
                            Attributes = new Attributes {
                                AttributeMap =
                                {
                                    ["custom.stringAttribute"] = CreateAttributeValue("string"),
                                    ["custom.longAttribute"] = CreateAttributeValue(long.MaxValue),
                                    ["custom.boolAttribute"] = CreateAttributeValue(true)
                                }}
                        }
                    }
                }
            };

            client.TrackSpan(span);

            Assert.AreEqual(3, sentItems.Count);
            Assert.AreEqual(1, sentItems.OfType<RequestTelemetry>().Count());
            Assert.AreEqual(2, sentItems.OfType<TraceTelemetry>().Count());

            var request = sentItems.OfType<RequestTelemetry>().Single();
            var trace1 = sentItems.OfType<TraceTelemetry>().First();
            var trace2 = sentItems.OfType<TraceTelemetry>().Last();

            Assert.AreEqual(request.Context.Operation.Id, trace1.Context.Operation.Id);
            Assert.AreEqual(request.Context.Operation.Id, trace2.Context.Operation.Id);
            Assert.AreEqual(request.Id, trace1.Context.Operation.ParentId);
            Assert.AreEqual(request.Id, trace2.Context.Operation.ParentId);

            Assert.AreEqual("test message1", trace1.Message);
            Assert.AreEqual("test message2", trace2.Message);

            Assert.AreEqual(now, trace1.Timestamp);
            Assert.AreNotEqual(now, trace2.Timestamp);
            Assert.IsTrue(Math.Abs((DateTime.UtcNow - trace2.Timestamp).TotalSeconds) < 1);

            Assert.IsFalse(trace1.Properties.Any());
            Assert.AreEqual(3, trace2.Properties.Count);
            Assert.IsTrue(trace2.Properties.ContainsKey("custom.stringAttribute"));
            Assert.AreEqual("string", trace2.Properties["custom.stringAttribute"]);

            Assert.IsTrue(trace2.Properties.ContainsKey("custom.longAttribute"));
            Assert.AreEqual(long.MaxValue.ToString(), trace2.Properties["custom.longAttribute"]);

            Assert.IsTrue(trace2.Properties.ContainsKey("custom.boolAttribute"));
            Assert.AreEqual(bool.TrueString, trace2.Properties["custom.boolAttribute"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependenciesWithAnnotations()
        {
            var now = DateTime.UtcNow;
            var span = CreateBasicSpan(SpanKind.Client, "spanName");
            span.TimeEvents = new TimeEvents
            {
                TimeEvent =
                {
                    new TimeEvent
                    {
                        Time = now.ToTimestamp(),
                        Annotation = new TimeEvent.Types.Annotation
                        {
                            Description = new TruncatableString {Value = "test message1"}
                        }
                    },
                    new TimeEvent
                    {
                        Annotation = new TimeEvent.Types.Annotation
                        {
                            Description = new TruncatableString {Value = "test message2"},
                            Attributes = new Attributes {
                                AttributeMap =
                                {
                                    ["custom.stringAttribute"] = CreateAttributeValue("string"),
                                    ["custom.longAttribute"] = CreateAttributeValue(long.MaxValue),
                                    ["custom.boolAttribute"] = CreateAttributeValue(true)
                                }}
                        }
                    }
                }
            };

            client.TrackSpan(span);

            Assert.AreEqual(3, sentItems.Count);
            Assert.AreEqual(1, sentItems.OfType<DependencyTelemetry>().Count());
            Assert.AreEqual(2, sentItems.OfType<TraceTelemetry>().Count());

            var dependency = sentItems.OfType<DependencyTelemetry>().Single();
            var trace1 = sentItems.OfType<TraceTelemetry>().First();
            var trace2 = sentItems.OfType<TraceTelemetry>().Last();

            Assert.AreEqual(dependency.Context.Operation.Id, trace1.Context.Operation.Id);
            Assert.AreEqual(dependency.Context.Operation.Id, trace2.Context.Operation.Id);
            Assert.AreEqual(dependency.Id, trace1.Context.Operation.ParentId);
            Assert.AreEqual(dependency.Id, trace2.Context.Operation.ParentId);

            Assert.AreEqual("test message1", trace1.Message);
            Assert.AreEqual("test message2", trace2.Message);

            Assert.AreEqual(now, trace1.Timestamp);
            Assert.AreNotEqual(now, trace2.Timestamp);
            Assert.IsTrue(Math.Abs((DateTime.UtcNow - trace2.Timestamp).TotalSeconds) < 1);

            Assert.IsFalse(trace1.Properties.Any());
            Assert.AreEqual(3, trace2.Properties.Count);
            Assert.IsTrue(trace2.Properties.ContainsKey("custom.stringAttribute"));
            Assert.AreEqual("string", trace2.Properties["custom.stringAttribute"]);

            Assert.IsTrue(trace2.Properties.ContainsKey("custom.longAttribute"));
            Assert.AreEqual(long.MaxValue.ToString(), trace2.Properties["custom.longAttribute"]);

            Assert.IsTrue(trace2.Properties.ContainsKey("custom.boolAttribute"));
            Assert.AreEqual(bool.TrueString, trace2.Properties["custom.boolAttribute"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithMessage()
        {
            var now = DateTime.UtcNow;
            var span = CreateBasicSpan(SpanKind.Server, "spanName");
            span.TimeEvents = new TimeEvents
            {
                TimeEvent =
                {
                    new TimeEvent
                    {
                        Time = now.ToTimestamp(),
                        MessageEvent = new TimeEvent.Types.MessageEvent
                        {
                            Id = 1,
                            CompressedSize = 2,
                            UncompressedSize = 3,
                            Type = TimeEvent.Types.MessageEvent.Types.Type.Received
                        }
                    },
                    new TimeEvent
                    {
                        Time = now.ToTimestamp(),
                        MessageEvent = new TimeEvent.Types.MessageEvent
                        {
                            Id = 4,
                            CompressedSize = 5,
                            UncompressedSize = 6,
                            Type = TimeEvent.Types.MessageEvent.Types.Type.Sent
                        }
                    },
                    new TimeEvent
                    {
                        MessageEvent = new TimeEvent.Types.MessageEvent
                        {
                            Id = 7,
                            CompressedSize = 8,
                            UncompressedSize = 9,
                            Type = TimeEvent.Types.MessageEvent.Types.Type.Unspecified
                        }
                    }
                }
            };

            client.TrackSpan(span);

            Assert.AreEqual(4, sentItems.Count);
            Assert.AreEqual(1, sentItems.OfType<RequestTelemetry>().Count());
            Assert.AreEqual(3, sentItems.OfType<TraceTelemetry>().Count());

            var request = sentItems.OfType<RequestTelemetry>().Single();
            var traces = sentItems.OfType<TraceTelemetry>().ToArray();

            foreach (var t in traces)
            {
                Assert.AreEqual(request.Context.Operation.Id, t.Context.Operation.Id);
                Assert.AreEqual(request.Id, t.Context.Operation.ParentId);
                Assert.IsFalse(t.Properties.Any());
            }

            Assert.AreEqual("MessageEvent. messageId: '1', type: 'Received', compressed size: '2', uncompressed size: '3'", traces[0].Message);
            Assert.AreEqual("MessageEvent. messageId: '4', type: 'Sent', compressed size: '5', uncompressed size: '6'", traces[1].Message);
            Assert.AreEqual("MessageEvent. messageId: '7', type: 'Unspecified', compressed size: '8', uncompressed size: '9'", traces[2].Message);
        }

        private Span CreateBasicSpan(SpanKind kind, string spanName)
        {
            var span = new Span
            {
                Kind = kind,
                TraceId = ByteString.CopyFrom(testTraceIdBytes, 0, 16),
                SpanId = ByteString.CopyFrom(testSpanIdBytes, 0, 8),
                Name = new TruncatableString { Value = spanName },
            };

            return span;
        }

        private AttributeValue CreateAttributeValue<T>(T value)
        {
            if (value is int)
            {
                return new AttributeValue { IntValue = Convert.ToInt32(value) };
            }

            if (value is long)
            {
                return new AttributeValue { IntValue = Convert.ToInt64(value) };
            }

            if (value is bool)
            {
                return new AttributeValue { BoolValue = Convert.ToBoolean(value) };
            }

            var s = value as string;
            if (s != null)
            {
                return new AttributeValue{ StringValue = new TruncatableString { Value = s } };
            }

            return new AttributeValue { StringValue = new TruncatableString { Value = value.ToString() } };
        }

        private static (string, byte[]) GenerateRandomId(int byteCount)
        {
            var idBytes = new byte[byteCount];
            Rand.NextBytes(idBytes);

            var idString = BitConverter.ToString(idBytes).Replace("-", "").ToLower();

            return (idString, idBytes);
        }

        private static readonly Random Rand = new Random();
    }
}