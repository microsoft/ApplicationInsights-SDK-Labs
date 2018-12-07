namespace Vent
{
    using E2ETests.Helpers;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using System.IO;
    using System.Threading.Tasks;

    public class ApplicationInsightsEventsParsingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TelemetryClient _client;

        public ApplicationInsightsEventsParsingMiddleware(RequestDelegate next, TelemetryClient client)
        {
            _next = next;
            _client = client;
        }

        public async Task Invoke(HttpContext context)
        {
            if ((context.Request.Method == "POST") && (context.Request.Path == "/v2/track"))
            {
                var items = await TelemetryItemsFactory.GetTelemetryItems(context.Request.Body);

                string result = null;

                foreach(var item in items)
                {
                    var telemetry = TelemetryFactory.ConvertTelemetryItemToITelemetry(item);

                    //TODO: fill out extra context of telemetry object

                    _client.Track(telemetry);

                    result = telemetry.Context.InstrumentationKey;

                }

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync("Hello " + result);
            }
        }
    }
}