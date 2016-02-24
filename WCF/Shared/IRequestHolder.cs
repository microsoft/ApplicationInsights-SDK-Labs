using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Represents an in-flight request to the service
    /// </summary>
    public interface IRequestHolder
    {
        /// <summary>
        /// Date and time the request was received
        /// </summary>
        DateTimeOffset StartedAt { get; }
        /// <summary>
        /// The RequestTelemetry object associated with the request
        /// </summary>
        RequestTelemetry Request { get; }
        /// <summary>
        /// Starts the request timer
        /// </summary>
        void Start();
        /// <summary>
        /// Stops the request timer
        /// </summary>
        /// <returns>The elapsed time</returns>
        TimeSpan Stop();

    }
}
