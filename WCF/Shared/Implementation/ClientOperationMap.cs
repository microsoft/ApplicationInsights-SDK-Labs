using System;
using System.Collections.Generic;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal class ClientOperationMap
    {
        private IDictionary<String, ClientOpDescription> dictionary;

        public ClientOperationMap(IEnumerable<ClientOpDescription> operations)
        {
            dictionary = new Dictionary<String, ClientOpDescription>();
            foreach ( var op in operations )
            {
                dictionary.Add(op.Action, op);
            }
        }

        public bool TryLookupByAction(String soapAction, out ClientOpDescription operation)
        {
            return dictionary.TryGetValue(soapAction, out operation);
        }
    }
}
