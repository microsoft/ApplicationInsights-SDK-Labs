using System;
using System.ServiceModel;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    [ServiceTelemetry]
    public class SimpleService : ISimpleService, ISimpleService2
    {
        public String GetSimpleData()
        {
            return "Hello world";
        }

        public String OtherGetSimpleData()
        {
            return "Hello World";
        }
        public void CallFailsWithFault()
        {
            throw new FaultException();
        }
        public void CallFailsWithTypedFault()
        {
            throw new FaultException<TypedFault>(
                new TypedFault { Name = "Hello" },
                "Call failed with typed fault"
                );
        }
        public void CallFailsWithException()
        {
            throw new InvalidOperationException();
        }
        public void CallWritesExceptionEvent()
        {
            try
            {
                throw new InvalidOperationException("Some exception");
            } catch ( Exception ex )
            {
                TelemetryClient client = new TelemetryClient();
                client.TrackException(ex);
            }
        }
        public void CallMarksRequestAsFailed()
        {
            var request = OperationContext.Current.GetRequestTelemetry();
            request.Success = false;
        }

        public void SampleOperation()
        {
        }

        public void CatchAllOperation()
        {
        }
        public void CallThatEmitsEvent()
        {
            TelemetryClient client = new TelemetryClient();
            client.TrackEvent("MyCustomEvent");
        }

        public void CallAnotherServiceAndLeakOperationContext(String address)
        {
            var factory = new ChannelFactory<ISimpleService>(new NetTcpBinding(), new EndpointAddress(address));
            var channel = factory.CreateChannel();
            // THIS IS INCORRECT CODE
            // The scope will be leaked, meaning that OperationContext.Current
            // will return the wrong scope later on.
            // We want to reproduce that behavior here so that it breaks
            // and we can check that the problem is fixed.
            var scope = new OperationContextScope((IContextChannel)channel);
            //using ( scope )
            {
                channel.GetSimpleData();
                ((IClientChannel)channel).Close();
            }
            factory.Close();
        }

        public bool CallIsClientSideContext()
        {
            return OperationContext.Current.IsClientSideContext();
        }
    }

    public class TypedFault
    {
        public String Name { get; set; }
    }
}
