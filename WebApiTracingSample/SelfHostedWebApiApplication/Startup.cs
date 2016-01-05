namespace SelfHostedWebApiApplication
{
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Sample;
    using Owin;
    using System.Web.Http;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            TelemetryConfiguration.Active.InstrumentationKey = "00001111-2222-3333-4444-555566667777";

            var config = new HttpConfiguration();

            config.EnableApplicationInsights();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            app.UseWebApi(config);
        }
    }
}