namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;
    using System.ServiceModel;

    internal static class ClientExceptionExtensions
    {
        public static string ToResultCode(this Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (exception is TimeoutException)
            {
                return "Timeout";
            }

            if (exception is EndpointNotFoundException)
            {
                return "EndpointNotFound";
            }

            if (exception is ServerTooBusyException)
            {
                return "ServerTooBusy";
            }

            if (exception is FaultException)
            {
                return "SoapFault";
            }

            return exception.GetType().Name;
        }
    }
}
