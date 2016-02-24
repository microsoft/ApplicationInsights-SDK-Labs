using System;
using System.Collections.Generic;
using System.ServiceModel.Description;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class ContractFilter
    {
        private Dictionary<String, OperationFilter> filters;

        public ContractFilter(IEnumerable<ContractDescription> contracts)
        {
            filters = new Dictionary<String, OperationFilter>();
            foreach ( var contract in contracts )
            {
                String key = GetKeyForContract(contract.Name, contract.Namespace);
                if ( !filters.ContainsKey(key) )
                {
                    filters[key] = new OperationFilter(contract);
                }
            }
        }

        public bool ShouldProcess(String contractName, String contractNamespace, String operationName)
        {
            String key = GetKeyForContract(contractName, contractNamespace);

            OperationFilter filter = null;
            if ( filters.TryGetValue(key, out filter) )
            {
                return filter.ShouldProcess(operationName);
            }
            // if unknown contract, err on the safe side
            return true;
        }

        // TODO: Avoid string concatenation to avoid extra allocation
        private String GetKeyForContract(String contractName, String contractNamespace)
        {
            return contractNamespace + '#' + contractName;
        }
    }
}
