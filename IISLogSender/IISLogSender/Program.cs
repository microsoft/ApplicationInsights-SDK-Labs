namespace IISLogSender
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;

    class Program
    {
        private const int AgeLimitHours = 48;
        private const int RateLimitPerMinute = 20000;

        private const string HeaderDate = "#Date: ";
        private const string HeaderFields = "#Fields: ";
        private const char DelimiterChar = ' ';
        private const char EmptyChar = '-';
        private const string LastProcessedFileName = "last_processed.txt";

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
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            string logDir = args[0];

            if (string.IsNullOrEmpty(logDir) || !Directory.Exists(logDir))
            {
                Console.WriteLine("Error: Invalid log directory '{0}'.", logDir);
                return;
            }

            Console.WriteLine("Using log directory '{0}'.", logDir);

            IEnumerable<string> logFiles = Directory.EnumerateFiles(logDir, "*.log", SearchOption.AllDirectories);

            var now = DateTimeOffset.Now;
            DateTime oldestProcessingTime = now.UtcDateTime.AddHours(-AgeLimitHours).ToLocalTime();
            DateTime lastProcessedTime = now.UtcDateTime.AddHours(-AgeLimitHours);
            DateTime newestProcessingTime = new DateTime(now.UtcDateTime.Year, now.UtcDateTime.Month, now.UtcDateTime.Day, now.UtcDateTime.Hour, Math.Max(0, now.UtcDateTime.Minute - 5), 0, DateTimeKind.Utc);

            if (File.Exists(LastProcessedFileName))
            {
                string text = File.ReadAllText(LastProcessedFileName);
                lastProcessedTime = DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                oldestProcessingTime = lastProcessedTime.ToLocalTime();
                Console.WriteLine("'{0}' exists. Will process requests later than '{1}'.", LastProcessedFileName, lastProcessedTime);
            }

            TelemetryClient telemetryClient = new TelemetryClient();

            DateTime processedUntilUtc = DateTime.MinValue;

            int totalRequests = 0;
            int currentMinute = DateTime.Now.Minute;
            int currentMinuteRequests = 0;
            
            foreach (string logFile in logFiles)
            {
                Console.WriteLine("Processing log file '{0}'...", logFile);
                DateTime lastWriteTime = File.GetLastWriteTime(logFile);
                if (lastWriteTime < oldestProcessingTime)
                {
                    Console.WriteLine("Log is older '{0}' than '{1}'; skipping.", lastWriteTime, oldestProcessingTime);
                    continue;
                }

                try
                {
                    using (StreamReader reader = File.OpenText(logFile))
                    {
                        bool inHeader = true;
                        Dictionary<string, int> fieldIndicies = null;

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();

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
                                        Console.WriteLine("Log date: {0}.", line.Substring(HeaderDate.Length));
                                    }
                                    else if (line.StartsWith(HeaderFields, StringComparison.Ordinal))
                                    {
                                        fieldIndicies = GetFieldIndicies(line);
                                    }

                                    continue;
                                }
                            }

                            RequestTelemetry request = ProcessLogLine(lastProcessedTime, newestProcessingTime, fieldIndicies, line);
                            if (request != null)
                            {
                                telemetryClient.TrackRequest(request);
                                processedUntilUtc = request.Timestamp.UtcDateTime;

                                totalRequests++;
                                currentMinuteRequests++;

                                TrackSimpleThrottle(currentMinute, currentMinuteRequests);
                            }
                        }
                    }
                }
                catch (IOException ioex)
                {
                    Console.WriteLine("Failed to open '{0}': {1}.", logFile, ioex.Message);
                }
            }

            if (processedUntilUtc != DateTime.MinValue)
            {
                File.WriteAllText(LastProcessedFileName, newestProcessingTime.ToString());
                Console.WriteLine("Wrote '{0}' with last processed time of '{1}'", LastProcessedFileName, newestProcessingTime);
            }

            telemetryClient.Flush();
        }

        private static void TrackSimpleThrottle(int currentMinute, int currentMinuteRequests)
        {
            if (currentMinute != DateTime.Now.Minute)
            {
                currentMinute = DateTime.Now.Minute;
                currentMinuteRequests = 0;
            }

            if (currentMinuteRequests > RateLimitPerMinute)
            {
                Console.WriteLine("Reached rate limit of {0} items per minute. Waiting until the next minute...", RateLimitPerMinute);

                while (currentMinute == DateTime.Now.Minute)
                {
                    Task.Delay(5000).Wait();
                }

                Console.WriteLine("Resuming processing.");
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

        private static RequestTelemetry ProcessLogLine(DateTime lastProcessed, DateTime newestProcessingTime, Dictionary<string, int> fieldIncidies, string line)
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
            //string query = GetFieldValue(fieldIncidies, fields, "cs-uri-query");
            //string url = query != null ? name + query : name;
            //string ip = GetFieldValue(fieldIncidies, fields, "s-ip");
            //Uri baseUri = new Uri("contoso.com");
            //Uri uri = new Uri(baseUri, url);

            var request = new RequestTelemetry(name, timestamp, duration, responseCode, success)
            {
                 HttpMethod = method
            };

            request.Context.Location.Ip = GetFieldValue(fieldIncidies, fields, "c-ip");
            request.Context.User.Id = GetFieldValue(fieldIncidies, fields, "cs-username");
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
    }
}
