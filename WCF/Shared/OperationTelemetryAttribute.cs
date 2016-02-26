using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Marks an OperationContract as an instrumented method
    /// </summary>
    /// <remarks>
    /// By default, when a WCF service has been instrumented through
    /// the <c ref='ServiceTelemetryAttribute' />, requests
    /// to any service operation will be tracked.
    ///
    /// However, in some cases you might want finer control and only
    /// record telemetry data for requests to certain operations.
    ///
    /// You can use the [OperationTelemetry] attribute on an operation contract method
    /// (that is, in the service contract interface, not on the service implementation)
    /// to tell Application Insights to only record telemetry for this method.
    ///
    /// Any operation contract methods without an [OperationTelemetry] attribute will be ignored
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class OperationTelemetryAttribute : Attribute, IOperationBehavior
    {
        void IOperationBehavior.AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        void IOperationBehavior.ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        void IOperationBehavior.ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
        }

        void IOperationBehavior.Validate(OperationDescription operationDescription)
        {
        }
    }
}
