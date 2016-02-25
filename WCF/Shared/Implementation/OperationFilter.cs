using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Description;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    class OperationFilter
    {
        private HashSet<String> instrumentedOperations;

        public OperationFilter(ContractDescription serviceContract)
        {
            instrumentedOperations = InspectContract(serviceContract);
        }

        public bool ShouldProcess(String operationName)
        {
            // if no operations had [OperationTelemetry], all are instrumented
            if ( instrumentedOperations == null || instrumentedOperations.Count == 0 )
                return true;
            return instrumentedOperations.Contains(operationName);
        }

        private HashSet<String> InspectContract(ContractDescription serviceContract)
        {
            HashSet<String> result = null;
            foreach ( var op in serviceContract.Operations )
            {
                if ( ShouldInstrument(op) )
                {
                    if ( result == null )
                        result = new HashSet<String>();
                    result.Add(op.Name);
                }
            }
            return result;
        }

        private bool ShouldInstrument(OperationDescription op)
        {
            OperationTelemetryAttribute behavior = null;
#if NET40
            behavior = op.Behaviors.Find<OperationTelemetryAttribute>();

#else
            behavior = op.OperationBehaviors
                         .OfType<OperationTelemetryAttribute>()
                         .FirstOrDefault();
#endif // NET40
            return behavior != null;
        }
    }
}
