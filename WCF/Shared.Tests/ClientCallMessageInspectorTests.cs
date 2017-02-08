using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Wcf.Implementation;
using Microsoft.ApplicationInsights.Wcf.Tests.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class ClientCallMessageInspectorTests
    {
        const String TwoWayOp1 = "http://tempuri.org/ISimpleService/GetSimpleData";
        const String TwoWayOp2 = "http://tempuri.org/ISimpleService/CallFailsWithFault";
        const String OneWayOp1 = "http://tempuri.org/IOneWayService/SuccessfullOneWayCall";

        const String HostName = "localhost";
        const String SvcUrl = "http://localhost/MyService.svc";

        [TestMethod]
        public void CanEmitTelemetryForTwoWayOperation()
        {
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var operationMap = BuildOperationMap();
            var channel = GetMockChannel();
            var inspector = new ClientCallMessageInspector(client, operationMap);

            var request = BuildMessage(TwoWayOp1);
            var response = BuildMessage(TwoWayOp1);

            var state = inspector.BeforeSendRequest(ref request, channel);
            inspector.AfterReceiveReply(ref response, state);

            var dependency = TestTelemetryChannel.CollectedData().OfType<DependencyTelemetry>().FirstOrDefault();
            Assert.IsNotNull(dependency);
            Assert.AreEqual(SvcUrl, dependency.Name);
            Assert.AreEqual(HostName, dependency.Target);
            Assert.AreEqual("GetSimpleData", dependency.Data);
            Assert.AreEqual(TwoWayOp1, dependency.Properties["soapAction"]);
        }

        [TestMethod]
        public void CanEmitTelemetryForOneWayOperation()
        {
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var operationMap = BuildOperationMap();
            var channel = GetMockChannel();
            var inspector = new ClientCallMessageInspector(client, operationMap);

            var request = BuildMessage(OneWayOp1);

            inspector.BeforeSendRequest(ref request, channel);
            // do NOT invoke AfterReceiveReply

            var dependency = TestTelemetryChannel.CollectedData().OfType<DependencyTelemetry>().FirstOrDefault();
            Assert.IsNotNull(dependency);
            Assert.AreEqual(DependencyConstants.WcfClientCall, dependency.Type);
            Assert.AreEqual(SvcUrl, dependency.Name);
            Assert.AreEqual(HostName, dependency.Target);
            Assert.AreEqual("SuccessfullOneWayCall", dependency.Data);
            Assert.AreEqual(OneWayOp1, dependency.Properties["soapAction"]);
            Assert.AreEqual("True", dependency.Properties["isOneWay"]);
        }

        [TestMethod]
        public void NoTelemetryEventIsEmmittedWhenSoapActionNotFound()
        {
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var operationMap = BuildOperationMap();
            var channel = GetMockChannel();
            var inspector = new ClientCallMessageInspector(client, operationMap);

            var request = BuildMessage("urn:no:soap:action");
            var response = BuildMessage("urn:no:soap:action");

            var state  = inspector.BeforeSendRequest(ref request, channel);
            inspector.AfterReceiveReply(ref response, state);

            var dependency = TestTelemetryChannel.CollectedData().OfType<DependencyTelemetry>().FirstOrDefault();
            Assert.IsNull(dependency);
        }

        [TestMethod]
        public void WhenServiceReturnsErrorDependencySuccessIsFalse()
        {
            TestTelemetryChannel.Clear();
            var client = new TelemetryClient();
            var operationMap = BuildOperationMap();
            var channel = GetMockChannel();
            var inspector = new ClientCallMessageInspector(client, operationMap);

            var request = BuildMessage(TwoWayOp1);
            var response = BuildFaultMessage(TwoWayOp1);

            var state = inspector.BeforeSendRequest(ref request, channel);
            inspector.AfterReceiveReply(ref response, state);

            var dependency = TestTelemetryChannel.CollectedData().OfType<DependencyTelemetry>().FirstOrDefault();
            Assert.IsNotNull(dependency);
            Assert.AreEqual(SvcUrl, dependency.Name);
            Assert.IsFalse(dependency.Success.Value);
        }

        private ClientOperationMap BuildOperationMap()
        {
            ClientOpDescription[] ops = new ClientOpDescription[]
            {
                new ClientOpDescription { Action = TwoWayOp1, IsOneWay = false, Name = "GetSimpleData" },
                new ClientOpDescription { Action = TwoWayOp2, IsOneWay = false, Name = "CallFailsWithFault" },
                new ClientOpDescription { Action = OneWayOp1, IsOneWay = true, Name = "SuccessfullOneWayCall" },
            };
            return new ClientOperationMap(ops);
        }

        private IClientChannel GetMockChannel()
        {
            return new MockChannel(SvcUrl);
        }

        private Message BuildMessage(String action)
        {
            return Message.CreateMessage(MessageVersion.Default, action, "<text/>");
        }
        private Message BuildFaultMessage(String action)
        {
            return Message.CreateMessage(
                MessageVersion.Default,
                MessageFault.CreateFault(
                    FaultCode.CreateReceiverFaultCode("e1", "http://tempuri.org"),
                    "There was an error processing the message"
                    ),
                action);
        }


        class MockChannel : IClientChannel
        {
            public bool AllowInitializationUI { get; set; }

            public bool AllowOutputBatching { get; set; }

            public bool DidInteractiveInitialization { get; private set; }

            public IExtensionCollection<IContextChannel> Extensions { get; private set; }

            public IInputSession InputSession { get; private set; }

            public EndpointAddress LocalAddress { get; private set; }

            public TimeSpan OperationTimeout { get; set; }

            public IOutputSession OutputSession { get; private set; }

            public EndpointAddress RemoteAddress { get; private set; }

            public string SessionId { get; private set; }

            public CommunicationState State { get; private set; }

            public Uri Via { get; private set; }

            public event EventHandler Closed { add { } remove { } }
            public event EventHandler Closing { add { } remove { } }
            public event EventHandler Faulted { add { } remove { } }
            public event EventHandler Opened { add { } remove { } }
            public event EventHandler Opening { add { } remove { } }
            public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived { add { } remove { } }


            public MockChannel(String remoteUrl)
            {
                this.RemoteAddress = new EndpointAddress(remoteUrl);
            }

            public void Abort()
            {
            }

            public IAsyncResult BeginClose(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginDisplayInitializationUI(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginOpen(AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw new NotImplementedException();
            }

            public void Close()
            {
                throw new NotImplementedException();
            }

            public void Close(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }

            public void DisplayInitializationUI()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public void EndClose(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public void EndDisplayInitializationUI(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public void EndOpen(IAsyncResult result)
            {
                throw new NotImplementedException();
            }

            public T GetProperty<T>() where T : class
            {
                throw new NotImplementedException();
            }

            public void Open()
            {
                throw new NotImplementedException();
            }

            public void Open(TimeSpan timeout)
            {
                throw new NotImplementedException();
            }
        }
    }
}
