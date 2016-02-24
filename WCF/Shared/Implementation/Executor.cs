using System;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal static class Executor
    {
        public static void ExceptionSafe<TCtxt, TValue>(String moduleName, String activityName, Action<TCtxt, TValue> action, TCtxt context, TValue value)
        {
            WcfEventSource.Log.TelemetryModuleExecutionStarted(moduleName, activityName);
            try
            {
                action(context, value);
                WcfEventSource.Log.TelemetryModuleExecutionStopped(moduleName, activityName);
            } catch ( Exception ex )
            {
                WcfEventSource.Log.TelemetryModuleExecutionFailed(moduleName, activityName, ex.ToString());
            }
        }

        public static void ExceptionSafe<TCtxt, TValue1, TValue2>(String moduleName, String activityName, Action<TCtxt, TValue1, TValue2> action, TCtxt context, TValue1 value1, TValue2 value2)
        {
            WcfEventSource.Log.TelemetryModuleExecutionStarted(moduleName, activityName);
            try
            {
                action(context, value1, value2);
                WcfEventSource.Log.TelemetryModuleExecutionStopped(moduleName, activityName);
            } catch ( Exception ex )
            {
                WcfEventSource.Log.TelemetryModuleExecutionFailed(moduleName, activityName, ex.ToString());
            }
        }
    }
}
