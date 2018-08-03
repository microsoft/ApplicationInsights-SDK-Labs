namespace Microsoft.LocalForwarder.Test.Library
{
    using ApplicationInsights;
    using ApplicationInsights.Channel;
    using ApplicationInsights.DataContracts;
    using ApplicationInsights.Extensibility;
    using ApplicationInsights.Extensibility.Implementation;
    using Google.Protobuf;
    using Google.Protobuf.WellKnownTypes;
    using LocalForwarder.Library;
    using Opencensus.Proto.Trace;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using VisualStudio.TestTools.UnitTesting;

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
            this.configuration = new TelemetryConfiguration();
            this.channel = new StubTelemetryChannel
            {
                OnSend = t => this.sentItems.Enqueue(t)
            };

            this.configuration.TelemetryChannel = this.channel;
            this.client = new TelemetryClient(this.configuration);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequest()
        {
            // ARRANGE
            var now = DateTime.UtcNow;

            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.StartTime = now.AddSeconds(-1).ToTimestamp();
            span.EndTime = now.ToTimestamp();

            // ACT
            this.client.TrackSpan(span, string.Empty);

            // ASSERT
            Assert.AreEqual(1, this.sentItems.Count);
            Assert.IsInstanceOfType(this.sentItems.Single(), typeof(RequestTelemetry));

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
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
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.ParentSpanId = ByteString.CopyFrom(this.testParentSpanIdBytes, 0, 8);

            // ACT
            this.client.TrackSpan(span, string.Empty);

            // ASSERT
            Assert.AreEqual(TestParentSpanId, ((RequestTelemetry)this.sentItems.Single()).Context.Operation.ParentId);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithStatus()
        {
            // ARRANGE
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.Status = new Status {Code = 0};

            // ACT
            this.client.TrackSpan(span, string.Empty);

            // ASSERT
            var request = (RequestTelemetry)this.sentItems.Single();

            Assert.IsTrue(request.Success.HasValue);
            Assert.IsTrue(request.Success.Value);
            Assert.IsTrue(string.IsNullOrEmpty(request.ResponseCode));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithStatusAndDescription()
        {
            // ARRANGE
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.Status = new Status {Code = 0, Message = "all good"};

            // ACT
            this.client.TrackSpan(span, string.Empty);

            // ASSERT
            var request = (RequestTelemetry)this.sentItems.Single();

            Assert.IsTrue(request.Success.HasValue);
            Assert.IsTrue(request.Success.Value);
            Assert.AreEqual("all good", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithNonSuccessStatusAndDescription()
        {
            // ARRANGE
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.Status = new Status { Code = 1, Message = "all bad" };

            // ACT
            this.client.TrackSpan(span, string.Empty);

            // ASSERT
            var request = (RequestTelemetry)this.sentItems.Single();

            Assert.IsTrue(request.Success.HasValue);
            Assert.IsFalse(request.Success.Value);
            Assert.AreEqual("all bad", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestErrorAttribute()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap = { ["error"] = this.CreateAttributeValue(true) }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.IsTrue(request.Success.HasValue);
            Assert.IsFalse(request.Success.Value);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependency()
        {
            // ARRANGE
            var now = DateTime.UtcNow;

            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.StartTime = now.AddSeconds(-1).ToTimestamp();
            span.EndTime = now.ToTimestamp();

            // ACT
            this.client.TrackSpan(span, string.Empty);

            // ASSERT
            Assert.AreEqual(1, this.sentItems.Count);
            Assert.IsInstanceOfType(this.sentItems.Single(), typeof(DependencyTelemetry));

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
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
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.ParentSpanId = ByteString.CopyFrom(this.testParentSpanIdBytes, 0, 8);

            // ACT
            this.client.TrackSpan(span, string.Empty);

            // ASSERT
            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual(TestParentSpanId, dependency.Context.Operation.ParentId);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithStatus()
        {
            // ARRANGE
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.Status = new Status {Code = 0};

            // ACT
            this.client.TrackSpan(span, string.Empty);

            // ASSERT
            var dependency = (DependencyTelemetry)this.sentItems.Single();

            Assert.IsTrue(dependency.Success.HasValue);
            Assert.IsTrue(dependency.Success.Value);
            Assert.AreEqual("0", dependency.ResultCode);
            Assert.IsFalse(dependency.Properties.ContainsKey("StatusDescription"));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithStatusAndDescription()
        {
            // ARRANGE
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.Status = new Status {Code = 0, Message = "all good"};

            // ACT
            this.client.TrackSpan(span, string.Empty);

            // ASSERT
            var dependency = (DependencyTelemetry)this.sentItems.Single();

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
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.Status = new Status { Code = 1, Message = "all bad" };

            // ACT
            this.client.TrackSpan(span, string.Empty);

            // ASSERT
            var dependency = (DependencyTelemetry)this.sentItems.Single();

            Assert.IsTrue(dependency.Success.HasValue);
            Assert.IsFalse(dependency.Success.Value);
            Assert.AreEqual("1", dependency.ResultCode);
            Assert.IsTrue(dependency.Properties.ContainsKey("statusDescription"));
            Assert.AreEqual("all bad", dependency.Properties["statusDescription"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyErrorAttribute()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap = { ["error"] = this.CreateAttributeValue(true) }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
            Assert.IsTrue(dependency.Success.HasValue);
            Assert.IsFalse(dependency.Success.Value);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestBasedOnSpanKindAttribute()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.Attributes = new Span.Types.Attributes()
            {
                AttributeMap = { ["span.kind"] = this.CreateAttributeValue("server") }
            };

            this.client.TrackSpan(span, string.Empty);

            Assert.IsInstanceOfType(this.sentItems.Single(), typeof(RequestTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestBasedOnSpanKindProperty()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.SameProcessAsParentSpan = null;
            span.ParentSpanId = ByteString.CopyFrom(this.testParentSpanIdBytes, 0, 8);
            this.client.TrackSpan(span, string.Empty);

            Assert.IsInstanceOfType(this.sentItems.Single(), typeof(RequestTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyBasedOnSpanKindProperty()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.SameProcessAsParentSpan = null;
            span.ParentSpanId = ByteString.CopyFrom(this.testParentSpanIdBytes, 0, 8);

            this.client.TrackSpan(span, string.Empty);

            Assert.IsInstanceOfType(this.sentItems.Single(), typeof(DependencyTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependenciesBasedOnSpanKindAttribute()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap = { ["span.kind"] = this.CreateAttributeValue("client") }
            };

            this.client.TrackSpan(span, string.Empty);

            Assert.IsInstanceOfType(this.sentItems.Single(), typeof(DependencyTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestBasedOnSameProcessAsParentFlag()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Unspecified, "spanName");
            span.SameProcessAsParentSpan = false;
            this.client.TrackSpan(span, string.Empty);

            Assert.IsInstanceOfType(this.sentItems.Single(), typeof(RequestTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDepednencyBasedOnSameProcessAsParentFlag()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Unspecified, "spanName");
            span.SameProcessAsParentSpan = true;
            this.client.TrackSpan(span, string.Empty);

            Assert.IsInstanceOfType(this.sentItems.Single(), typeof(DependencyTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDepednencyBasedOnSameProcessAsParentFlagNotSet()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Unspecified, "spanName");
            span.SameProcessAsParentSpan = null;
            this.client.TrackSpan(span, string.Empty);

            Assert.IsInstanceOfType(this.sentItems.Single(), typeof(DependencyTelemetry));
        }


        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithoutName()
        {
            var span = new Span
            {
                Kind = Span.Types.SpanKind.Server,
                TraceId = ByteString.CopyFrom(this.testTraceIdBytes, 0, 16),
                SpanId = ByteString.CopyFrom(this.testSpanIdBytes, 0, 8),
            };

            this.client.TrackSpan(span, string.Empty);

            Assert.IsNull(this.sentItems.OfType<RequestTelemetry>().Single().Name);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithoutKind()
        {
            var span = new Span
            {
                TraceId = ByteString.CopyFrom(this.testTraceIdBytes, 0, 16),
                SpanId = ByteString.CopyFrom(this.testSpanIdBytes, 0, 8),
                Name = new TruncatableString { Value = "spanName" }
            };

            this.client.TrackSpan(span, string.Empty);

            Assert.IsInstanceOfType(this.sentItems.Single(), typeof(DependencyTelemetry));
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithoutStartAndEndTime()
        {
            var span = new Span
            {
                Kind = Span.Types.SpanKind.Server,
                TraceId = ByteString.CopyFrom(this.testTraceIdBytes, 0, 16),
                SpanId = ByteString.CopyFrom(this.testSpanIdBytes, 0, 8),
                Name = new TruncatableString { Value = "spanName" }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.IsTrue(Math.Abs((request.Timestamp - DateTime.UtcNow).TotalSeconds) < 1);
            Assert.AreEqual(0, request.Duration.TotalSeconds);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestWithUrl()
        {
            var url = new Uri("https://host:123/path?query");
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "HttpIn");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = this.CreateAttributeValue(url),
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.status_code"] = this.CreateAttributeValue(409),
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(url.ToString(), request.Url.ToString());
            Assert.AreEqual("POST /path", request.Name);
            Assert.AreEqual("409", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestWithUrlAndRoute()
        {
            var url = new Uri("https://host:123/path?query");
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "HttpIn");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = this.CreateAttributeValue(url),
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.route"] = this.CreateAttributeValue("route"),
                    ["http.status_code"] = this.CreateAttributeValue(503),
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(url.ToString(), request.Url.ToString());
            Assert.AreEqual("POST route", request.Name);
            Assert.AreEqual("503", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestWithUrlAndNoMethod()
        {
            var url = new Uri("https://host:123/path?query");
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "HttpIn");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = this.CreateAttributeValue(url),
                    ["http.status_code"] = this.CreateAttributeValue(200),
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(url.ToString(), request.Url.ToString());
            Assert.AreEqual("/path", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestWithUrlOtherAttributesAreIgnored()
        {
            var url = new Uri("https://host:123/path?query");
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "HttpIn");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = this.CreateAttributeValue(url),
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.path"] = this.CreateAttributeValue("another path"),
                    ["http.host"] = this.CreateAttributeValue("another host"),
                    ["http.port"] = this.CreateAttributeValue(8080),
                    ["http.status_code"] = this.CreateAttributeValue(200),
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(url.ToString(), request.Url.ToString());
            Assert.AreEqual("POST /path", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestHostPortPathAttributes()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "HttpIn");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.path"] = this.CreateAttributeValue("path"),
                    ["http.host"] = this.CreateAttributeValue("host"),
                    ["http.port"] = this.CreateAttributeValue(123),
                    ["http.status_code"] = this.CreateAttributeValue(200),
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual("https://host:123/path", request.Url.ToString());
            Assert.AreEqual("POST path", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestHostPathAttributes()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "HttpIn");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.path"] = this.CreateAttributeValue("path"),
                    ["http.host"] = this.CreateAttributeValue("host"),
                    ["http.status_code"] = this.CreateAttributeValue(200),
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual("https://host/path", request.Url.ToString());
            Assert.AreEqual("POST path", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestHostAttributes()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "HttpIn");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.host"] = this.CreateAttributeValue("host"),
                    ["http.status_code"] = this.CreateAttributeValue(200),
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual("https://host/", request.Url.ToString());
            Assert.AreEqual("POST", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestOnlyMethodAttributes()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "HttpIn");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.status_code"] = this.CreateAttributeValue(200),
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.IsNull(request.Url);
            Assert.AreEqual("POST", request.Name);
            Assert.AreEqual("200", request.ResponseCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpRequestUserAgent()
        {
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.99 Safari/537.36";

            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "HttpIn");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = this.CreateAttributeValue("https://host/path"),
                    ["http.user_agent"] = this.CreateAttributeValue(userAgent)
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(userAgent, request.Context.User.UserAgent);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithUrl()
        {
            var url = new Uri("https://host:123/path?query");
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "HttpOut");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = this.CreateAttributeValue(url),
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.status_code"] = this.CreateAttributeValue(200)
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
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
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "HttpOut");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.url"] = this.CreateAttributeValue(url),
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.path"] = this.CreateAttributeValue("another path"),
                    ["http.host"] = this.CreateAttributeValue("another host"),
                    ["http.port"] = this.CreateAttributeValue(8080),
                    ["http.status_code"] = this.CreateAttributeValue(200)
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual(url.ToString(), dependency.Data);
            Assert.AreEqual("POST /path", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.AreEqual("host", dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithHostPortPath()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "HttpOut");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.path"] = this.CreateAttributeValue("path"),
                    ["http.host"] = this.CreateAttributeValue("host"),
                    ["http.port"] = this.CreateAttributeValue(123),
                    ["http.status_code"] = this.CreateAttributeValue(200)
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual("https://host:123/path", dependency.Data);
            Assert.AreEqual("POST path", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.AreEqual("host", dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithHostPort()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "HttpOut");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {

                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.host"] = this.CreateAttributeValue("host"),
                    ["http.port"] = this.CreateAttributeValue(123),
                    ["http.status_code"] = this.CreateAttributeValue(200)
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual("https://host:123/", dependency.Data);
            Assert.AreEqual("POST", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.AreEqual("host", dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithHost()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "HttpOut");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {

                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.host"] = this.CreateAttributeValue("host"),
                    ["http.status_code"] = this.CreateAttributeValue(200)
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual("https://host/", dependency.Data);
            Assert.AreEqual("POST", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.AreEqual("host", dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithMethod()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "HttpOut");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.method"] = this.CreateAttributeValue("POST"),
                    ["http.status_code"] = this.CreateAttributeValue(200)
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
            Assert.IsNull(dependency.Data);
            Assert.AreEqual("POST", dependency.Name);
            Assert.AreEqual("200", dependency.ResultCode);
            Assert.IsNull(dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksHttpDependencyWithStatusCodeOnly()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "HttpOut");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["http.status_code"] = this.CreateAttributeValue(200)
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
            Assert.IsNull(dependency.Data);
            Assert.IsNull(dependency.Name);
            Assert.IsNull(dependency.Target);
            Assert.AreEqual("Http", dependency.Type);
            Assert.AreEqual("200", dependency.ResultCode);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithCustomAttributes()
        {
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["custom.stringAttribute"] = this.CreateAttributeValue("string"),
                    ["custom.longAttribute"] = this.CreateAttributeValue(long.MaxValue),
                    ["custom.boolAttribute"] = this.CreateAttributeValue(true)
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
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
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.Attributes = new Span.Types.Attributes
            {
                AttributeMap =
                {
                    ["custom.stringAttribute"] = this.CreateAttributeValue("string"),
                    ["custom.longAttribute"] = this.CreateAttributeValue(long.MaxValue),
                    ["custom.boolAttribute"] = this.CreateAttributeValue(true)
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
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

            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.Links = new Span.Types.Links
            {
                Link =
                {
                    new Span.Types.Link
                    {
                        SpanId = ByteString.CopyFrom(link0SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link0TraceIdBytes),
                        Type = Span.Types.Link.Types.Type.ChildLinkedSpan
                    },
                    new Span.Types.Link
                    {
                        SpanId = ByteString.CopyFrom(link1SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link1TraceIdBytes),
                        Type = Span.Types.Link.Types.Type.ParentLinkedSpan
                    },
                    new Span.Types.Link
                    {
                        SpanId = ByteString.CopyFrom(link2SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link2TraceIdBytes),
                        Type = Span.Types.Link.Types.Type.Unspecified
                    }
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
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
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.Links = new Span.Types.Links
            {
                Link =
                {
                    new Span.Types.Link
                    {
                        SpanId = ByteString.CopyFrom(GenerateRandomId(16).Item2),
                        TraceId = ByteString.CopyFrom(GenerateRandomId(8).Item2),
                        Type = Span.Types.Link.Types.Type.ChildLinkedSpan,
                        Attributes = new Span.Types.Attributes { AttributeMap = { ["some.attribute"] = this.CreateAttributeValue("foo")} }
                    }
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
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

            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.Links = new Span.Types.Links
            {
                Link =
                {
                    new Span.Types.Link
                    {
                        SpanId = ByteString.CopyFrom(link0SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link0TraceIdBytes),
                        Type = Span.Types.Link.Types.Type.ChildLinkedSpan
                    },
                    new Span.Types.Link
                    {
                        SpanId = ByteString.CopyFrom(link1SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link1TraceIdBytes),
                        Type = Span.Types.Link.Types.Type.ParentLinkedSpan
                    },
                    new Span.Types.Link
                    {
                        SpanId = ByteString.CopyFrom(link2SpanIdBytes),
                        TraceId = ByteString.CopyFrom(link2TraceIdBytes),
                        Type = Span.Types.Link.Types.Type.Unspecified
                    }
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
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
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.Links = new Span.Types.Links
            {
                Link =
                {
                    new Span.Types.Link
                    {
                        SpanId = ByteString.CopyFrom(GenerateRandomId(16).Item2),
                        TraceId = ByteString.CopyFrom(GenerateRandomId(8).Item2),
                        Type = Span.Types.Link.Types.Type.ChildLinkedSpan,
                        Attributes = new Span.Types.Attributes { AttributeMap = { ["some.attribute"] = this.CreateAttributeValue("foo")} }
                    }
                }
            };

            this.client.TrackSpan(span, string.Empty);

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual(4, request.Properties.Count);

            Assert.IsTrue(request.Properties.ContainsKey("link0_some.attribute"));
            Assert.AreEqual("foo", request.Properties["link0_some.attribute"]);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithAnnotations()
        {
            var now = DateTime.UtcNow;
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.TimeEvents = new Span.Types.TimeEvents
            {
                TimeEvent =
                {
                    new Span.Types.TimeEvent
                    {
                        Time = now.ToTimestamp(),
                        Annotation = new Span.Types.TimeEvent.Types.Annotation
                        {
                            Description = new TruncatableString {Value = "test message1"}
                        }
                    },
                    new Span.Types.TimeEvent
                    {
                        Annotation = new Span.Types.TimeEvent.Types.Annotation
                        {
                            Description = new TruncatableString {Value = "test message2"},
                            Attributes = new Span.Types.Attributes {
                                AttributeMap =
                                {
                                    ["custom.stringAttribute"] = this.CreateAttributeValue("string"),
                                    ["custom.longAttribute"] = this.CreateAttributeValue(long.MaxValue),
                                    ["custom.boolAttribute"] = this.CreateAttributeValue(true)
                                }}
                        }
                    }
                }
            };

            this.client.TrackSpan(span, string.Empty);

            Assert.AreEqual(3, this.sentItems.Count);
            Assert.AreEqual(1, this.sentItems.OfType<RequestTelemetry>().Count());
            Assert.AreEqual(2, this.sentItems.OfType<TraceTelemetry>().Count());

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            var trace1 = this.sentItems.OfType<TraceTelemetry>().First();
            var trace2 = this.sentItems.OfType<TraceTelemetry>().Last();

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
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "spanName");
            span.TimeEvents = new Span.Types.TimeEvents
            {
                TimeEvent =
                {
                    new Span.Types.TimeEvent
                    {
                        Time = now.ToTimestamp(),
                        Annotation = new Span.Types.TimeEvent.Types.Annotation
                        {
                            Description = new TruncatableString {Value = "test message1"}
                        }
                    },
                    new Span.Types.TimeEvent
                    {
                        Annotation = new Span.Types.TimeEvent.Types.Annotation
                        {
                            Description = new TruncatableString {Value = "test message2"},
                            Attributes = new Span.Types.Attributes {
                                AttributeMap =
                                {
                                    ["custom.stringAttribute"] = this.CreateAttributeValue("string"),
                                    ["custom.longAttribute"] = this.CreateAttributeValue(long.MaxValue),
                                    ["custom.boolAttribute"] = this.CreateAttributeValue(true)
                                }}
                        }
                    }
                }
            };

            this.client.TrackSpan(span, string.Empty);

            Assert.AreEqual(3, this.sentItems.Count);
            Assert.AreEqual(1, this.sentItems.OfType<DependencyTelemetry>().Count());
            Assert.AreEqual(2, this.sentItems.OfType<TraceTelemetry>().Count());

            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
            var trace1 = this.sentItems.OfType<TraceTelemetry>().First();
            var trace2 = this.sentItems.OfType<TraceTelemetry>().Last();

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
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.TimeEvents = new Span.Types.TimeEvents
            {
                TimeEvent =
                {
                    new Span.Types.TimeEvent
                    {
                        Time = now.ToTimestamp(),
                        MessageEvent = new Span.Types.TimeEvent.Types.MessageEvent
                        {
                            Id = 1,
                            CompressedSize = 2,
                            UncompressedSize = 3,
                            Type = Span.Types.TimeEvent.Types.MessageEvent.Types.Type.Received
                        }
                    },
                    new Span.Types.TimeEvent
                    {
                        Time = now.ToTimestamp(),
                        MessageEvent = new Span.Types.TimeEvent.Types.MessageEvent
                        {
                            Id = 4,
                            CompressedSize = 5,
                            UncompressedSize = 6,
                            Type = Span.Types.TimeEvent.Types.MessageEvent.Types.Type.Sent
                        }
                    },
                    new Span.Types.TimeEvent
                    {
                        MessageEvent = new Span.Types.TimeEvent.Types.MessageEvent
                        {
                            Id = 7,
                            CompressedSize = 8,
                            UncompressedSize = 9,
                            Type = Span.Types.TimeEvent.Types.MessageEvent.Types.Type.Unspecified
                        }
                    }
                }
            };

            this.client.TrackSpan(span, string.Empty);

            Assert.AreEqual(4, this.sentItems.Count);
            Assert.AreEqual(1, this.sentItems.OfType<RequestTelemetry>().Count());
            Assert.AreEqual(3, this.sentItems.OfType<TraceTelemetry>().Count());

            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            var traces = this.sentItems.OfType<TraceTelemetry>().ToArray();

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

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksRequestWithCorrectIkey()
        {
            // ARRANGE
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "HttpIn");

            // ACT
            this.client.TrackSpan(span, "ikey1");

            // ASSERT
            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            Assert.AreEqual("ikey1", request.Context.InstrumentationKey);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksDependencyWithCorrectIkey()
        {
            // ARRANGE
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Client, "HttpOut");

            // ACT
            this.client.TrackSpan(span, "ikey1");

            // ASSERT
            var dependency = this.sentItems.OfType<DependencyTelemetry>().Single();
            Assert.AreEqual("ikey1", dependency.Context.InstrumentationKey);
        }

        [TestMethod]
        public void OpenCensusTelemetryConverterTests_TracksTraceWithCorrectIkey()
        {
            // ARRANGE
            var now = DateTime.UtcNow;
            var span = this.CreateBasicSpan(Span.Types.SpanKind.Server, "spanName");
            span.TimeEvents = new Span.Types.TimeEvents
            {
                TimeEvent =
                {
                    new Span.Types.TimeEvent
                    {
                        Time = now.ToTimestamp(),
                        Annotation = new Span.Types.TimeEvent.Types.Annotation
                        {
                            Description = new TruncatableString {Value = "test message1"}
                        }
                    },
                    new Span.Types.TimeEvent
                    {
                        Annotation = new Span.Types.TimeEvent.Types.Annotation
                        {
                            Description = new TruncatableString {Value = "test message2"},
                            Attributes = new Span.Types.Attributes
                            {
                                AttributeMap =
                                {
                                    ["custom.stringAttribute"] = this.CreateAttributeValue("string"),
                                    ["custom.longAttribute"] = this.CreateAttributeValue(long.MaxValue),
                                    ["custom.boolAttribute"] = this.CreateAttributeValue(true)
                                }
                            }
                        }
                    }
                }
            };

            // ACT
            this.client.TrackSpan(span, "ikey1");

            // ASSERT
            var request = this.sentItems.OfType<RequestTelemetry>().Single();
            var trace1 = this.sentItems.OfType<TraceTelemetry>().First();
            var trace2 = this.sentItems.OfType<TraceTelemetry>().Last();

            Assert.AreEqual("ikey1", request.Context.InstrumentationKey);
            Assert.AreEqual("ikey1", trace1.Context.InstrumentationKey);
            Assert.AreEqual("ikey1", trace2.Context.InstrumentationKey);
        }

        private Span CreateBasicSpan(Span.Types.SpanKind kind, string spanName)
        {
            var span = new Span
            {
                Kind = kind,
                TraceId = ByteString.CopyFrom(this.testTraceIdBytes, 0, 16),
                SpanId = ByteString.CopyFrom(this.testSpanIdBytes, 0, 8),
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