namespace IISLogSender
{
    using System;
    using System.IO;
    using Microsoft.ApplicationInsights.DataContracts;

    class Program
    {
        private static void PrintUsage()
        {
            Console.WriteLine("IISLogSender - Send IIS logs to Application Insights");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("\tIISLogSender.exe <log directory>");
            Console.WriteLine(@"\tE.g. IISLogSender.exe C:\inetpub\logs");
            Console.WriteLine();
        }

        static void Main(string[] args)
        {
            App.Telemetry.TrackEvent("Main");

            try
            {
                if (args.Length == 0)
                {
                    PrintUsage();
                    return;
                }

                string logDir = args[0];

                if (string.IsNullOrEmpty(logDir) || !Directory.Exists(logDir))
                {
                    App.WriteOutput(SeverityLevel.Error, "Invalid log directory '{0}'.", logDir);
                    return;
                }

                App.WriteOutput("Using log directory '{0}'.", logDir);

                App.Telemetry.TrackEvent("CommandLine/ArgsValidated");

                var sender = new IISLogSender();
                sender.ProcessLogs(logDir);
            }
            catch (Exception ex)
            {
                App.Telemetry.TrackException(ex);
                Flush();
                throw;
            }

            Flush();

            App.WriteOutput("Done.");
        }

        private static void Flush()
        {
            App.WriteOutput("Flushing buffered items...");
            App.Telemetry.Flush();
        }
    }
}
