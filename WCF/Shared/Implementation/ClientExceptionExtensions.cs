using System;
using System.ServiceModel;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal static class ClientExceptionExtensions
    {
        public static String ToResultCode(this Exception exception)
        {
            if ( exception == null )
            {
                throw new ArgumentNullException(nameof(exception));
            }
            if ( exception is TimeoutException )
            {
                return "Timeout";
            }
            if ( exception is EndpointNotFoundException )
            {
                return "EndpointNotFound";
            }
            if ( exception is ServerTooBusyException )
            {
                return "ServerTooBusy";
            }
            if ( exception is FaultException )
            {
                return "SoapFault";
            }
            return exception.GetType().Name;
        }
    }
}
