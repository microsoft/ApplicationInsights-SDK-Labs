using System;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal static class WcfExtensions
    {
        public static HttpRequestMessageProperty GetHttpRequestHeaders(this IOperationContext operation)
        {
            if ( operation.HasIncomingMessageProperty(HttpRequestMessageProperty.Name) )
            {
                return (HttpRequestMessageProperty)operation.GetIncomingMessageProperty(HttpRequestMessageProperty.Name);
            }
            return null;
        }
        public static HttpResponseMessageProperty GetHttpResponseHeaders(this IOperationContext operation)
        {
            if ( operation.HasOutgoingMessageProperty(HttpResponseMessageProperty.Name) )
            {
                return (HttpResponseMessageProperty)operation.GetOutgoingMessageProperty(HttpResponseMessageProperty.Name);
            }
            return null;
        }
    }
}
