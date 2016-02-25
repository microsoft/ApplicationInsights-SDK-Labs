using System;

namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    internal static class Executor
    {
        public static void ExceptionSafe<TCtxt>(String moduleName, String activityName, Action<TCtxt> action, TCtxt context)
        {
            WcfEventSource.Log.TelemetryModuleExecutionStarted(moduleName, activityName);
            try
            {
                action(context);
                WcfEventSource.Log.TelemetryModuleExecutionStopped(moduleName, activityName);
            } catch ( Exception ex )
            {
                WcfEventSource.Log.TelemetryModuleExecutionFailed(moduleName, activityName, ex.ToString());
            }
        }
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
    }
}
