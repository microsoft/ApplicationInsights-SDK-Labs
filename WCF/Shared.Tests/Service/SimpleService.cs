using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
    }

    public class TypedFault
    {
        public String Name { get; set; }
    }
}
