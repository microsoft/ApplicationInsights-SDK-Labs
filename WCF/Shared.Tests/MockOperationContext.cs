using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.ApplicationInsights.Wcf.Tests
{
    public class MockOperationContext : IOperationContext
    {
        public IDictionary<String, object> IncomingProperties { get; private set; }
        public IDictionary<String, object> OutgoingProperties { get; private set; }
        public String OperationId { get; set; }
        public Uri EndpointUri { get; set; }
        public String OperationName { get; set; }
        public String ContractName { get; set; }
        public String ContractNamespace { get; set; }
        public ServiceSecurityContext SecurityContext { get; set; }

        public MockOperationContext()
        {
            this.IncomingProperties = new Dictionary<String, object>();
            this.OutgoingProperties = new Dictionary<String, object>();
            this.OperationId = Guid.NewGuid().ToString();
            this.ContractName = "IFakeService";
            this.ContractNamespace = "urn:fake";
        }

        public bool HasIncomingMessageProperty(string propertyName)
        {
            return IncomingProperties.ContainsKey(propertyName);
        }

        public object GetIncomingMessageProperty(string propertyName)
        {
            return IncomingProperties[propertyName];
        }

        public bool HasOutgoingMessageProperty(String propertyName)
        {
            return OutgoingProperties.ContainsKey(propertyName);
        }
        public object GetOutgoingMessageProperty(String propertyName)
        {
            return OutgoingProperties[propertyName];
        }
    }
}