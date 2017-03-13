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
using System.Globalization;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    [TestClass]
    public class WcfEventSourceTests
    {
        private void CheckMessage(WcfEventListener listener, String format, params object[] args)
        {
            Assert.AreEqual(String.Format(CultureInfo.CurrentCulture, format, args), listener.FirstEventMessage);

        }
        [TestMethod]
        public void InitializationFailure_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var msg = "Exception message";
                WcfEventSource.Log.InitializationFailure(msg);
                CheckMessage(listener, WcfEventSource.InitializationFailure_Message, msg);
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
                CheckMessage(listener, WcfEventSource.TelemetryModuleExecutionStarted_Message, typeName, stageName);
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
                CheckMessage(listener, WcfEventSource.TelemetryModuleExecutionStopped_Message, typeName, stageName);
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
                CheckMessage(listener, WcfEventSource.TelemetryModuleExecutionFailed_Message, typeName, stageName, exception);
            }
        }

        [TestMethod]
        public void NoOperationContextFound_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                WcfEventSource.Log.NoOperationContextFound();
                CheckMessage(listener, WcfEventSource.NoOperationContextFound_Message);
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
                CheckMessage(listener, WcfEventSource.OperationIgnored_Message, contractName, contractNamespace, operationName);
            }
        }

        [TestMethod]
        public void WcfTelemetryInitializerLoaded_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var typeName = "MyType";
                WcfEventSource.Log.WcfTelemetryInitializerLoaded(typeName);
                CheckMessage(listener, WcfEventSource.WcfTelemetryInitializerLoaded_Message, typeName);
            }
        }

        [TestMethod]
        public void LocationIdSet_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var ip = "10.0.0.1";
                WcfEventSource.Log.LocationIdSet(ip);
                CheckMessage(listener, WcfEventSource.LocationIdSet_Message, ip);
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
                CheckMessage(listener, WcfEventSource.OperationContextCreated_Message, operationId, ownsContext);
            }
        }

        [TestMethod]
        public void RequestMessageClosed_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var action = "reading property";
                var argument = "Myproperty";
                WcfEventSource.Log.RequestMessageClosed(action, argument);
                CheckMessage(listener, WcfEventSource.RequestMessageClosed_Message, action, argument);
            }
        }

        [TestMethod]
        public void ResponseMessageClosed_Message()
        {
            using ( var listener = new WcfEventListener() )
            {
                var action = "reading property";
                var argument = "Myproperty";
                WcfEventSource.Log.ResponseMessageClosed(action, argument);
                CheckMessage(listener, WcfEventSource.ResponseMessageClosed_Message, action, argument);
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
                    var str = String.Format(CultureInfo.CurrentCulture, eventData.Message, eventData.Payload.ToArray());
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
