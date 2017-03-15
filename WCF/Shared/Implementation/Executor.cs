namespace Microsoft.ApplicationInsights.Wcf.Implementation
{
    using System;

    internal static class Executor
    {
        public delegate void LogAction<TCtxt, TParam>(TCtxt ctxt, ref TParam param);

        public static void ExceptionSafe<TCtxt>(string moduleName, string activityName, Action<TCtxt> action, TCtxt context)
        {
            WcfEventSource.Log.TelemetryModuleExecutionStarted(moduleName, activityName);
            try
            {
                action(context);
                WcfEventSource.Log.TelemetryModuleExecutionStopped(moduleName, activityName);
            }
            catch (Exception ex)
            {
                WcfEventSource.Log.TelemetryModuleExecutionFailed(moduleName, activityName, ex.ToString());
            }
        }

        public static void ExceptionSafe<TCtxt, TParam>(string moduleName, string activityName, LogAction<TCtxt, TParam> action, TCtxt context, ref TParam param)
        {
            WcfEventSource.Log.TelemetryModuleExecutionStarted(moduleName, activityName);
            try
            {
                action(context, ref param);
                WcfEventSource.Log.TelemetryModuleExecutionStopped(moduleName, activityName);
            }
            catch (Exception ex)
            {
                WcfEventSource.Log.TelemetryModuleExecutionFailed(moduleName, activityName, ex.ToString());
            }
        }

        public static void ExceptionSafe<TCtxt, TValue>(string moduleName, string activityName, Action<TCtxt, TValue> action, TCtxt context, TValue value)
        {
            WcfEventSource.Log.TelemetryModuleExecutionStarted(moduleName, activityName);
            try
            {
                action(context, value);
                WcfEventSource.Log.TelemetryModuleExecutionStopped(moduleName, activityName);
            }
            catch (Exception ex)
            {
                WcfEventSource.Log.TelemetryModuleExecutionFailed(moduleName, activityName, ex.ToString());
            }
        }
    }
}
