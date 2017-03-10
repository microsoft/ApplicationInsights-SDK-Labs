using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.ApplicationInsights.Wcf
{
    /// <summary>
    /// Allows a TelemetryModule or TelemetryInitializer to store temporary
    /// state between request/response processing
    /// </summary>
    public interface IOperationContextState
    {
        /// <summary>
        /// Store a value in the context
        /// </summary>
        /// <param name="key">The key to store the state under</param>
        /// <param name="value">The value to store</param>
        void SetState(String key, object value);
        /// <summary>
        /// Retrieve a value from the context
        /// </summary>
        /// <typeparam name="T">The type of value</typeparam>
        /// <param name="key">The key the state is stored under</param>
        /// <param name="value">The value, if found</param>
        /// <returns>True if the specified key was found, false otherwise.</returns>
        bool TryGetState<T>(String key, out T value);

    }
}
