using System;
using System.Collections.Generic;
using System.ServiceModel.Description;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal class ClientContract
    {
        private IDictionary<String, ClientOperation> dictionary;
        public Type ContractType { get; private set; }

        public ClientContract(Type contractType)
            : this(ContractDescription.GetContract(contractType))
        {
        }

        public ClientContract(ContractDescription description)
        {
            ContractType = description.ContractType;
            dictionary = new Dictionary<String, ClientOperation>();
            foreach ( var op in description.Operations )
            {
                var opDesc = new ClientOperation(op);
                dictionary.Add(opDesc.Action, opDesc);
            }
        }

        public bool TryLookupByAction(String soapAction, out ClientOperation operation)
        {
            return dictionary.TryGetValue(soapAction, out operation);
        }
    }
}
