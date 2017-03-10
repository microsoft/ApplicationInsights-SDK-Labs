using Microsoft.ApplicationInsights.Wcf.Implementation;
#if NET40
using Microsoft.Diagnostics.Tracing;
#else
using System.Diagnostics.Tracing;
#endif
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class WcfEventSourceTests
    {
        [TestMethod]
        public void InitializationFailure_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var msg = "Exception message";
                WcfEventSource.Log.InitializationFailure(msg);
                Assert.AreEqual(String.Format(WcfEventSource.InitializationFailure_Message, msg), listener.FirstEventMessage);
            }
        }

        [TestMethod]
        public void TelemetryModuleExecutionStarted_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var typeName = "MyType";
                var stageName = "MyStage";
                WcfEventSource.Log.TelemetryModuleExecutionStarted(typeName, stageName);
                Assert.AreEqual(String.Format(WcfEventSource.TelemetryModuleExecutionStarted_Message, typeName, stageName), listener.FirstEventMessage);
            }
        }

        [TestMethod]
        public void TelemetryModuleExecutionStopped_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var typeName = "MyType";
                var stageName = "MyStage";
                WcfEventSource.Log.TelemetryModuleExecutionStopped(typeName, stageName);
                Assert.AreEqual(String.Format(WcfEventSource.TelemetryModuleExecutionStopped_Message, typeName, stageName), listener.FirstEventMessage);
            }
        }

        [TestMethod]
        public void TelemetryModuleExecutionFailed_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var typeName = "MyType";
                var stageName = "MyStage";
                var exception = "MyException";
                WcfEventSource.Log.TelemetryModuleExecutionFailed(typeName, stageName, exception);
                Assert.AreEqual(String.Format(WcfEventSource.TelemetryModuleExecutionFailed_Message, typeName, stageName, exception), listener.FirstEventMessage);
            }
        }

        [TestMethod]
        public void NoOperationContextFound_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                WcfEventSource.Log.NoOperationContextFound();
                Assert.AreEqual(WcfEventSource.NoOperationContextFound_Message, listener.FirstEventMessage);
            }
        }

        [TestMethod]
        public void OperationIgnored_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var contractName = "MyContract";
                var contractNamespace = "MyNS";
                var operationName = "MyOperation";
                WcfEventSource.Log.OperationIgnored(contractName, contractNamespace, operationName);
                Assert.AreEqual(String.Format(WcfEventSource.OperationIgnored_Message, contractName, contractNamespace, operationName), listener.FirstEventMessage);
            }
        }

        [TestMethod]
        public void WcfTelemetryInitializerLoaded_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var typeName = "MyType";
                WcfEventSource.Log.WcfTelemetryInitializerLoaded(typeName);
                Assert.AreEqual(String.Format(WcfEventSource.WcfTelemetryInitializerLoaded_Message, typeName), listener.FirstEventMessage);
            }
        }

        [TestMethod]
        public void LocationIdSet_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var ip = "10.0.0.1";
                WcfEventSource.Log.LocationIdSet(ip);
                Assert.AreEqual(String.Format(WcfEventSource.LocationIdSet_Message, ip), listener.FirstEventMessage);
            }
        }

        [TestMethod]
        public void OperationContextCreated_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var operationId = "abcdef";
                var ownsContext = true;
                WcfEventSource.Log.OperationContextCreated(operationId, ownsContext);
                Assert.AreEqual(String.Format(WcfEventSource.OperationContextCreated_Message, operationId, ownsContext), listener.FirstEventMessage);
            }
        }

        [TestMethod]
        public void RequestMessageClosed_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var property = "MyProperty";
                WcfEventSource.Log.RequestMessageClosed(property);
                Assert.AreEqual(String.Format(WcfEventSource.RequestMessageClosed_Message, property), listener.FirstEventMessage);
            }
        }

        [TestMethod]
        public void ResponseMessageClosed_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var property = "MyProperty";
                WcfEventSource.Log.ResponseMessageClosed(property);
                Assert.AreEqual(String.Format(WcfEventSource.ResponseMessageClosed_Message, property), listener.FirstEventMessage);
            }
        }

        class WcfEventListener : EventListener
        {
            public String FirstEventMessage { get; private set; }
            public List<String> AllMessages { get; private set; }
            private object lockObj;

            public WcfEventListener()
            {
                lockObj = new object();
                AllMessages = new List<string>();
                this.EnableEvents(WcfEventSource.Log, EventLevel.Verbose);
            }

            protected override void OnEventWritten(EventWrittenEventArgs eventData)
            {
                lock ( lockObj )
                {
                    var str = String.Format(eventData.Message, eventData.Payload.ToArray());
                    AllMessages.Add(str);
                    if ( String.IsNullOrEmpty(FirstEventMessage) )
                    {
                        FirstEventMessage = str;
                    }
                }
            }
        }
    }
}
