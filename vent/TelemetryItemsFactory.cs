namespace E2ETests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using AI;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    internal static class TelemetryItemsFactory
    {
        public static async Task<IList<Envelope>> GetTelemetryItems(Stream body)
        {
            var streamReader = new StreamReader(body);

            var items = new List<Envelope>();

            while (!streamReader.EndOfStream)
            {
                var line = await streamReader.ReadLineAsync();

                JsonReader reader = new JsonTextReader(new StringReader(line));
                reader.DateParseHandling = DateParseHandling.None;
                JObject obj = JObject.Load(reader);
                var envelope = obj.ToObject<Envelope>();

                var item = CreateTelemetryItem(envelope, line);
                items.Add(item);
            }

            return items;
        }

        private static Envelope CreateTelemetryItem(
            Envelope envelope,
            string content)
        {
            Envelope result;

            TelemetryItemType type;
            if (Enum.TryParse<TelemetryItemType>(envelope.data.baseType.Replace("Data", ""), out type))
            {
                switch (type)
                {
                    case TelemetryItemType.Exception:
                        {
                            result = JsonConvert.DeserializeObject<TelemetryItem<ExceptionData>>(content);
                            break;
                        }

                    case TelemetryItemType.Request:
                        {
                            result = JsonConvert.DeserializeObject<TelemetryItem<RequestData>>(content);
                            break;
                        }

                    case TelemetryItemType.Metric:
                        {
                            result = JsonConvert.DeserializeObject<TelemetryItem<MetricData>>(content);
                            break;
                        }

                    case TelemetryItemType.RemoteDependency:
                        {
                            result = JsonConvert.DeserializeObject<TelemetryItem<RemoteDependencyData>>(content);
                            break;
                        }

                    case TelemetryItemType.Message:
                        {
                            result = JsonConvert.DeserializeObject<TelemetryItem<MessageData>>(content);
                            break;
                        }

                    default:
                        {
                            throw new InvalidDataException("Unsupported telemetry type");
                        }
                }
            }
            else
            {
                throw new InvalidDataException("Unsupported telemetry type");
            }

            return result;
        }
    }
}    