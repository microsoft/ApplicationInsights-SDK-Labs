namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel.Description;

    internal class ContractFilter
    {
        private Dictionary<string, OperationFilter> filters;

        public ContractFilter(IEnumerable<ContractDescription> contracts)
        {
            this.filters = new Dictionary<string, OperationFilter>();
            foreach (var contract in contracts)
            {
                var key = this.GetKeyForContract(contract.Name, contract.Namespace);
                if (!this.filters.ContainsKey(key))
                {
                    this.filters[key] = new OperationFilter(contract);
                }
            }
        }

        public bool ShouldProcess(string contractName, string contractNamespace, string operationName)
        {
            var key = this.GetKeyForContract(contractName, contractNamespace);

            OperationFilter filter = null;
            if (this.filters.TryGetValue(key, out filter))
            {
                return filter.ShouldProcess(operationName);
            }

            // if unknown contract, err on the safe side
            return true;
        }

        // TODO: Avoid string concatenation to avoid extra allocation
        private string GetKeyForContract(string contractName, string contractNamespace)
        {
            return contractNamespace + '#' + contractName;
        }
    }
}
