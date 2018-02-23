namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.ServiceModel.Description;

    internal class OperationFilter
    {
        private HashSet<string> instrumentedOperations;

        public OperationFilter(ContractDescription serviceContract)
        {
            this.instrumentedOperations = this.InspectContract(serviceContract);
        }

        public bool ShouldProcess(string operationName)
        {
            // if no operations had [OperationTelemetry], all are instrumented
            if (this.instrumentedOperations == null || this.instrumentedOperations.Count == 0)
            {
                return true;
            }

            return this.instrumentedOperations.Contains(operationName);
        }

        private HashSet<string> InspectContract(ContractDescription serviceContract)
        {
            HashSet<string> result = null;
            foreach (var op in serviceContract.Operations)
            {
                if (this.ShouldInstrument(op))
                {
                    if (result == null)
                    {
                        result = new HashSet<string>();
                    }

                    result.Add(op.Name);
                }
            }

            return result;
        }

        private bool ShouldInstrument(OperationDescription op)
        {
            OperationTelemetryAttribute behavior = null;
            behavior = op.OperationBehaviors
                         .OfType<OperationTelemetryAttribute>()
                         .FirstOrDefault();
            return behavior != null;
        }
    }
}
