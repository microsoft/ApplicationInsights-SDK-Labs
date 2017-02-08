using System;
using System.Collections.Generic;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal class ClientOperationMap
    {
        private IDictionary<String, ClientOperation> dictionary;

        public ClientOperationMap(IEnumerable<ClientOperation> operations)
        {
            dictionary = new Dictionary<String, ClientOperation>();
            foreach ( var op in operations )
            {
                dictionary.Add(op.Action, op);
            }
        }

        public ClientOperation LookupByAction(String soapAction)
        {
            return dictionary[soapAction];
        }
    }
}
