namespace IISLogSender
{
    using System;
    using System.Globalization;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;

    internal static class App
    {
        internal static readonly TelemetryClient Telemetry = new TelemetryClient();

        internal static void WriteOutput(string message, params object[] parameters)
        {
            WriteOutput(SeverityLevel.Information, message, parameters);
        }

        internal static void WriteOutput(SeverityLevel severityLevel, string message, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                message = string.Format(CultureInfo.InvariantCulture, message, parameters);
            }

            Telemetry.TrackTrace(message, severityLevel);

            if (severityLevel != SeverityLevel.Information)
            {
                message = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", severityLevel.ToString(), message);
            }

            Console.WriteLine(message);
        }
    }
}
