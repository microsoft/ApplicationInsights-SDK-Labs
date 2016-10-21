using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.ServiceModel;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Represents the context of the currently executing
    /// operation. Wraps the underlying
    /// <c ref="System.ServiceModel.OperationContext">OperationContext</c>
    /// </summary>
    public interface IOperationContext
    {
        /// <summary>
        /// The ID assigned to this request
        /// </summary>
        String OperationId { get; }
        /// <summary>
        /// The URI of the service endpoint
        /// </summary>
        Uri EndpointUri { get; }
        /// <summary>
        /// The URI the message was addressed to 
        /// </summary>
        Uri ToHeader { get; }
        /// <summary>
        /// The RequestTelemetry event
        /// </summary>
        RequestTelemetry Request { get; }
        /// <summary>
        /// True if WCF owns the Request telemetry object
        /// </summary>
        bool OwnsRequest { get; }
        /// <summary>
        /// Name of the service contract being invoked
        /// </summary>
        String ContractName { get; }
        /// <summary>
        /// Namespace of the service contract being invoked
        /// </summary>
        String ContractNamespace { get; }
        /// <summary>
        /// The name of the operation being invoked
        /// </summary>
        String OperationName { get; }
        /// <summary>
        /// The service security context
        /// </summary>
        ServiceSecurityContext SecurityContext { get; }
        /// <summary>
        /// Checks if the incoming message has a given property
        /// </summary>
        /// <param name="propertyName">The name of the property to be checked</param>
        /// <returns>True if the property exists; false otherwise</returns>
        bool HasIncomingMessageProperty(String propertyName);
        /// <summary>
        /// Returns the value of the given property in the incoming message
        /// </summary>
        /// <param name="propertyName">The name of the property to be checked</param>
        /// <returns>The property value</returns>
        object GetIncomingMessageProperty(String propertyName);
        /// <summary>
        /// Checks if the outgoing message has a given property
        /// </summary>
        /// <param name="propertyName">The name of the property to be checked</param>
        /// <returns>True if the property exists; false otherwise</returns>
        bool HasOutgoingMessageProperty(String propertyName);
        /// <summary>
        /// Returns the value of the given property in the outgoing message
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <returns>The property value</returns>
        object GetOutgoingMessageProperty(String propertyName);
        /// <summary>
        /// Returns the specified SOAP header on the request message
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="ns">The header XML namespace</param>
        /// <returns>The header, or null if it is not present</returns>
        T GetIncomingMessageHeader<T>(String name, String ns);
    }
}
