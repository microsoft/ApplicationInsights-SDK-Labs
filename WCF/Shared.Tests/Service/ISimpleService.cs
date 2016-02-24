using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Wcf.Tests.Service
{
    [ServiceContract]
    public interface ISimpleService
    {
        [OperationContract]
        String GetSimpleData();
        [OperationContract]
        void CallFailsWithFault();
        [OperationContract]
        void CallFailsWithTypedFault();
        [OperationContract]
        void CallFailsWithException();
    }
}
