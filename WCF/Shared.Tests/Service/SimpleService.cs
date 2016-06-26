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

        public void SampleOperation()
        {
        }

        public void CatchAllOperation()
        {
        }
    }

    public class TypedFault
    {
        public String Name { get; set; }
    }
}
