using System;
using System.ServiceModel.Channels;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal static class WcfExtensions
    {
        public static HttpRequestMessageProperty GetHttpRequestHeaders(this IOperationContext operation)
        {
            try
            {
                if ( operation.HasIncomingMessageProperty(HttpRequestMessageProperty.Name) )
                {
                    return (HttpRequestMessageProperty)operation.GetIncomingMessageProperty(HttpRequestMessageProperty.Name);
                }
            } catch ( ObjectDisposedException )
            {
                // WCF message is already disposed, just avoid it
            }
            return null;
        }
        public static HttpResponseMessageProperty GetHttpResponseHeaders(this IOperationContext operation)
        {
            try
            {
                if ( operation.HasOutgoingMessageProperty(HttpResponseMessageProperty.Name) )
                {
                    return (HttpResponseMessageProperty)operation.GetOutgoingMessageProperty(HttpResponseMessageProperty.Name);
                }
            } catch ( ObjectDisposedException )
            {
                // WCF message is already disposed, just avoid it
            }
            return null;
        }
    }
}
