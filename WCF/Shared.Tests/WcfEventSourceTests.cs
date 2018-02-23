namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Tracing;
    using System.Globalization;
    using System.Linq;
    using Microsoft.ApplicationInsights.Wcf.Implementation;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class WcfEventSourceTests
    {
        [TestMethod]
        public void InitializationFailure_Message()
        {
            using (var listener = new WcfEventListener())
            {
                var msg = "Exception message";
                WcfEventSource.Log.InitializationFailure(msg);
                CheckMessage(listener, WcfEventSource.InitializationFailureMessage, msg);
            }
        }

        [TestMethod]
        public void TelemetryModuleExecutionStarted_Message()
        {
            using (var listener = new WcfEventListener())
            {
                var typeName = "MyType";
                var stageName = "MyStage";
                WcfEventSource.Log.TelemetryModuleExecutionStarted(typeName, stageName);
                CheckMessage(listener, WcfEventSource.TelemetryModuleExecutionStartedMessage, typeName, stageName);
            }
        }

        [TestMethod]
        public void TelemetryModuleExecutionStopped_Message()
        {
            using (var listener = new WcfEventListener())
            {
                var typeName = "MyType";
                var stageName = "MyStage";
                WcfEventSource.Log.TelemetryModuleExecutionStopped(typeName, stageName);
                CheckMessage(listener, WcfEventSource.TelemetryModuleExecutionStoppedMessage, typeName, stageName);
            }
        }

        [TestMethod]
        public void TelemetryModuleExecutionFailed_Message()
        {
            using (var listener = new WcfEventListener())
            {
                var typeName = "MyType";
                var stageName = "MyStage";
                var exception = "MyException";
                WcfEventSource.Log.TelemetryModuleExecutionFailed(typeName, stageName, exception);
                CheckMessage(listener, WcfEventSource.TelemetryModuleExecutionFailedMessage, typeName, stageName, exception);
            }
        }

        [TestMethod]
        public void NoOperationContextFound_Message()
        {
            using (var listener = new WcfEventListener())
            {
                WcfEventSource.Log.NoOperationContextFound();
                CheckMessage(listener, WcfEventSource.NoOperationContextFoundMessage);
            }
        }

        [TestMethod]
        public void OperationIgnored_Message()
        {
            using (var listener = new WcfEventListener())
            {
                var contractName = "MyContract";
                var contractNamespace = "MyNS";
                var operationName = "MyOperation";
                WcfEventSource.Log.OperationIgnored(contractName, contractNamespace, operationName);
                CheckMessage(listener, WcfEventSource.OperationIgnoredMessage, contractName, contractNamespace, operationName);
            }
        }

        [TestMethod]
        public void WcfTelemetryInitializerLoaded_Message()
        {
            using (var listener = new WcfEventListener())
            {
                var typeName = "MyType";
                WcfEventSource.Log.WcfTelemetryInitializerLoaded(typeName);
                CheckMessage(listener, WcfEventSource.WcfTelemetryInitializerLoadedMessage, typeName);
            }
        }

        [TestMethod]
        public void LocationIdSet_Message()
        {
            using (var listener = new WcfEventListener())
            {
                var ip = "10.0.0.1";
                WcfEventSource.Log.LocationIdSet(ip);
                CheckMessage(listener, WcfEventSource.LocationIdSetMessage, ip);
            }
        }

        [TestMethod]
        public void OperationContextCreated_Message()
        {
            using (var listener = new WcfEventListener())
            {
                var operationId = "abcdef";
                var ownsContext = true;
                WcfEventSource.Log.OperationContextCreated(operationId, ownsContext);
                CheckMessage(listener, WcfEventSource.OperationContextCreatedMessage, operationId, ownsContext);
            }
        }

        [TestMethod]
        public void RequestMessageClosed_Message()
        {
            using (var listener = new WcfEventListener())
            {
                var action = "reading property";
                var argument = "Myproperty";
                WcfEventSource.Log.RequestMessageClosed(action, argument);
                CheckMessage(listener, WcfEventSource.RequestMessageClosedMessage, action, argument);
            }
        }

        [TestMethod]
        public void ResponseMessageClosed_Message()
        {
            using (var listener = new WcfEventListener())
            {
                var action = "reading property";
                var argument = "Myproperty";
                WcfEventSource.Log.ResponseMessageClosed(action, argument);
                CheckMessage(listener, WcfEventSource.ResponseMessageClosedMessage, action, argument);
            }
        }

        private static void CheckMessage(WcfEventListener listener, string format, params object[] args)
        {
            Assert.AreEqual(string.Format(CultureInfo.CurrentCulture, format, args), listener.FirstEventMessage);
        }

        private class WcfEventListener : EventListener
        {
            private object lockObj;

            public WcfEventListener()
            {
                this.lockObj = new object();
                this.AllMessages = new List<string>();
                this.EnableEvents(WcfEventSource.Log, EventLevel.Verbose);
            }

            public string FirstEventMessage { get; private set; }

            public List<string> AllMessages { get; private set; }

            protected override void OnEventWritten(EventWrittenEventArgs eventData)
            {
                lock (this.lockObj)
                {
                    var str = string.Format(CultureInfo.CurrentCulture, eventData.Message, eventData.Payload.ToArray());
                    this.AllMessages.Add(str);
                    if (string.IsNullOrEmpty(this.FirstEventMessage))
                    {
                        this.FirstEventMessage = str;
                    }
                }
            }
        }
    }
}
