namespace vent
{
    using System.Globalization;
    using System.IO;
    using System.Threading.Tasks;
    using AI;
    using E2ETests.Helpers;
    using Microsoft.AspNetCore.Http;

    public class RequestCultureMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestCultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var reader = new StreamReader(context.Request.Body);

            var items = TelemetryItemFactory.GetTelemetryItems(reader.ReadToEnd());

            var metric = ((AI.TelemetryItem<AI.MetricData>)(items[0])).data;
            

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Hello " + metric.baseData.metrics[0].name);
        }
    }
}