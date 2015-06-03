namespace IISLogSender
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;

    /// <summary>
    /// IISLogSender class.
    /// </summary>
    public class IISLogSender
    {
#if DEBUG
        private const int AgeLimitHours = 1000;
#else
        private const int AgeLimitHours = 48;
#endif
        private const int TrackRateLimit = 5000;

        private const string HeaderDate = "#Date: ";
        private const string HeaderFields = "#Fields: ";
        private const char DelimiterChar = ' ';
        private const char EmptyChar = '-';
        private const string LastProcessedFileName = "last_processed.txt";

        private DateTime lastFlushTime;
        private int numberOfTrackCalls;

        /// <summary>
        /// Constructs an instance of the IISLogSender class.
        /// </summary>
        public IISLogSender()
        {
        }

        /// <summary>
        /// Processes all logs in the given log directory and sends requests to Application Insights.
        /// </summary>
        public void ProcessLogs(string logDir)
        {
            this.ResetThrottling();

            IList<string> logFiles = Directory.EnumerateFiles(logDir, "*.log", SearchOption.AllDirectories).ToList();

            App.Telemetry.TrackMetric("LogFiles", logFiles.Count);
            App.WriteOutput("Found {0} log files.", logFiles.Count);

            var now = DateTimeOffset.Now;
            DateTime oldestProcessingTime = now.UtcDateTime.AddHours(-AgeLimitHours).ToLocalTime();
            DateTime lastProcessedTime = now.UtcDateTime.AddHours(-AgeLimitHours);
            DateTime newestProcessingTime = new DateTime(now.UtcDateTime.Year, now.UtcDateTime.Month, now.UtcDateTime.Day, now.UtcDateTime.Hour, Math.Max(0, now.UtcDateTime.Minute - 5), 0, DateTimeKind.Utc);

            if (File.Exists(LastProcessedFileName))
            {
                App.Telemetry.TrackEvent("LastProcessedFileExists");

                string text = File.ReadAllText(LastProcessedFileName);
                lastProcessedTime = DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                oldestProcessingTime = lastProcessedTime.ToLocalTime();
                App.WriteOutput("'{0}' exists. Will process requests later than '{1}'.", LastProcessedFileName, lastProcessedTime);
            }

            DateTime processedUntilUtc = DateTime.MinValue;

            int totalRequests = 0;
            
            foreach (string logFile in logFiles)
            {
                App.WriteOutput("Processing log file '{0}'...", logFile);
                DateTime lastWriteTime = File.GetLastWriteTime(logFile);
                if (lastWriteTime < oldestProcessingTime)
                {
                    App.WriteOutput("Log '{0}' is older than '{1}'; skipping.", lastWriteTime, oldestProcessingTime);
                    continue;
                }

                int currentLogRequests = 0;
                int currentLogTrackedRequests = 0;

                try
                {
                    using (StreamReader reader = File.OpenText(logFile))
                    {
                        bool inHeader = true;
                        Dictionary<string, int> fieldIndicies = null;
                        int lineNumber = 0;

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            lineNumber++;

                            if (line.Length == 0)
                            {
                                continue;
                            }

                            if (inHeader)
                            {
                                if (line[0] != '#')
                                {
                                    inHeader = false;
                                }
                                else
                                {
                                    if (line.StartsWith(HeaderDate, StringComparison.Ordinal))
                                    {
                                        App.WriteOutput("Log date: {0}.", line.Substring(HeaderDate.Length));
                                    }
                                    else if (line.StartsWith(HeaderFields, StringComparison.Ordinal))
                                    {
                                        fieldIndicies = GetFieldIndicies(line);
                                    }

                                    continue;
                                }
                            }

                            currentLogRequests++;

                            try
                            {
                                RequestTelemetry request = ProcessLogLine(lineNumber, lastProcessedTime, newestProcessingTime, fieldIndicies, line);
                                if (request != null)
                                {
                                    App.Telemetry.TrackRequest(request);
                                    numberOfTrackCalls++;

                                    if (request.Timestamp.UtcDateTime > processedUntilUtc)
                                    {
                                        processedUntilUtc = request.Timestamp.UtcDateTime;
                                    }

                                    currentLogTrackedRequests++;
                                    totalRequests++;

                                    TrackSimpleThrottle();
                                }
                            }
                            catch (Exception ex)
                            {
                                App.WriteOutput(SeverityLevel.Error, "Failed to process request {0} in log.", currentLogRequests);
                                var properties = new Dictionary<string, string>();
                                properties.Add("LogFile", logFile);
                                properties.Add("LogRequest", currentLogRequests.ToString(CultureInfo.InvariantCulture));
                                App.Telemetry.TrackException(ex, properties);
                            }
                        }
                    }
                }
                catch (IOException ioex)
                {
                    App.WriteOutput(SeverityLevel.Error, "Failed to open '{0}': {1}.", logFile, ioex.Message);
                }

                App.WriteOutput("Tracked {0} of {1} items.", currentLogTrackedRequests, currentLogRequests);
            }

            if (processedUntilUtc != DateTime.MinValue)
            {
                File.WriteAllText(LastProcessedFileName, newestProcessingTime.ToString());
                App.WriteOutput("Wrote '{0}' with last processed time of '{1}'", LastProcessedFileName, newestProcessingTime);
            }
        }

        private static Dictionary<string, int> GetFieldIndicies(string line)
        {
            string[] fields = line.Substring(HeaderFields.Length).Split(DelimiterChar);
            var result = new Dictionary<string, int>(fields.Length);

            for (int i = 0; i < fields.Length; i++)
            {
                result.Add(fields[i], i);
            }

            return result;
        }

        private static RequestTelemetry ProcessLogLine(int lineNumber, DateTime lastProcessed, DateTime newestProcessingTime, Dictionary<string, int> fieldIncidies, string line)
        {
            string[] fields = line.Split(DelimiterChar);

            DateTimeOffset timestamp = GetTimeStamp(GetFieldValue(fieldIncidies, fields, "date"), GetFieldValue(fieldIncidies, fields, "time"));

            if (timestamp.UtcDateTime <= lastProcessed || timestamp.UtcDateTime > newestProcessingTime)
            {
                return null;
            }

            string method = GetFieldValue(fieldIncidies, fields, "cs-method");
            string name = GetFieldValue(fieldIncidies, fields, "cs-uri-stem");
            TimeSpan duration = GetTimeSpan(GetFieldValue(fieldIncidies, fields, "time-taken"));
            string responseCode = GetFieldValue(fieldIncidies, fields, "sc-status");
            bool success = responseCode.StartsWith("2", StringComparison.Ordinal);
            string query = GetFieldValue(fieldIncidies, fields, "cs-uri-query");
            string url = query != null ? name + query : name;
            Uri uri = new Uri(url, UriKind.RelativeOrAbsolute);

            var request = new RequestTelemetry(name, timestamp, duration, responseCode, success)
            {
                Id = Tuple.Create(lineNumber, line).GetHashCode().ToString(CultureInfo.InvariantCulture),
                HttpMethod = method,
                Url = uri
            };

            request.Context.Location.Ip = GetFieldValue(fieldIncidies, fields, "c-ip");
            request.Context.User.UserAgent = GetFieldValue(fieldIncidies, fields, "cs(User-Agent)");

            string referer = GetFieldValue(fieldIncidies, fields, "cs(Referer)");
            if (referer != null)
            {
                request.Properties.Add("Referer", referer);
            }

            return request;
        }

        private static string GetFieldValue(Dictionary<string, int> fieldIncidies, string[] fields, string fieldName)
        {
            int index;

            if (!fieldIncidies.TryGetValue(fieldName, out index))
            {
                return null;
            }

            if (index >= fields.Length)
            {
                return null;
            }

            string value = fields[index];

            if (value.Length == 1 && value[0] == EmptyChar)
            {
                return null;
            }

            return value;
        }

        private static DateTimeOffset GetTimeStamp(string date, string time)
        {
            return DateTime.Parse(date + " " + time, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        }

        private static TimeSpan GetTimeSpan(string timespanInMilliseconds)
        {
            int ms = int.Parse(timespanInMilliseconds, CultureInfo.InvariantCulture);

            return TimeSpan.FromMilliseconds(ms);
        }

        private void ResetThrottling()
        {
            App.WriteOutput("Resetting throttling...");
            this.lastFlushTime = DateTime.Now;
            this.numberOfTrackCalls = 0;
        }

        private void TrackSimpleThrottle()
        {
            if (numberOfTrackCalls > TrackRateLimit)
            {
                App.WriteOutput(SeverityLevel.Warning, "Reached limit of {0} items.", TrackRateLimit);

                App.Telemetry.Flush();
                this.ResetThrottling();

                App.WriteOutput(SeverityLevel.Verbose, "Waiting 30 seconds before resuming processing...");
                Task.Delay(30000).Wait();

                App.WriteOutput("Resuming processing.");
            }

            if (DateTime.Now > this.lastFlushTime.AddSeconds(30))
            {
                this.ResetThrottling();
            }
        }
    }
}
