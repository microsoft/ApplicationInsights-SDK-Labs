namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel.Channels;

    internal static class WcfExtensions
    {
        public static HttpRequestMessageProperty GetHttpRequestHeaders(this IOperationContext operation)
        {
            if (operation.HasIncomingMessageProperty(HttpRequestMessageProperty.Name))
            {
                return (HttpRequestMessageProperty)operation.GetIncomingMessageProperty(HttpRequestMessageProperty.Name);
            }

            return null;
        }

        public static HttpResponseMessageProperty GetHttpResponseHeaders(this IOperationContext operation)
        {
            if (operation.HasOutgoingMessageProperty(HttpResponseMessageProperty.Name))
            {
                return (HttpResponseMessageProperty)operation.GetOutgoingMessageProperty(HttpResponseMessageProperty.Name);
            }

            return null;
        }

        public static HttpRequestMessageProperty GetHttpRequestHeaders(this Message message)
        {
            HttpRequestMessageProperty headers = null;
            if (message.Properties.ContainsKey(HttpRequestMessageProperty.Name))
            {
                headers = (HttpRequestMessageProperty)message.Properties[HttpRequestMessageProperty.Name];
            }
            else
            {
                headers = new HttpRequestMessageProperty();
                message.Properties.Add(HttpRequestMessageProperty.Name, headers);
            }

            return headers;
        }

        public static bool IsClosed(this Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            return message.State == MessageState.Closed;
        }
    }
}
