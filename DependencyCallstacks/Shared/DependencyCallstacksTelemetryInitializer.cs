namespace Microsoft.ApplicationInsights.Extensibility
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// Telemetry initializer that extends the Application Insights .NET SDK to collect call stack information for every dependency.
    /// </summary>
    public class DependencyCallstacksTelemetryInitializer : ITelemetryInitializer
    {
        private static readonly string CallStackIdentifier = "__MSCallStack";

        /// <summary>
        /// Adds call stack information to ITelemetry objects of type DependencyTelemetry.
        /// </summary>
        /// <param name="telemetry">ITelemetry object to be initialized.</param>
        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry != null && telemetry is DependencyTelemetry)
            {
                // Ensure this only happens the first time this telemetry is initialized
                // Only the first initialization will have useful call stacks most of the time
                if (!telemetry.Context.Properties.ContainsKey(CallStackIdentifier))
                {
                    telemetry.Context.Properties.Add(CallStackIdentifier, this.GetCallStack());
                }
            }
        }

        /// <summary>
        /// Collects the call stack at a given moment in time.
        /// </summary>
        /// <returns>A string representing the call stack in the form Function1,Class1,FileInfo1\nFunction2,Class2,FileInfo2\n...</returns>
        protected string GetCallStack()
        {
            StackTrace stackTrace = new StackTrace(true);
            StackFrame[] stackFrames = stackTrace.GetFrames();
            string stack = string.Empty;

            foreach (StackFrame stackFrame in stackFrames)
            {
                MethodBase method = stackFrame.GetMethod();

                if (method != null && method.ReflectedType != null)
                {
                    string function = method.Name;
                    string className = method.ReflectedType.FullName;
                    string fileName = stackFrame.GetFileName();
                    string fileInfo = (fileName != null) ? string.Format(CultureInfo.InvariantCulture, "{0}:{1}", fileName.Split('\\').Last(), stackFrame.GetFileLineNumber()) : string.Empty;
                    bool isOwnCode = !(className.StartsWith("Microsoft.", StringComparison.Ordinal) || className.StartsWith("System.", StringComparison.Ordinal));

                    // This data gets stored in a custom property. The length of data stored in a custom property is limited.
                    // We save space by only sending the last part of the namespace (the class name).
                    className = className.Split('.').Last();

                    // We save even more space by only sending the "Just My Code" version of the call stack.
                    if (isOwnCode)
                    {
                        stack += string.Format(CultureInfo.InvariantCulture, "{0},{1},{2}\n", function, className, fileInfo);
                    }
                }
            }

            return stack;
        }
    }
}
