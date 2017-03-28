namespace Microsoft.ApplicationInsights.Wcf
{
    using System;
    using System.ServiceModel;
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// Represents the context of the currently executing
    /// operation. Wraps the underlying
    /// <see cref="System.ServiceModel.OperationContext">OperationContext</see>.
    /// </summary>
    public interface IOperationContext
    {
        /// <summary>
        /// Gets the ID assigned to this request.
        /// </summary>
        string OperationId { get; }

        /// <summary>
        /// Gets the URI of the service endpoint.
        /// </summary>
        Uri EndpointUri { get; }

        /// <summary>
        /// Gets the URI the message was addressed to.
        /// </summary>
        Uri ToHeader { get; }

        /// <summary>
        /// Gets the RequestTelemetry event.
        /// </summary>
        RequestTelemetry Request { get; }

        /// <summary>
        /// Gets a value indicating whether WCF owns the Request telemetry object.
        /// </summary>
        bool OwnsRequest { get; }

        /// <summary>
        /// Gets the name of the service contract being invoked.
        /// </summary>
        string ContractName { get; }

        /// <summary>
        /// Gets the namespace of the service contract being invoked.
        /// </summary>
        string ContractNamespace { get; }

        /// <summary>
        /// Gets the name of the operation being invoked.
        /// </summary>
        string OperationName { get; }

        /// <summary>
        /// Gets the service security context.
        /// </summary>
        ServiceSecurityContext SecurityContext { get; }

        /// <summary>
        /// Checks if the incoming message has a given property.
        /// </summary>
        /// <param name="propertyName">The name of the property to be checked.</param>
        /// <returns>True if the property exists; false otherwise.</returns>
        bool HasIncomingMessageProperty(string propertyName);

        /// <summary>
        /// Returns the value of the given property in the incoming message.
        /// </summary>
        /// <param name="propertyName">The name of the property to be checked.</param>
        /// <returns>The property value.</returns>
        object GetIncomingMessageProperty(string propertyName);

        /// <summary>
        /// Checks if the outgoing message has a given property.
        /// </summary>
        /// <param name="propertyName">The name of the property to be checked.</param>
        /// <returns>True if the property exists; false otherwise.</returns>
        bool HasOutgoingMessageProperty(string propertyName);

        /// <summary>
        /// Returns the value of the given property in the outgoing message.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property value.</returns>
        object GetOutgoingMessageProperty(string propertyName);

        /// <summary>
        /// Returns the specified SOAP header on the request message.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="ns">The header XML namespace.</param>
        /// <returns>The header, or null if it is not present.</returns>
        T GetIncomingMessageHeader<T>(string name, string ns);
    }
}
