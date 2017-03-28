namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Description;

    internal class ClientContract
    {
        private IDictionary<string, ClientOperation> dictionary;

        public ClientContract(Type contractType)
            : this(ContractDescription.GetContract(contractType))
        {
        }

        public ClientContract(ContractDescription description)
        {
            this.ContractType = description.ContractType;
            this.dictionary = new Dictionary<string, ClientOperation>();
            foreach (var op in description.Operations)
            {
                var operationDesc = new ClientOperation(this.ContractType.Name, op);
                this.dictionary.Add(operationDesc.Action, operationDesc);
            }
        }

        public Type ContractType { get; private set; }

        public bool TryLookupByAction(string soapAction, out ClientOperation operation)
        {
            return this.dictionary.TryGetValue(soapAction, out operation);
        }
    }
}
